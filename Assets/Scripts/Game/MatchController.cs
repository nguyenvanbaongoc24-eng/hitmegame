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

        private string _scenarioName;
        private bool _scenarioOriginalAutoPlay;

        public void ConfigureScenario(string name)
        {
            if (_scenarioName == null) _scenarioOriginalAutoPlay = autoPlay;
            _scenarioName = DevScenario.IsKnown(name) ? name : null;
            if (_scenarioName != null)
            {
                autoPlay = true;
                _fastAutoPlayTimings = false;
            }
            else autoPlay = _scenarioOriginalAutoPlay;
        }



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

        private readonly Dictionary<string, (string kind, string id)> _pendingEmotes = new Dictionary<string, (string kind, string id)>();

        public bool IsPaused { get; private set; }

        private float _botEmoteTimer;

        private const float BotEmoteInterval = 18f;

        private int _lastAimTickStep = -1;



        public void SetPaused(bool paused)

        {

            if (_bots.Count == 0) return; // Only pause for practice / bot mode per SET-05

            IsPaused = paused;

        }



        public void HandlePlayerEmote(string kind, string id, string playerId = "player")

        {

            if (State == null) return;

            if (State.Phase == Phase.Placing)

            {

                // Secret in placing phase (EMO-07): do not show on field yet!

                _pendingEmotes[playerId] = (kind, id);

            }

            else

            {

                if (_views.TryGetValue(playerId, out var view) && view != null)

                {

                    view.ShowEmote(kind, id);

                }

            }

        }



        private readonly Dictionary<string, Phase1BotBrain> _bots = new Dictionary<string, Phase1BotBrain>(MaxPoolSize);
        private readonly Dictionary<string, int> _playerCharIndices = new Dictionary<string, int>(MaxPoolSize);

        public string GetPlayerCharacterId(string playerId)
        {
            if (!string.IsNullOrEmpty(playerId) && _playerCharIndices.TryGetValue(playerId, out int idx))
            {
                return HitMe.Data.CharacterRegistry.GetByIndex(idx).id;
            }
            if (playerId == "player")
            {
                return HitMe.Data.CharacterRegistry.GetByIndex(localCharacterIndex).id;
            }
            return "nam_lang";
        }



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

            int totalCanonical = HitMe.Data.CharacterRegistry.CanonicalCharacters.Length;

            int count = Mathf.Clamp(maxPlayers, 1, totalCanonical);

            int chosenLocal = ((localCharIndex % totalCanonical) + totalCanonical) % totalCanonical;



            // M10: Only pick enabled canonical characters for bots (Nam Làng, Nữ Năng Động, Cô Ba, Anh Teen)

            var enabledPool = new List<int>();

            var fallbackPool = new List<int>();

            for (int c = 0; c < totalCanonical; c++)

            {

                if (c == chosenLocal) continue;

                if (HitMe.Data.CharacterRegistry.CanonicalCharacters[c].enabled)

                {

                    enabledPool.Add(c);

                }

                else

                {

                    fallbackPool.Add(c);

                }

            }



            // Deterministic Fisher-Yates shuffle using SeededRandom

            for (int i = enabledPool.Count - 1; i > 0; i--)

            {

                int swapIdx = Mathf.Clamp((int)random.Range(0f, i + 0.9999f), 0, i);

                int tmp = enabledPool[i];

                enabledPool[i] = enabledPool[swapIdx];

                enabledPool[swapIdx] = tmp;

            }



            for (int i = fallbackPool.Count - 1; i > 0; i--)

            {

                int swapIdx = Mathf.Clamp((int)random.Range(0f, i + 0.9999f), 0, i);

                int tmp = fallbackPool[i];

                fallbackPool[i] = fallbackPool[swapIdx];

                fallbackPool[swapIdx] = tmp;

            }



            var candidatePool = new List<int>(enabledPool);

            candidatePool.AddRange(fallbackPool);



            int[] roster = new int[count];

            roster[0] = chosenLocal;

            for (int i = 1; i < count; i++)

            {

                roster[i] = candidatePool[i - 1];

            }

            return roster;

        }







        public static event System.Action<string, string, string> OnMatchE2EEvent;

        public static event System.Action<string, string, string> OnEmoteTriggered;



        public static void EmitE2EEvent(string eventType, string sourceId = null, string targetId = null)

        {

            OnMatchE2EEvent?.Invoke(eventType, sourceId ?? "", targetId ?? "");

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
            if (_scenarioName != null) DevScenario.Configure(_scenarioName, config);



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



                float spawnMinX = ArenaCoordinateMapper.PlayMinX + 0.75f;

                float spawnMaxX = ArenaCoordinateMapper.PlayMaxX - 0.75f;

                float spawnMinY = ArenaCoordinateMapper.PlayMinY + 0.75f;

                float spawnMaxY = ArenaCoordinateMapper.PlayMaxY - 1.00f;

                const float minPlayerDist = 1.00f; // >= 1.2 tile (1.00m >= 0.96m)



                Vec2 spawnPos = new Vec2(_random.Range(spawnMinX, spawnMaxX), _random.Range(spawnMinY, spawnMaxY));

                for (int attempt = 0; attempt < 100; attempt++)

                {

                    bool tooClose = false;

                    for (int j = 0; j < players.Count; j++)

                    {

                        float dx = spawnPos.X - players[j].Position.X;

                        float dy = spawnPos.Y - players[j].Position.Y;

                        if (Mathf.Sqrt(dx * dx + dy * dy) < minPlayerDist)

                        {

                            tooClose = true;

                            break;

                        }

                    }

                    if (!tooClose) break;

                    spawnPos = new Vec2(_random.Range(spawnMinX, spawnMaxX), _random.Range(spawnMinY, spawnMaxY));

                }



                var p = new PlayerState(id, displayName, bot, config.StartHp,

                    spawnPos,

                    Vec2.Up);



                players.Add(p);



                _playerCharIndices[id] = charIdx;
                BindPooledView(p, i, charIdx);



                if (bot) _bots[id] = new Phase1BotBrain((BotLevel)((i - 1) % 3), _random);



            }



            _autoBrain = new Phase1BotBrain(BotLevel.Predictor, _random);







            State = new MatchState(config, players);
            if (_scenarioName != null) DevScenario.Prime(_scenarioName, State);



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



            if (!IsPaused && _bots.Count > 0)

            {

                _botEmoteTimer += Time.deltaTime;

                if (_botEmoteTimer >= BotEmoteInterval)

                {

                    _botEmoteTimer = 0f;

                    var aliveBots = new System.Collections.Generic.List<string>();

                    foreach (var p in State.Players)

                    {

                        if (p.Id != "player" && p.IsAlive) aliveBots.Add(p.Id);

                    }

                    if (aliveBots.Count > 0)

                    {

                        string chosenBotId = aliveBots[UnityEngine.Random.Range(0, aliveBots.Count)];

                        string[] phraseIds = { "phrase_01", "phrase_02", "phrase_03", "phrase_04", "phrase_05", "phrase_06" };

                        string chosenPhrase = phraseIds[UnityEngine.Random.Range(0, phraseIds.Length)];

                        HandlePlayerEmote("phrase", chosenPhrase, chosenBotId);

                        OnEmoteTriggered?.Invoke("phrase", chosenPhrase, chosenBotId);

                    }

                }

            }



            var config = State.Config;



            State.StartRound();
            if (_scenarioName != null) DevScenario.PrepareRound(_scenarioName, State);

            if (State.IsTiebreak) TriggerToast("HIỆP PHỤ!");
            else if (State.NormalRoundNumber == config.SuddenDeathRound) TriggerToast("CHẾT NGAY!");



            var localPlayer = State.FindPlayer("player");



            if (_gestureV2 == null) _gestureV2 = new PlacementGestureV2();



            _gestureV2.BeginRound(localPlayer != null ? localPlayer.Position : Vec2.Zero, Vec2.Up);



            SecondsRemaining = config.PlaceSeconds;



            _readyRequested = false;



            _lastTickSecond = -1;







            OnRoundStarted?.Invoke(State.RoundNumber, State.Config.MaxRounds);



            OnPhaseChanged?.Invoke(Phase.Placing);



            OnTimerTick?.Invoke(SecondsRemaining);



            AudioManager.Instance?.PlayRoundStart();







            var assignedPlacements = new List<Vec2>();

            // reuse localPlayer

            if (localPlayer != null) assignedPlacements.Add(localPlayer.Position);



            float botMinX = ArenaCoordinateMapper.PlayMinX + 0.75f;

            float botMaxX = ArenaCoordinateMapper.PlayMaxX - 0.75f;

            float botMinY = ArenaCoordinateMapper.PlayMinY + 0.75f;

            float botMaxY = ArenaCoordinateMapper.PlayMaxY - 1.00f; // M4: top margin >= 1 tile

            const float minBotDist = 1.00f; // M4: separation >= 1.2 tile (0.90m)



            foreach (var pair in _bots)

            {

                var botPlayer = State.FindPlayer(pair.Key);

                if (botPlayer == null || !botPlayer.IsAlive) continue;

                if (_scenarioName != null)
                {
                    State.SetInput(DevScenario.Input(_scenarioName, State, botPlayer));
                    continue;
                }



                var rawInput = pair.Value.MakeInput(botPlayer, config);

                Vec2 chosenPos = rawInput.Placement;

                chosenPos = new Vec2(Mathf.Clamp(chosenPos.X, botMinX, botMaxX), Mathf.Clamp(chosenPos.Y, botMinY, botMaxY));



                for (int attempt = 0; attempt < 60; attempt++)

                {

                    bool tooClose = false;

                    for (int a = 0; a < assignedPlacements.Count; a++)

                    {

                        float dx = chosenPos.X - assignedPlacements[a].X;

                        float dy = chosenPos.Y - assignedPlacements[a].Y;

                        if (Mathf.Sqrt(dx * dx + dy * dy) < minBotDist)

                        {

                            tooClose = true;

                            break;

                        }

                    }

                    if (!tooClose) break;

                    chosenPos = new Vec2(_random.Range(botMinX, botMaxX), _random.Range(botMinY, botMaxY));

                }



                assignedPlacements.Add(chosenPos);

                rawInput.Placement = chosenPos;

                State.SetInput(rawInput);

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



                    var autoInput = _scenarioName != null
                        ? DevScenario.Input(_scenarioName, State, localPlayer)
                        : _autoBrain.MakeInput(localPlayer, config);



                    State.SetInput(autoInput);







                    _gestureV2.PointerDown(0, Vec2.Zero, autoInput.Placement, true, 0);



                    _gestureV2.PointerMove(0, new Vec2(autoInput.AimDirection.X * 100f, autoInput.AimDirection.Y * 100f),



                        autoInput.Placement + autoInput.AimDirection, true, 10, 10f);



                    SyncGesturePreview();

                    if (_scenarioName != null)
                    {
                        Present(true);
                        yield return new WaitForSeconds(1.2f);
                    }







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



                    if (!WebGLPointerDiagnostics.IsRequested() && !IsPaused)



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



                if (_pendingEmotes.Count > 0)

                {

                    foreach (var kvp in _pendingEmotes)

                    {

                        if (_views.TryGetValue(kvp.Key, out var view) && view != null)

                        {

                            view.ShowEmote(kvp.Value.kind, kvp.Value.id);

                        }

                    }

                    _pendingEmotes.Clear();

                }







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



                // B3: Synchronized throw animation for all alive characters

                for (int i = 0; i < State.Players.Count; i++)

                {

                    var p = State.Players[i];

                    if (p.IsAlive && _views.TryGetValue(p.Id, out var v) && v != null)

                    {

                        v.PlayAnimation("Throw");

                    }

                }



                // Wait 0.15s for windup before launching weapon at release frame

                yield return new WaitForSeconds(0.15f);



                SpawnWeapons();

                for (int i = 0; i < State.Players.Count; i++)

                {

                    if (State.Players[i].IsAlive) EmitE2EEvent("throw", State.Players[i].Id, "");

                }



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



                // Dispatch events from State.LastResolution.Hits

                if (State.LastResolution != null && State.LastResolution.Hits != null)

                {

                    for (int hIdx = 0; hIdx < State.LastResolution.Hits.Count; hIdx++)

                    {

                        var hitEv = State.LastResolution.Hits[hIdx];

                        EmitE2EEvent("hit", hitEv.ShooterId, hitEv.TargetId);



                        if (_views.TryGetValue(hitEv.TargetId, out var victimView) && victimView != null)

                        {

                            var shooter = State.FindPlayer(hitEv.ShooterId);

                            Vector2 recoilDir = Vector2.down;

                            if (shooter != null)

                            {

                                var victim = State.FindPlayer(hitEv.TargetId);

                                if (victim != null)

                                {

                                    recoilDir = new Vector2(victim.Position.X - shooter.Position.X, victim.Position.Y - shooter.Position.Y).normalized;

                                }

                            }

                            victimView.TriggerHit(recoilDir);

                        }

                    }

                }



                for (int i = 0; i < State.Players.Count; i++)

                {

                    var p = State.Players[i];

                    if (_views.TryGetValue(p.Id, out var view) && view != null)

                    {

                        if (!p.IsAlive || p.Hp <= 0)

                        {

                            view.PlayAnimation("Defeat");

                            anyDefeat = true;

                            EmitE2EEvent("ko", "", p.Id);

                        }

                        else if (_hpBeforeResolve.TryGetValue(p.Id, out int oldHp) && p.Hp < oldHp)

                        {

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



                SyncProfileMemoryBaseline();



                yield return _fastAutoPlayTimings ? _waitFastAuto : (CapturePhase1 ? _waitResolveCapture : _waitResolveNormal);



            }







            // Match End Phase



            Present(false);



            OnPhaseChanged?.Invoke(State.Phase);







            MatchRanking.BuildRankingsAndWinners(State, _rankedBuffer, _winnerIdsBuffer);

            for (int w = 0; w < _winnerIdsBuffer.Count; w++)

            {

                EmitE2EEvent("win", _winnerIdsBuffer[w], "");

            }







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



                if (snap.AimActive && snap.Facing.LengthSquared > 0.01f)

                {

                    float angle = Mathf.Atan2(snap.Facing.Y, snap.Facing.X) * Mathf.Rad2Deg;

                    if (angle < 0f) angle += 360f;

                    int step = Mathf.FloorToInt(angle / 15f);

                    if (_lastAimTickStep != -1 && step != _lastAimTickStep)

                    {

                        AudioManager.Instance?.PlayAimTick();

                    }

                    _lastAimTickStep = step;

                }

                else

                {

                    _lastAimTickStep = -1;

                }

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



            AudioManager.Instance?.PlayThrow("dep_to_ong");



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



