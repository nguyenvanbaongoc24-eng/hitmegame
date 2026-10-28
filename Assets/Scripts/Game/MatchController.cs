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
        private Vec2 _localPlacement;
        private Vec2 _localDirection = Vec2.Up;

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
        public void SetLocalPlacement(Vector2 position) { _localPlacement = State.ConstrainPlacement("player", new Vec2(position.x, position.y)); Present(true); }
        public void SetLocalDirection(Vector2 direction) { if (direction.sqrMagnitude > .03f) _localDirection = new Vec2(direction.x, direction.y).Normalized; Present(true); }

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
                State.StartRound(); _localPlacement = State.FindPlayer("player").Position; _localDirection = State.FindPlayer("player").Facing; SecondsRemaining = config.PlaceSeconds;
                foreach (var pair in _bots) State.SetInput(pair.Value.MakeInput(State.FindPlayer(pair.Key), config));
                if (autoPlay)
                {
                    var autoBrain = new Phase1BotBrain(BotLevel.Predictor, _random);
                    State.SetInput(autoBrain.MakeInput(State.FindPlayer("player"), config));
                    // The capture run deliberately leaves round one visible long enough for
                    // ScreenCapture to render the placing state. Normal AutoPlay stays instant.
                    if (CapturePhase1 && State.RoundNumber == 1)
                    {
                        Present(true);
                        yield return new WaitForSeconds(.8f);
                    }
                    SecondsRemaining = 0f;
                }
                while (!autoPlay && SecondsRemaining > 0 && !hud.ReadyPressed) { SecondsRemaining -= Time.deltaTime; State.SetInput(new RoundInput("player") { HasPlacement = true, Placement = _localPlacement, HasAim = true, AimDirection = _localDirection, IsReady = false }); Present(true); yield return null; }
                State.SetInput(new RoundInput("player") { HasPlacement = true, Placement = _localPlacement, HasAim = true, AimDirection = _localDirection, IsReady = true }); State.LockRound(); hud.ResetButtons(); Present(false); yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? .25f : .02f);
                State.BeginReveal(); Present(false, true); yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? .8f : .02f);
                State.BeginThrowing(); Present(false); SpawnWeapons(); yield return new WaitForSeconds(CapturePhase1 && State.RoundNumber == 1 ? 1f : .02f);
                State.ResolveRound(); ClearWeapons(); Present(false); yield return new WaitForSeconds(CapturePhase1 ? .02f : 1f);
            }
            Present(false);
        }

        private void CreateView(PlayerState p, int index) { var go = new GameObject(p.Id); var view = go.AddComponent<PlayerView>(); view.Initialise(p.Id, index == 0 ? Color.cyan : Color.HSVToRGB(index / 6f, .7f, 1)); _views[p.Id] = view; }
        private void Present(bool placing, bool showAllAim = false)
        {
            if (State == null) return;
            foreach (var p in State.Players)
                _views[p.Id].Present(p, true, showAllAim || (placing && !p.IsBot));
        }
        private void SpawnWeapons() { foreach (var p in State.Players) { if (!p.IsAlive) continue; var end = HitCalculator.GetWallPoint(p.Position, State.LockedInputs[p.Id].AimDirection, State.Config); var go = new GameObject("Weapon"); go.AddComponent<WeaponFlightView>().Initialise(new Vector3(p.Position.X, p.Position.Y), new Vector3(end.X, end.Y), 1f); _weapons.Add(go); } }
        private void ClearWeapons() { foreach (var weapon in _weapons) if (weapon != null) Destroy(weapon); _weapons.Clear(); }
    }
}
