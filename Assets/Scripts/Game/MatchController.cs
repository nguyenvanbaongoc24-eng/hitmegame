using System.Collections;
using System.Collections.Generic;
using HitMe.Core;
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
        public Vector2 LocalPosition { get { var p = State == null ? null : State.FindPlayer("player"); return p == null ? Vector2.zero : new Vector2(p.Position.X, p.Position.Y); } }
        private readonly Dictionary<string, PlayerView> _views = new Dictionary<string, PlayerView>();
        private readonly Dictionary<string, Phase1BotBrain> _bots = new Dictionary<string, Phase1BotBrain>();
        private readonly List<GameObject> _weapons = new List<GameObject>();
        private SeededRandom _random;
        private PlacementGestureState _gesture;

        private void Start()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != "-capturePhase1") continue;
                autoPlay = true;
                CapturePhase1 = true;
            }
            StartCoroutine(NewMatch());
        }
        private void Update() { if (hud != null && hud.ReplayPressed) { hud.ResetButtons(); StartCoroutine(NewMatch()); } }
        public void BeginLocalGesture(Vector2 point)
        {
            if (_gesture == null || State == null || State.Phase != Phase.Placing) return;
            _gesture.PointerDown(new Vec2(point.x, point.y), State.Config.PlayerRadius);
            SyncGesturePreview();
        }

        public void MoveLocalGesture(Vector2 point)
        {
            if (_gesture == null || State == null || State.Phase != Phase.Placing) return;
            _gesture.PointerMove(new Vec2(point.x, point.y), State.Config.MinAimDrag);
            SyncGesturePreview();
        }

        public void EndLocalGesture()
        {
            if (_gesture == null) return;
            _gesture.PointerUp();
        }

        private IEnumerator NewMatch()
        {
            yield return null;
            var config = configAsset.ToCore(); _random = new SeededRandom(config.Seed); _bots.Clear();
            foreach (var view in _views.Values) Destroy(view.gameObject); _views.Clear();
            var players = new List<PlayerState>();
            for (int i = 0; i < config.MaxPlayers; i++)
            {
                string id = i == 0 ? "player" : "bot-" + i; bool bot = i != 0;
                var p = new PlayerState(id, bot ? "BOT " + i : "BẠN", bot, config.StartHp, new Vec2(_random.Range(config.MinX, config.MaxX), _random.Range(config.MinY, config.MaxY)), Vec2.Up);
                players.Add(p); CreateView(p, i); if (bot) _bots[id] = new Phase1BotBrain((BotLevel)((i - 1) % 3), _random);
            }
            State = new MatchState(config, players); hud.ResetButtons();
            while (State.Phase != Phase.MatchEnd)
            {
                State.StartRound();
                var localPlayer = State.FindPlayer("player");
                _gesture = new PlacementGestureState(localPlayer.Position, localPlayer.Facing);
                SecondsRemaining = config.PlaceSeconds;
                foreach (var pair in _bots) State.SetInput(pair.Value.MakeInput(State.FindPlayer(pair.Key), config));
                if (autoPlay)
                {
                    var autoBrain = new Phase1BotBrain(BotLevel.Predictor, _random);
                    var autoInput = autoBrain.MakeInput(localPlayer, config);
                    State.SetInput(autoInput);
                    _gesture.SyncPlacement(State.ConstrainPlacement("player", autoInput.Placement));
                    _gesture.SetAimDirection(autoInput.AimDirection);
                    SyncGesturePreview();
                    // The capture run deliberately leaves round one visible long enough for
                    // ScreenCapture to render the placing state. Normal AutoPlay stays instant.
                    if (CapturePhase1 && State.RoundNumber == 1)
                    {
                        Present(true);
                        yield return new WaitForSeconds(.8f);
                    }
                    SecondsRemaining = 0f;
                }
                while (!autoPlay && SecondsRemaining > 0 && !hud.ReadyPressed) { SecondsRemaining -= Time.deltaTime; State.SetInput(_gesture.BuildRoundInput("player", false)); Present(true); yield return null; }
                State.SetInput(_gesture.BuildRoundInput("player", true)); State.LockRound(); hud.ResetButtons(); Present(false); yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? .25f : .02f);
                State.BeginReveal(); Present(false, true); yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? .8f : .02f);
                State.BeginThrowing(); Present(false); SpawnWeapons(); yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? 1f : .02f);
                State.ResolveRound(); ClearWeapons(); Present(false); yield return new WaitForSeconds(CapturePhase1 ? .02f : 1f);
            }
            Present(false);
        }

        private void CreateView(PlayerState p, int index) { var go = new GameObject(p.Id); var view = go.AddComponent<PlayerView>(); view.Initialise(p.Id, index == 0 ? Color.cyan : Color.HSVToRGB(index / 6f, .7f, 1)); _views[p.Id] = view; }
        private void SyncGesturePreview()
        {
            var position = State.ConstrainPlacement("player", _gesture.Placement);
            _gesture.SyncPlacement(position);
            var player = State.FindPlayer("player");
            player.Position = position;
            player.Facing = _gesture.EffectiveDirection;
            Present(true);
        }

        private void Present(bool placing, bool showAllAim = false)
        {
            if (State == null) return;
            foreach (var p in State.Players)
                _views[p.Id].Present(p, !placing || !p.IsBot, showAllAim || (placing && !p.IsBot && _gesture != null && _gesture.HasAim));
        }
        private void SpawnWeapons() { foreach (var p in State.Players) { if (!p.IsAlive) continue; var end = HitCalculator.GetWallPoint(p.Position, State.LockedInputs[p.Id].AimDirection, State.Config); var go = new GameObject("Weapon"); go.AddComponent<WeaponFlightView>().Initialise(new Vector3(p.Position.X, p.Position.Y), new Vector3(end.X, end.Y), 1f); _weapons.Add(go); } }
        private void ClearWeapons() { foreach (var weapon in _weapons) if (weapon != null) Destroy(weapon); _weapons.Clear(); }
    }
}
