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
        private static readonly string[] CachedPlayerIds = { "player", "bot-1", "bot-2", "bot-3", "bot-4", "bot-5" };
        private readonly List<PlayerView> _playerPool = new List<PlayerView>(MaxPoolSize);
        private readonly List<WeaponFlightView> _weaponPool = new List<WeaponFlightView>(MaxPoolSize);
        private readonly List<HitFx> _hitFxPool = new List<HitFx>(MaxPoolSize);
        private readonly Dictionary<string, PlayerView> _views = new Dictionary<string, PlayerView>(MaxPoolSize);
        private readonly Dictionary<string, Phase1BotBrain> _bots = new Dictionary<string, Phase1BotBrain>(MaxPoolSize);
        private readonly List<PlayerState> _rankedBuffer = new List<PlayerState>(MaxPoolSize);
        private readonly List<string> _winnerIdsBuffer = new List<string>(MaxPoolSize);
        private readonly List<HitEvent> _weaponHitsBuffer = new List<HitEvent>(8);
        private readonly Dictionary<string, int> _hpBeforeResolve = new Dictionary<string, int>(8);
        private readonly RoundInput _localFinalInput = new RoundInput("player");

        private SeededRandom _random;
        private Phase1BotBrain _autoBrain;
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
        private int _profileTargetSeconds;
        private bool _fastAutoPlayTimings;
        private int _profileFrames;
        private int _profileSteadyFrames;
        private long _profileStartBytes;
        private long _profilePositiveAllocBytes;
        private long _profileSteadyAllocBytes;
        private long _profilePrevBytes;
        private int _profileStartGcCount;
        private float _profileStartTime;
        private float _profileMinFps = 999f;
        private bool _inTransitionFrame;

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
            _profileTargetSeconds = ParseQueryInt("profileSeconds=");
            if (urlAutoMatches > 0 || _profileTargetSeconds > 0)
            {
                autoPlay = true;
                _fastAutoPlayTimings = true;
                int initialMatches = urlAutoMatches > 0 ? urlAutoMatches : 10;
                _webAutoPlayTotalMatches = initialMatches;
                _webAutoPlayRemainingMatches = initialMatches;
                StartProfilingWindow();
                Debug.Log($"[WebGLAutoPlay] Starting automated verification of {_webAutoPlayTotalMatches} matches on build (targetSeconds={_profileTargetSeconds})...");
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
            return ParseQueryInt("autoPlay=");
        }

        private static int ParseQueryInt(string key)
        {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url)) return 0;
            int idx = url.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return 0;
            idx += key.Length;
            int val = 0;
            while (idx < url.Length && char.IsDigit(url[idx]))
            {
                val = val * 10 + (url[idx] - '0');
                idx++;
            }
            return val > 0 ? val : 1;
        }

        private void SyncProfileMemoryBaseline()
        {
            _profilePrevBytes = GC.GetTotalMemory(false);
        }

        private void StartProfilingWindow()
        {
            _profileFrames = 0;
            _profileSteadyFrames = 0;
            _profilePositiveAllocBytes = 0;
            _profileSteadyAllocBytes = 0;
            _profileMinFps = float.MaxValue;
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
                if (Time.unscaledDeltaTime > 0.0001f)
                {
                    float instFps = 1f / Time.unscaledDeltaTime;
                    if (_profileFrames > 5 && instFps < _profileMinFps)
                    {
                        _profileMinFps = instFps;
                    }
                }
                long curBytes = GC.GetTotalMemory(false);
                long delta = curBytes - _profilePrevBytes;
                if (delta > 0)
                {
                    _profilePositiveAllocBytes += delta;
                    if (State.Phase == Phase.Placing || State.Phase == Phase.Throwing)
                    {
                        _profileSteadyAllocBytes += delta;
                    }
                }
                if (State.Phase == Phase.Placing || State.Phase == Phase.Throwing)
                {
                    _profileSteadyFrames++;
                }
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

        public int ActiveWeaponCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < _weaponPool.Count; i++)
                    if (_weaponPool[i] != null && _weaponPool[i].gameObject.activeSelf) c++;
                return c;
            }
        }

        public int ActiveHitFxCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < _hitFxPool.Count; i++)
                    if (_hitFxPool[i] != null && _hitFxPool[i].gameObject.activeSelf) c++;
                return c;
            }
        }

        public int ActivePlayerViewCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < _playerPool.Count; i++)
                    if (_playerPool[i] != null && _playerPool[i].gameObject.activeSelf) c++;
                return c;
            }
        }

        public GestureSnapshot CurrentGestureSnapshot
        {
            get { return _gestureV2 != null ? _gestureV2.Snapshot() : default; }
        }

        public void StopAndCleanupMatch()
        {
            if (_matchCoroutine != null)
            {
                StopCoroutine(_matchCoroutine);
                _matchCoroutine = null;
            }
            ClearWeapons();
            for (int i = 0; i < _playerPool.Count; i++)
            {
                if (_playerPool[i] != null) _playerPool[i].gameObject.SetActive(false);
            }
            _views.Clear();
            _bots.Clear();
            _readyRequested = false;
            if (_gestureV2 != null)
            {
                _gestureV2.BeginRound(Vec2.Zero, Vec2.Up);
            }
            State = null;
        }

        public void StartMatch()
        {
            EnsurePoolsInitialized();
            if (_matchCoroutine != null)
            {
                StopCoroutine(_matchCoroutine);
                _matchCoroutine = null;
            }
            InitializeMatchSession();
            BeginNextRound();
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
            // Z10: Player placement is strictly constrained inside white court boundary
            float clampMinX = ArenaCoordinateMapper.PlayMinX;
            float clampMaxX = ArenaCoordinateMapper.PlayMaxX;
            float clampMinY = ArenaCoordinateMapper.PlayMinY;
            float clampMaxY = ArenaCoordinateMapper.PlayMaxY;
            bool insideArena = gamePos.X >= clampMinX && gamePos.X <= clampMaxX &&
                               gamePos.Y >= clampMinY && gamePos.Y <= clampMaxY;
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

        private void InitializeMatchSession()
        {
            EnsurePoolsInitialized();
            if (configAsset != null)
            {
                PlayerView.ConfigureSharedResources(configAsset);
                WeaponFlightView.ConfigureSharedResources(configAsset);
            }

            var config = configAsset != null ? configAsset.ToCore() : new GameConfig();
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
                string id = i < CachedPlayerIds.Length ? CachedPlayerIds[i] : ("bot-" + i);
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
            _autoBrain = new Phase1BotBrain(BotLevel.Predictor, _random);

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
            SyncProfileMemoryBaseline();
        }

        private void BeginNextRound()
        {
            if (State == null || State.Phase == Phase.MatchEnd) return;
            var config = State.Config;
            State.StartRound();
            var localPlayer = State.FindPlayer("player");
            if (_gestureV2 == null) _gestureV2 = new PlacementGestureV2();
            _gestureV2.BeginRound(localPlayer != null ? localPlayer.Position : Vec2.Zero, Vec2.Up);
            SecondsRemaining = config.PlaceSeconds;
            _readyRequested = false;
            _lastTickSecond = -1;

            OnRoundStarted?.Invoke(State.RoundNumber, State.Config.MaxRounds);
            OnPhaseChanged?.Invoke(Phase.Placing);
            OnTimerTick?.Invoke(SecondsRemaining);

            foreach (var pair in _bots)
            {
                State.SetInput(pair.Value.MakeInput(State.FindPlayer(pair.Key), config));
            }

            for (int i = 0; i < State.Players.Count; i++)
            {
                var p = State.Players[i];
                if (_views.TryGetValue(p.Id, out var v) && v != null && p.IsAlive)
                {
                    if (State.RoundNumber == 1)
                    {
                        v.PlayAnimation("Spawn");
                    }
                    else
                    {
                        v.PlayAnimation("Idle");
                    }
                }
            }

            Present(true);
            SyncProfileMemoryBaseline();
        }

        private IEnumerator NewMatch()
        {
            yield return null;
            if (State == null)
            {
                InitializeMatchSession();
                BeginNextRound();
            }

            var config = State.Config;
            while (State != null && State.Phase != Phase.MatchEnd)
            {
                if (State.Phase != Phase.Placing)
                {
                    BeginNextRound();
                }
                var localPlayer = State.FindPlayer("player");

                if (autoPlay)
                {
                    var autoInput = _autoBrain.MakeInput(localPlayer, config);
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
                string captureUrl = Application.absoluteURL ?? "";
                if (captureUrl.IndexOf("dragAim=1", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _gestureV2.PointerDown(0, Vec2.Zero, State.Config.HeadlessAimDragOrigin, true, 0);
                    _gestureV2.PointerMove(0, new Vec2(0f, 150f), new Vec2(0f, -1.0f), true, 10, 10f);
                    SyncGesturePreview();
                }

                if (captureUrl.IndexOf("captureHold=placing", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    while (true)
                    {
                        Present(true);
                        yield return null;
                    }
                }

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

                _localFinalInput.HasPlacement = true;
                _localFinalInput.Placement = lockSnap.Position;
                _localFinalInput.HasAim = lockSnap.AimActive;
                _localFinalInput.AimDirection = lockSnap.Facing;
                _localFinalInput.IsReady = true;
                State.SetInput(_localFinalInput);
                State.LockRound();
                OnPhaseChanged?.Invoke(State.Phase);

                if (hud != null) hud.ResetButtons();
                _readyRequested = false;

                if (AudioManager.Instance != null && !timedOut)
                {
                    AudioManager.Instance.PlayReady();
                }

                Present(false);
                if (captureUrl.IndexOf("captureHold=locked", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    while (true)
                    {
                        Present(false);
                        yield return null;
                    }
                }
                SyncProfileMemoryBaseline();
                yield return _fastAutoPlayTimings ? _waitFastAuto : (CapturePhase1 && State.RoundNumber == 1 ? _waitLockCapture : _waitLockNormal);

                // Reveal Phase
                State.BeginReveal();
                foreach (var pair in _bots)
                {
                    var lp = State.FindPlayer("player");
                    if (lp != null && lp.IsAlive)
                    {
                        pair.Value.ObservePlayerPosition(lp.Position);
                    }
                }
                OnPhaseChanged?.Invoke(State.Phase);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayReveal();
                }
                Present(false, true);
                if (captureUrl.IndexOf("captureHold=reveal", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    while (true)
                    {
                        Present(false, true);
                        yield return null;
                    }
                }
                SyncProfileMemoryBaseline();
                yield return _fastAutoPlayTimings ? _waitFastAuto : _waitReveal;

                // Throwing Phase
                State.BeginThrowing();
                OnPhaseChanged?.Invoke(State.Phase);
                Present(false);
                SpawnWeapons();
                if (captureUrl.IndexOf("captureHold=throwing", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    yield return new WaitForSeconds(0.35f);
                    while (true)
                    {
                        yield return null;
                    }
                }
                SyncProfileMemoryBaseline();
                yield return _fastAutoPlayTimings ? _waitFastAuto : (CapturePhase1 && State.RoundNumber == 1 ? _waitThrowCapture : _waitThrowNormal);

                // Resolve / RoundEnd / MatchEnd Phase (MatchState.Phase is the single source of truth)
                for (int i = 0; i < State.Players.Count; i++)
                {
                    var p = State.Players[i];
                    _hpBeforeResolve[p.Id] = p.Hp;
                }

                State.ResolveRound();
                ClearWeapons();

                // Trigger Hit / Defeat animations for feedback
                bool anyHit = false;
                bool anyDefeat = false;
                for (int i = 0; i < State.Players.Count; i++)
                {
                    var p = State.Players[i];
                    if (_views.TryGetValue(p.Id, out var view) && view != null)
                    {
                        if (!p.IsAlive || p.Hp <= 0)
                        {
                            view.PlayAnimation("Defeat");
                            anyDefeat = true;
                        }
                        else if (_hpBeforeResolve.TryGetValue(p.Id, out int oldHp) && p.Hp < oldHp)
                        {
                            view.PlayAnimation("Hit");
                            anyHit = true;
                        }
                        else
                        {
                            view.PlayAnimation("Idle");
                        }
                    }
                }
                if (anyDefeat)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();
                    else Audio.AudioService.Instance?.PlayDamage();
                }
                else if (anyHit)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();
                    else Audio.AudioService.Instance?.PlayHit();
                }
                if (State.Phase == Phase.MatchEnd)
                {
                    SyncProfileMemoryBaseline();
                    break;
                }
                OnPhaseChanged?.Invoke(State.Phase);
                Present(false);
                if (captureUrl.IndexOf("captureHold=roundResult", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    while (true)
                    {
                        Present(false);
                        yield return null;
                    }
                }
                SyncProfileMemoryBaseline();
                yield return _fastAutoPlayTimings ? _waitFastAuto : (CapturePhase1 ? _waitResolveCapture : _waitResolveNormal);
            }

            // Match End Phase
            Present(false);
            OnPhaseChanged?.Invoke(State.Phase);

            MatchRanking.BuildRankingsAndWinners(State, _rankedBuffer, _winnerIdsBuffer);

            for (int i = 0; i < State.Players.Count; i++)
            {
                var p = State.Players[i];
                if (_views.TryGetValue(p.Id, out var v) && v != null)
                {
                    if (_winnerIdsBuffer.Contains(p.Id))
                    {
                        v.PlayAnimation("Win");
                    }
                    else
                    {
                        v.PlayAnimation("Defeat");
                    }
                }
            }

            bool isWinner = _winnerIdsBuffer.Contains("player");
            if (AudioManager.Instance != null)
            {
                if (isWinner) AudioManager.Instance.PlayWin();
                else AudioManager.Instance.PlayLose();
            }

            OnMatchEnded?.Invoke(_rankedBuffer, _winnerIdsBuffer);
            SyncProfileMemoryBaseline();

            if (_webAutoPlayRemainingMatches > 0)
            {
                int doneIndex = _webAutoPlayTotalMatches - _webAutoPlayRemainingMatches + 1;
                _webAutoPlayRemainingMatches--;
                Debug.Log($"[WebGLAutoPlay] Match {doneIndex}/{_webAutoPlayTotalMatches} completed: rounds={State.RoundNumber}, winners=[{string.Join(", ", _winnerIdsBuffer)}]");
                SyncProfileMemoryBaseline();

                float currentElapsed = Time.realtimeSinceStartup - _profileStartTime;
                if (_webAutoPlayRemainingMatches == 0 && _profileTargetSeconds > 0 && currentElapsed < _profileTargetSeconds)
                {
                    _webAutoPlayRemainingMatches = 1;
                    _webAutoPlayTotalMatches++;
                }

                if (_webAutoPlayRemainingMatches > 0)
                {
                    yield return _waitFastAuto;
                    StartMatch();
                }
                else
                {
                    float elapsedSec = Mathf.Max(0.01f, Time.realtimeSinceStartup - _profileStartTime);
                    float avgFps = _profileFrames / elapsedSec;
                    float minFps = _profileMinFps < float.MaxValue ? _profileMinFps : avgFps;
                    long avgAllocPerFrame = _profileFrames > 0 ? (_profilePositiveAllocBytes / _profileFrames) : 0;
                    long steadyAllocPerFrame = _profileSteadyFrames > 0 ? (_profileSteadyAllocBytes / _profileSteadyFrames) : 0;
                    int gcCollections = GC.CollectionCount(0) - _profileStartGcCount;
                    Debug.Log($"[WebGLAutoPlay] All {_webAutoPlayTotalMatches}/{_webAutoPlayTotalMatches} matches completed with 0 errors! " +
                              $"Frames={_profileFrames}, Elapsed={elapsedSec:F2}s, AvgFPS={avgFps:F1}, MinFPS={minFps:F1}, SteadyAllocPerFrame={steadyAllocPerFrame} B/frame, ManagedAllocPerFrame={avgAllocPerFrame} B/frame, GCGen0={gcCollections}");
                }
            }
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
            bool isSelf = p.Id == "player" || slotIndex == 0;
            Color ringColor = isSelf ? PlayerView.PlayerRingPalette[0] : PlayerView.PlayerRingPalette[Mathf.Clamp(slotIndex, 1, PlayerView.PlayerRingPalette.Length - 1)];
            view.Initialise(p.Id, ringColor, characterIndex);
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
                bool showTouchRing = false;

                if (_views.TryGetValue(p.Id, out var view) && view != null)
                {
                    view.Present(p, visible, showAim, showTouchRing, State.Config.AimTouchRadius,
                        State.Config.AimLineExtraLength, isLocal && snap.AimActive ? 1.50f : 0f);

                    if (!p.IsAlive || p.Hp <= 0)
                    {
                        view.PlayAnimation("Defeat");
                    }
                    else if (placing && isLocal && snap.AimActive)
                    {
                        view.PlayAnimation("Aim");
                    }
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

                if (_views.TryGetValue(p.Id, out var pView) && pView != null)
                {
                    pView.PlayAnimation("Throw");
                }

                Vec2 throwDir = input.HasAim ? input.AimDirection : p.Facing;
                if (throwDir.Length <= 0.0001f) throwDir = Vec2.Up;

                HitCalculator.CalculateHits(p, State.Players, throwDir, State.Config, _weaponHitsBuffer);
                Vec2 endPoint;
                bool didHit = _weaponHitsBuffer.Count > 0;
                if (didHit)
                {
                    endPoint = _weaponHitsBuffer[0].Point;
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
