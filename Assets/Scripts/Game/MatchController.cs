using System;
using System.Collections;
using System.Collections.Generic;
using HitMe.Core;
using HitMe.Audio;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class MatchController : MonoBehaviour
    {
        public GameConfigAsset configAsset;
        public ArenaView arena;
        public HudView hud;
        public int localCharacterIndex = 0;

        [Tooltip("Automation only: lets the local player use BotBrain and lock every round immediately.")]
        public bool autoPlay;
        public MatchState State { get; private set; }
        public float SecondsRemaining { get; private set; }
        public bool CapturePhase1 { get; private set; }
        public Vector2 LocalPosition
        {
            get
            {
                var p = State == null ? null : State.FindPlayer("player");
                return p == null ? Vector2.zero : new Vector2(p.Position.X, p.Position.Y);
            }
        }

        public PlacementGestureMode GestureMode
        {
            get
            {
                if (_gestureV2 == null) return PlacementGestureMode.Idle;
                var s = _gestureV2.Snapshot();
                if (s.AimActive) return PlacementGestureMode.Aiming;
                if (s.HasPlaced) return PlacementGestureMode.Moving;
                return PlacementGestureMode.Idle;
            }
        }

        public bool HasGestureAim
        {
            get
            {
                return _gestureV2 != null && _gestureV2.Snapshot().AimActive;
            }
        }

        public Action<string> OnToastRequested;
        public Action<Phase> OnPhaseChanged;
        public Action<int, int> OnRoundStarted; // round, maxRounds
        public Action<float> OnTimerTick;
        public Action<List<PlayerState>, List<string>> OnMatchEnded;

        private const int MaxPoolSize = 6;
        private readonly List<PlayerView> _playerPool = new List<PlayerView>(MaxPoolSize);
        private readonly List<WeaponFlightView> _weaponPool = new List<WeaponFlightView>(MaxPoolSize);
        private readonly List<HitFx> _hitFxPool = new List<HitFx>(MaxPoolSize);
        private readonly Dictionary<string, PlayerView> _views = new Dictionary<string, PlayerView>(MaxPoolSize);
        private readonly Dictionary<string, Phase1BotBrain> _bots = new Dictionary<string, Phase1BotBrain>(MaxPoolSize);
        private readonly List<PlayerState> _rankedBuffer = new List<PlayerState>(MaxPoolSize);
        private readonly List<string> _winnerIdsBuffer = new List<string>(MaxPoolSize);

        private SeededRandom _random;
        private PlacementGestureV2 _gestureV2;
        private Coroutine _matchCoroutine;
        private int _lastTickSecond = -1;
        private bool _poolsInitialized;
        private bool _readyRequested;
        private uint _matchCounter;

        // Pre-allocated WaitForSeconds to avoid per-round GC allocations
        private readonly WaitForSeconds _waitLockNormal = new WaitForSeconds(0.05f);
        private readonly WaitForSeconds _waitLockCapture = new WaitForSeconds(0.25f);
        private readonly WaitForSeconds _waitReveal = new WaitForSeconds(0.8f);
        private readonly WaitForSeconds _waitThrowNormal = new WaitForSeconds(0.85f);
        private readonly WaitForSeconds _waitThrowCapture = new WaitForSeconds(1.0f);
        private readonly WaitForSeconds _waitResolveNormal = new WaitForSeconds(1.2f);
        private readonly WaitForSeconds _waitResolveCapture = new WaitForSeconds(0.02f);
        private readonly WaitForSeconds _waitFastAuto = new WaitForSeconds(0.05f);

        private int _webAutoPlayRemainingMatches;
        private int _webAutoPlayTotalMatches;
        private bool _fastAutoPlayTimings;
        private int _profileFrames;
        private long _profileStartBytes;
        private long _profilePositiveAllocBytes;
        private long _profilePrevBytes;
        private int _profileStartGcCount;
        private float _profileStartTime;

        /// <summary>
        /// Deterministic selection of up to 6 distinct characters from the 7-character roster.
        /// Slot 0 uses localCharacterIndex; remaining slots are shuffled using SeededRandom without duplicates.
        /// </summary>
        public static int[] SelectMatchRoster(int localCharIndex, int maxPlayers, SeededRandom random)
        {
            int count = Mathf.Clamp(maxPlayers, 1, PlayerView.TotalCharacterCount);
            int chosenLocal = ((localCharIndex % PlayerView.TotalCharacterCount) + PlayerView.TotalCharacterCount) % PlayerView.TotalCharacterCount;

            int[] remaining = new int[PlayerView.TotalCharacterCount - 1];
            int rIdx = 0;
            for (int c = 0; c < PlayerView.TotalCharacterCount; c++)
            {
                if (c == chosenLocal) continue;
                remaining[rIdx++] = c;
            }

            // Deterministic Fisher-Yates shuffle using SeededRandom
            for (int i = remaining.Length - 1; i > 0; i--)
            {
                int swapIdx = Mathf.Clamp((int)random.Range(0f, i + 0.9999f), 0, i);
                int tmp = remaining[i];
                remaining[i] = remaining[swapIdx];
                remaining[swapIdx] = tmp;
            }

            int[] roster = new int[count];
            roster[0] = chosenLocal;
            for (int i = 1; i < count; i++)
            {
                roster[i] = remaining[i - 1];
            }
            return roster;
        }

        private void Awake()
        {
            if (configAsset != null)
            {
                PlayerView.ConfigureSharedResources(configAsset);
                WeaponFlightView.ConfigureSharedResources(configAsset);
            }
            EnsurePoolsInitialized();
        }

        private void Start()
        {
            if (WebGLPointerDiagnostics.IsRequested()) gameObject.AddComponent<WebGLPointerDiagnostics>();
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != "-capturePhase1") continue;
                autoPlay = true;
                CapturePhase1 = true;
            }

            int urlAutoMatches = ParseAutoPlayQueryCount();
            if (urlAutoMatches > 0)
            {
                autoPlay = true;
                _fastAutoPlayTimings = true;
                _webAutoPlayTotalMatches = urlAutoMatches;
                _webAutoPlayRemainingMatches = urlAutoMatches;
                StartProfilingWindow();
                Debug.Log($"[WebGLAutoPlay] Starting automated verification of {_webAutoPlayTotalMatches} matches on build...");
                StartMatch();
                return;
            }

            // If no canvas exists (e.g. running standalone headless test), start match immediately
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                StartMatch();
            }
        }

        private static int ParseAutoPlayQueryCount()
        {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url)) return 0;
            int idx = url.IndexOf("autoPlay=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return 0;
            idx += "autoPlay=".Length;
            int val = 0;
            while (idx < url.Length && char.IsDigit(url[idx]))
            {
                val = val * 10 + (url[idx] - '0');
                idx++;
            }
            return val > 0 ? val : 1;
        }

        private void StartProfilingWindow()
        {
            _profileFrames = 0;
            _profilePositiveAllocBytes = 0;
            _profileStartBytes = GC.GetTotalMemory(false);
            _profilePrevBytes = _profileStartBytes;
            _profileStartGcCount = GC.CollectionCount(0);
            _profileStartTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (State != null && State.Phase != Phase.MatchEnd)
            {
                _profileFrames++;
                long curBytes = GC.GetTotalMemory(false);
                long delta = curBytes - _profilePrevBytes;
                if (delta > 0) _profilePositiveAllocBytes += delta;
                _profilePrevBytes = curBytes;
            }

            if (hud != null && hud.ReplayPressed)
            {
                hud.ResetButtons();
                StartMatch();
            }
        }

        private void EnsurePoolsInitialized()
        {
            if (_poolsInitialized) return;
            _poolsInitialized = true;

            var poolRoot = new GameObject("MatchObjectPools");
            poolRoot.transform.SetParent(transform, false);

            for (int i = 0; i < MaxPoolSize; i++)
            {
                var pGo = new GameObject("PooledPlayer_" + i);
                pGo.transform.SetParent(poolRoot.transform, false);
                var pv = pGo.AddComponent<PlayerView>();
                pv.Initialise("pool-" + i, Color.white, i);
                pGo.SetActive(false);
                _playerPool.Add(pv);

                var fxGo = new GameObject("PooledHitFx_" + i);
                fxGo.transform.SetParent(poolRoot.transform, false);
                var fx = fxGo.AddComponent<HitFx>();
                if (configAsset != null && configAsset.hitFxFrames != null)
                {
                    fx.frames = configAsset.hitFxFrames;
                }
                fxGo.SetActive(false);
                _hitFxPool.Add(fx);

                var wGo = new GameObject("PooledWeapon_" + i);
                wGo.transform.SetParent(poolRoot.transform, false);
                var wf = wGo.AddComponent<WeaponFlightView>();
                wf.Initialise(Vector3.zero, Vector3.zero, 0.75f, false, fx);
                wGo.SetActive(false);
                _weaponPool.Add(wf);
            }
        }

        public void StartMatch()
        {
            EnsurePoolsInitialized();
            if (_matchCoroutine != null) StopCoroutine(_matchCoroutine);
            _matchCoroutine = StartCoroutine(NewMatch());
        }

        public void RequestReady()
        {
            if (hud != null) hud.ResetButtons();
            if (State != null && State.Phase == Phase.Placing)
            {
                _readyRequested = true;
            }
        }

        public void HandlePointer(int pointerId, Vector2 screenPos, Vector2 worldPos, PointerPhase phase)
        {
            if (_gestureV2 == null || State == null || State.Phase != Phase.Placing) return;

            var config = State.Config;
            var gamePos = new Vec2(worldPos.x, worldPos.y);
            var screenVec = new Vec2(screenPos.x, screenPos.y);
            bool insideArena = gamePos.X >= config.MinX && gamePos.X <= config.MaxX &&
                               gamePos.Y >= config.MinY && gamePos.Y <= config.MaxY;
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            float dpi = Screen.dpi > 0f ? Screen.dpi : 160f;
            float minAimPixels = PlacementGestureV2.MmToPixels(4.0f, dpi);

            switch (phase)
            {
                case PointerPhase.Began:
                    _gestureV2.PointerDown(pointerId, screenVec, gamePos, insideArena, nowMs);
                    if (insideArena && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPlace();
                    }
                    break;
                case PointerPhase.Moved:
                    _gestureV2.PointerMove(pointerId, screenVec, gamePos, insideArena, nowMs, minAimPixels);
                    break;
                case PointerPhase.Ended:
                case PointerPhase.Canceled:
                    _gestureV2.PointerUp(pointerId, screenVec, gamePos, insideArena, nowMs);
                    break;
            }

            SyncGesturePreview();
        }

        public void HandlePointer(PointerSample pointer)
        {
            int pid = 0;
            Vector2 screen = Camera.main != null ? Camera.main.WorldToScreenPoint(new Vector3(pointer.Position.X, pointer.Position.Y, 0f)) : Vector2.zero;
            Vector2 world = new Vector2(pointer.Position.X, pointer.Position.Y);
            HandlePointer(pid, screen, world, pointer.Phase);
        }

        private IEnumerator NewMatch()
        {
            yield return null;
            EnsurePoolsInitialized();
            if (configAsset != null)
            {
                PlayerView.ConfigureSharedResources(configAsset);
                WeaponFlightView.ConfigureSharedResources(configAsset);
            }

            var config = configAsset.ToCore();
            uint matchSeed = config.Seed + (_matchCounter * 97u);
            _matchCounter++;
            _random = new SeededRandom(matchSeed);
            _bots.Clear();
            _views.Clear();

            for (int i = 0; i < _playerPool.Count; i++)
            {
                if (_playerPool[i] != null) _playerPool[i].gameObject.SetActive(false);
            }
            ClearWeapons();

            int playerCount = Mathf.Clamp(config.MaxPlayers, 1, MaxPoolSize);
            int[] rosterIndices = SelectMatchRoster(localCharacterIndex, playerCount, _random);

            var players = new List<PlayerState>(playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                string id = i == 0 ? "player" : "bot-" + i;
                bool bot = i != 0;
                int charIdx = rosterIndices[i];
                string displayName = PlayerView.CharacterNames[charIdx];
                var p = new PlayerState(id, displayName, bot, config.StartHp,
                    new Vec2(_random.Range(config.MinX, config.MaxX), _random.Range(config.MinY, config.MaxY)),
                    Vec2.Up);
                players.Add(p);
                BindPooledView(p, i, charIdx);
                if (bot) _bots[id] = new Phase1BotBrain((BotLevel)((i - 1) % 3), _random);
            }

            State = new MatchState(config, players);
            if (hud != null) hud.ResetButtons();
            _readyRequested = false;

            if (_gestureV2 == null)
            {
                _gestureV2 = new PlacementGestureV2();
            }

            if (_matchCounter == 1)
            {
                StartProfilingWindow();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMatchBgm();
            }

            while (State.Phase != Phase.MatchEnd)
            {
                State.StartRound();
                var localPlayer = State.FindPlayer("player");
                _gestureV2.BeginRound(localPlayer.Position, Vec2.Up);
                SecondsRemaining = config.PlaceSeconds;
                _readyRequested = false;
                _lastTickSecond = -1;

                OnRoundStarted?.Invoke(State.RoundNumber, State.Config.MaxRounds);
                OnPhaseChanged?.Invoke(Phase.Placing);

                foreach (var pair in _bots)
                {
                    State.SetInput(pair.Value.MakeInput(State.FindPlayer(pair.Key), config));
                }

                if (autoPlay)
                {
                    var autoBrain = new Phase1BotBrain(BotLevel.Predictor, _random);
                    var autoInput = autoBrain.MakeInput(localPlayer, config);
                    State.SetInput(autoInput);

                    _gestureV2.PointerDown(0, Vec2.Zero, autoInput.Placement, true, 0);
                    _gestureV2.PointerMove(0, new Vec2(autoInput.AimDirection.X * 100f, autoInput.AimDirection.Y * 100f),
                        autoInput.Placement + autoInput.AimDirection, true, 10, 10f);
                    SyncGesturePreview();

                    if (CapturePhase1 && State.RoundNumber == 1)
                    {
                        Present(true);
                        yield return _waitReveal;
                    }
                    SecondsRemaining = 0f;
                }

                bool timedOut = false;
                while (!autoPlay && SecondsRemaining > 0 && !_readyRequested && (hud == null || !hud.ReadyPressed))
                {
                    if (!WebGLPointerDiagnostics.IsRequested())
                    {
                        SecondsRemaining -= Time.deltaTime;
                    }

                    OnTimerTick?.Invoke(SecondsRemaining);

                    int currentSec = Mathf.CeilToInt(SecondsRemaining);
                    if (currentSec <= 3 && currentSec > 0 && currentSec != _lastTickSecond)
                    {
                        _lastTickSecond = currentSec;
                        if (AudioManager.Instance != null)
                        {
                            if (currentSec == 1) AudioManager.Instance.PlayCountdownLast();
                            else AudioManager.Instance.PlayTick();
                        }
                    }

                    var snap = _gestureV2.Snapshot();
                    RoundInput liveInput = new RoundInput("player")
                    {
                        HasPlacement = snap.HasPlaced,
                        Placement = snap.Position,
                        HasAim = snap.AimActive,
                        AimDirection = snap.Facing,
                        IsReady = false
                    };
                    State.SetInput(liveInput);

                    Present(true);
                    yield return null;
                }

                if (SecondsRemaining <= 0f && !autoPlay)
                {
                    timedOut = true;
                    TriggerToast("Tự ném theo hướng cuối");
                }

                var lockSnap = _gestureV2.Snapshot();
                _gestureV2.Lock();

                RoundInput finalInput = new RoundInput("player")
                {
                    HasPlacement = true,
                    Placement = lockSnap.Position,
                    HasAim = lockSnap.AimActive,
                    AimDirection = lockSnap.Facing,
                    IsReady = true
                };
                State.SetInput(finalInput);
                State.LockRound();

                if (hud != null) hud.ResetButtons();
                _readyRequested = false;

                if (AudioManager.Instance != null && !timedOut)
                {
                    AudioManager.Instance.PlayReady();
                }

                Present(false);
                yield return _fastAutoPlayTimings ? _waitFastAuto : (CapturePhase1 && State.RoundNumber == 1 ? _waitLockCapture : _waitLockNormal);

                // Reveal Phase
                State.BeginReveal();
                OnPhaseChanged?.Invoke(Phase.Reveal);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayReveal();
                }
                Present(false, true);
                yield return _fastAutoPlayTimings ? _waitFastAuto : _waitReveal;

                // Throwing Phase
                State.BeginThrowing();
                OnPhaseChanged?.Invoke(Phase.Throwing);
                Present(false);
                SpawnWeapons();
                yield return _fastAutoPlayTimings ? _waitFastAuto : (CapturePhase1 && State.RoundNumber == 1 ? _waitThrowCapture : _waitThrowNormal);

                // Resolve Phase
                State.ResolveRound();
                ClearWeapons();
                OnPhaseChanged?.Invoke(Phase.Resolve);
                Present(false);
                yield return _fastAutoPlayTimings ? _waitFastAuto : (CapturePhase1 ? _waitResolveCapture : _waitResolveNormal);
            }

            // Match End Phase
            Present(false);
            OnPhaseChanged?.Invoke(Phase.MatchEnd);

            _rankedBuffer.Clear();
            for (int i = 0; i < State.Players.Count; i++)
            {
                _rankedBuffer.Add(State.Players[i]);
            }
            _rankedBuffer.Sort(ComparePlayersForRanking);

            _winnerIdsBuffer.Clear();
            for (int i = 0; i < State.WinnerIds.Count; i++)
            {
                _winnerIdsBuffer.Add(State.WinnerIds[i]);
            }

            bool isWinner = _winnerIdsBuffer.Contains("player");
            if (AudioManager.Instance != null)
            {
                if (isWinner) AudioManager.Instance.PlayWin();
                else AudioManager.Instance.PlayLose();
            }

            OnMatchEnded?.Invoke(_rankedBuffer, _winnerIdsBuffer);

            if (_webAutoPlayRemainingMatches > 0)
            {
                int doneIndex = _webAutoPlayTotalMatches - _webAutoPlayRemainingMatches + 1;
                _webAutoPlayRemainingMatches--;
                Debug.Log($"[WebGLAutoPlay] Match {doneIndex}/{_webAutoPlayTotalMatches} completed: rounds={State.RoundNumber}, winners=[{string.Join(", ", _winnerIdsBuffer)}]");

                if (_webAutoPlayRemainingMatches > 0)
                {
                    yield return _waitFastAuto;
                    StartMatch();
                }
                else
                {
                    float elapsedSec = Mathf.Max(0.01f, Time.realtimeSinceStartup - _profileStartTime);
                    float avgFps = _profileFrames / elapsedSec;
                    long avgAllocPerFrame = _profileFrames > 0 ? (_profilePositiveAllocBytes / _profileFrames) : 0;
                    int gcCollections = GC.CollectionCount(0) - _profileStartGcCount;
                    Debug.Log($"[WebGLAutoPlay] All {_webAutoPlayTotalMatches}/{_webAutoPlayTotalMatches} matches completed with 0 errors! " +
                              $"Frames={_profileFrames}, Elapsed={elapsedSec:F2}s, AvgFPS={avgFps:F1}, ManagedAllocPerFrame={avgAllocPerFrame} B/frame, GCGen0={gcCollections}");
                }
            }
        }

        private static int ComparePlayersForRanking(PlayerState a, PlayerState b)
        {
            if (a.IsAlive != b.IsAlive) return b.IsAlive.CompareTo(a.IsAlive);
            if (a.Hp != b.Hp) return b.Hp.CompareTo(a.Hp);
            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }

        private void TriggerToast(string message)
        {
            OnToastRequested?.Invoke(message);
        }

        private void BindPooledView(PlayerState p, int slotIndex, int characterIndex)
        {
            var view = _playerPool[slotIndex];
            view.gameObject.name = p.Id;
            view.gameObject.SetActive(true);
            view.Initialise(p.Id, Color.white, characterIndex);
            _views[p.Id] = view;
        }

        private void SyncGesturePreview()
        {
            if (_gestureV2 == null || State == null) return;
            var snap = _gestureV2.Snapshot();
            if (!snap.HasPlaced) return;

            var position = State.ConstrainPlacement("player", snap.Position);
            var player = State.FindPlayer("player");
            if (player != null)
            {
                player.Position = position;
                player.Facing = snap.Facing;
            }
            Present(true);
        }

        private void Present(bool placing, bool showAllAim = false)
        {
            if (State == null) return;
            var snap = _gestureV2 != null ? _gestureV2.Snapshot() : default;

            for (int i = 0; i < State.Players.Count; i++)
            {
                var p = State.Players[i];
                bool isLocal = p.Id == "player";

                bool visible = placing ? isLocal : p.IsAlive;
                bool showAim = placing ? (isLocal && snap.AimActive) : (showAllAim && p.IsAlive);
                bool showTouchRing = placing && isLocal;

                if (_views.TryGetValue(p.Id, out var view) && view != null)
                {
                    view.Present(p, visible, showAim, showTouchRing, State.Config.AimTouchRadius,
                        State.Config.AimLineExtraLength, isLocal && snap.AimActive ? 1.5f : 0f);
                }
            }
        }

        private void SpawnWeapons()
        {
            ClearWeapons();
            float duration = _fastAutoPlayTimings ? 0.04f : 0.75f;
            for (int i = 0; i < State.Players.Count && i < _weaponPool.Count; i++)
            {
                var p = State.Players[i];
                RoundInput input;
                if (!p.IsAlive || !State.LockedInputs.TryGetValue(p.Id, out input)) continue;

                Vec2 throwDir = input.HasAim ? input.AimDirection : p.Facing;
                if (throwDir.Length <= 0.0001f) throwDir = Vec2.Up;

                var hits = HitCalculator.CalculateHits(p, State.Players, throwDir, State.Config);
                Vec2 endPoint;
                bool didHit = hits.Count > 0;
                if (didHit)
                {
                    endPoint = hits[0].Point;
                }
                else
                {
                    endPoint = HitCalculator.GetWallPoint(p.Position, throwDir, State.Config);
                }

                var flight = _weaponPool[i];
                var hitFx = _hitFxPool[i];
                flight.Initialise(
                    new Vector3(p.Position.X, p.Position.Y, 0f),
                    new Vector3(endPoint.X, endPoint.Y, 0f),
                    duration,
                    didHit,
                    hitFx);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayThrow();
                AudioManager.Instance.PlayFly();
            }
        }

        private void ClearWeapons()
        {
            for (int i = 0; i < _weaponPool.Count; i++)
            {
                if (_weaponPool[i] != null && _weaponPool[i].gameObject.activeSelf)
                {
                    _weaponPool[i].gameObject.SetActive(false);
                }
            }
            for (int i = 0; i < _hitFxPool.Count; i++)
            {
                if (_hitFxPool[i] != null && _hitFxPool[i].gameObject.activeSelf)
                {
                    _hitFxPool[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
