using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<string, PlayerView> _views = new Dictionary<string, PlayerView>();
        private readonly Dictionary<string, Phase1BotBrain> _bots = new Dictionary<string, Phase1BotBrain>();
        private readonly List<GameObject> _weapons = new List<GameObject>();
        private SeededRandom _random;
        private PlacementGestureV2 _gestureV2;
        private Coroutine _matchCoroutine;
        private int _lastTickSecond = -1;

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

            // If no canvas exists (e.g. running standalone headless test), start match immediately
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                StartMatch();
            }
        }

        private void Update()
        {
            if (hud != null && hud.ReplayPressed)
            {
                hud.ResetButtons();
                StartMatch();
            }
        }

        public void StartMatch()
        {
            if (_matchCoroutine != null) StopCoroutine(_matchCoroutine);
            _matchCoroutine = StartCoroutine(NewMatch());
        }

        public void RequestReady()
        {
            if (hud != null) hud.ResetButtons();
            // Signal ready to break placing loop early
            if (State != null && State.Phase == Phase.Placing)
            {
                _readyRequested = true;
            }
        }

        private bool _readyRequested;

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
            // Backward compatibility with PointerSample
            int pid = 0;
            Vector2 screen = Camera.main != null ? Camera.main.WorldToScreenPoint(new Vector3(pointer.Position.X, pointer.Position.Y, 0f)) : Vector2.zero;
            Vector2 world = new Vector2(pointer.Position.X, pointer.Position.Y);
            HandlePointer(pid, screen, world, pointer.Phase);
        }

        private IEnumerator NewMatch()
        {
            yield return null;
            var config = configAsset.ToCore();
            _random = new SeededRandom(config.Seed);
            _bots.Clear();

            foreach (var view in _views.Values)
            {
                if (view != null && view.gameObject != null) Destroy(view.gameObject);
            }
            _views.Clear();

            var players = new List<PlayerState>();
            string[] botNames = new string[] { "Nữ Năng Động", "Quậy Boy", "Cô Ba", "Anh Teen", "Chú Chó Vàng" };
            for (int i = 0; i < config.MaxPlayers; i++)
            {
                string id = i == 0 ? "player" : "bot-" + i;
                bool bot = i != 0;
                string displayName = bot ? botNames[(i - 1) % botNames.Length] : "Nam Làng";
                var p = new PlayerState(id, displayName, bot, config.StartHp,
                    new Vec2(_random.Range(config.MinX, config.MaxX), _random.Range(config.MinY, config.MaxY)),
                    Vec2.Up);
                players.Add(p);
                CreateView(p, i);
                if (bot) _bots[id] = new Phase1BotBrain((BotLevel)((i - 1) % 3), _random);
            }

            State = new MatchState(config, players);
            if (hud != null) hud.ResetButtons();
            _readyRequested = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMatchBgm();
            }

            while (State.Phase != Phase.MatchEnd)
            {
                State.StartRound();
                var localPlayer = State.FindPlayer("player");
                _gestureV2 = new PlacementGestureV2();
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
                        yield return new WaitForSeconds(.8f);
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

                    // Countdown audio tick in last 3 seconds
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
                    // UX Rule: Hết 5 giây: hiện toast "Tự ném theo hướng cuối".
                    TriggerToast("Tự ném theo hướng cuối");
                }

                // Lock Placement Phase
                var lockSnap = _gestureV2.Snapshot();
                _gestureV2.Lock();

                // If no manual aim drag was done, aim in facing direction (NoAimPolicy = ThrowFacing)
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
                yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? .25f : .05f);

                // Reveal Phase
                State.BeginReveal();
                OnPhaseChanged?.Invoke(Phase.Reveal);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayReveal();
                }
                Present(false, true);
                yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? .8f : 0.8f);

                // Throwing Phase
                State.BeginThrowing();
                OnPhaseChanged?.Invoke(Phase.Throwing);
                Present(false);
                SpawnWeapons();
                yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? 1f : 0.85f);

                // Resolve Phase
                State.ResolveRound();
                ClearWeapons();
                OnPhaseChanged?.Invoke(Phase.Resolve);
                Present(false);
                yield return new WaitForSeconds(CapturePhase1 ? .02f : 1.2f);
            }

            // Match End Phase
            Present(false);
            OnPhaseChanged?.Invoke(Phase.MatchEnd);

            var ranked = State.Players
                .OrderByDescending(p => p.IsAlive)
                .ThenByDescending(p => p.Hp)
                .ThenBy(p => p.Name)
                .ToList();

            bool isWinner = State.WinnerIds.Contains("player");
            if (AudioManager.Instance != null)
            {
                if (isWinner) AudioManager.Instance.PlayWin();
                else AudioManager.Instance.PlayLose();
            }

            OnMatchEnded?.Invoke(ranked, State.WinnerIds.ToList());
        }

        private void TriggerToast(string message)
        {
            OnToastRequested?.Invoke(message);
        }

        private void CreateView(PlayerState p, int index)
        {
            var go = new GameObject(p.Id);
            var view = go.AddComponent<PlayerView>();
            // Color tint removed: All 6 characters have distinct visual index without tint
            view.Initialise(p.Id, Color.white, index % 6);
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

            foreach (var p in State.Players)
            {
                bool isLocal = p.Id == "player";

                // UX Rule: Trong pha Placing CHỈ hiện nhân vật người chơi và thước ngắm của họ.
                // Không hiện đối thủ, vị trí, hướng ngắm hoặc dép của đối thủ.
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
            foreach (var p in State.Players)
            {
                RoundInput input;
                if (!p.IsAlive || !State.LockedInputs.TryGetValue(p.Id, out input)) continue;

                Vec2 throwDir = input.HasAim ? input.AimDirection : p.Facing;
                if (throwDir.Length <= 0.0001f) throwDir = Vec2.Up;

                // Rule: Vũ khí dừng ở người đầu tiên trúng.
                var hits = HitCalculator.CalculateHits(p, State.Players, throwDir, State.Config);
                Vec2 endPoint;
                if (hits.Count > 0)
                {
                    endPoint = hits[0].Point;
                }
                else
                {
                    endPoint = HitCalculator.GetWallPoint(p.Position, throwDir, State.Config);
                }

                var go = new GameObject("Weapon_" + p.Id);
                var flight = go.AddComponent<WeaponFlightView>();
                flight.Initialise(new Vector3(p.Position.X, p.Position.Y, 0f), new Vector3(endPoint.X, endPoint.Y, 0f), 0.75f);
                _weapons.Add(go);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayThrow();
                AudioManager.Instance.PlayFly(); // Placeholder cue
            }
        }

        private void ClearWeapons()
        {
            foreach (var weapon in _weapons)
            {
                if (weapon != null) Destroy(weapon);
            }
            _weapons.Clear();
        }
    }
}
