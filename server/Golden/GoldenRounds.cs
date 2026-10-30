using System.Text.Json;
using HitMe.Core;

namespace HitMe.Server.Golden;

// Public fields keep this contract consumable by Unity JsonUtility as well as System.Text.Json.
public sealed class GoldenRoundsFile
{
    public int SchemaVersion = 1;
    public string GameConfigVersion = "phase1-v1.2";
    public GoldenGameConfig Config = new();
    public GoldenRound[] Rounds = Array.Empty<GoldenRound>();
    public GoldenMatch[] Matches = Array.Empty<GoldenMatch>();
}

public sealed class GoldenGameConfig
{
    public int StartHp;
    public float ArenaWidth;
    public float ArenaHeight;
    public float PlayerRadius;
    public int DamagePerHit;
    public int MaxHitsPerThrow;
    public int MaxRounds;
    public float MinSpawnDistance;

    public static GoldenGameConfig From(GameConfig config) => new()
    {
        StartHp = config.StartHp, ArenaWidth = config.ArenaWidth, ArenaHeight = config.ArenaHeight,
        PlayerRadius = config.PlayerRadius, DamagePerHit = config.DamagePerHit,
        MaxHitsPerThrow = config.MaxHitsPerThrow, MaxRounds = config.MaxRounds,
        MinSpawnDistance = config.MinSpawnDistance
    };

    public GameConfig ToCore() => new()
    {
        StartHp = StartHp, ArenaWidth = ArenaWidth, ArenaHeight = ArenaHeight,
        PlayerRadius = PlayerRadius, DamagePerHit = DamagePerHit,
        MaxHitsPerThrow = MaxHitsPerThrow, MaxRounds = MaxRounds,
        MinSpawnDistance = MinSpawnDistance
    };
}

public sealed class GoldenRound
{
    public int Id;
    public uint Seed;
    public string Scenario = "random";
    public GoldenPlayer[] Players = Array.Empty<GoldenPlayer>();
    public GoldenInput[] Inputs = Array.Empty<GoldenInput>();
    public GoldenExpected Expected = new();
}

// A whole-match vector complements single-round vectors: it captures eliminations
// across rounds and the Core's final winner/tie decision at MaxRounds.
public sealed class GoldenMatch
{
    public int Id;
    public uint Seed;
    public string Scenario = "match";
    public GoldenPlayer[] Players = Array.Empty<GoldenPlayer>();
    public GoldenMatchRound[] Rounds = Array.Empty<GoldenMatchRound>();
    public GoldenMatchExpected Expected = new();
}

public sealed class GoldenMatchRound
{
    public GoldenInput[] Inputs = Array.Empty<GoldenInput>();
}

public sealed class GoldenMatchExpected
{
    public int RoundNumber;
    public string Phase = "MatchEnd";
    public string[] WinnerIds = Array.Empty<string>();
    public GoldenPlayer[] Players = Array.Empty<GoldenPlayer>();
}

public sealed class GoldenPlayer
{
    public string Id = "";
    public int Hp;
    public float X;
    public float Y;
    public float FacingX;
    public float FacingY;
    public int HitsLanded;

    public PlayerState ToCore() => new(Id, Id, false, Hp, new Vec2(X, Y), new Vec2(FacingX, FacingY)) { HitsLanded = HitsLanded };
    public static GoldenPlayer From(PlayerState player) => new()
    {
        Id = player.Id, Hp = player.Hp, X = player.Position.X, Y = player.Position.Y,
        FacingX = player.Facing.X, FacingY = player.Facing.Y, HitsLanded = player.HitsLanded
    };
}

public sealed class GoldenInput
{
    public string PlayerId = "";
    public bool HasAim;
    public float AimX;
    public float AimY;

    public RoundInput ToCore() => new(PlayerId) { HasAim = HasAim, AimDirection = new Vec2(AimX, AimY), IsReady = true };
}

public sealed class GoldenHit
{
    public string ShooterId = "";
    public string TargetId = "";
    public float Distance;
}

public sealed class GoldenExpected
{
    public GoldenHit[] Hits = Array.Empty<GoldenHit>();
    public GoldenPlayer[] Players = Array.Empty<GoldenPlayer>();
}

public static class GoldenRoundsCodec
{
    static readonly JsonSerializerOptions Options = new() { IncludeFields = true, PropertyNameCaseInsensitive = true, WriteIndented = true };
    public static GoldenRoundsFile Read(string path) => JsonSerializer.Deserialize<GoldenRoundsFile>(File.ReadAllText(path), Options) ?? throw new InvalidOperationException("Invalid golden rounds JSON.");
    public static void Write(string path, GoldenRoundsFile file) => File.WriteAllText(path, JsonSerializer.Serialize(file, Options) + Environment.NewLine);
}

public static class GoldenRoundsFactory
{
    public static GoldenRoundsFile Create()
    {
        var config = new GameConfig { MaxPlayers = 6, StartHp = 3, MaxRounds = 5, DamagePerHit = 1, MaxHitsPerThrow = 1, PlayerRadius = .55f, MinSpawnDistance = .9f };
        var rounds = new List<GoldenRound>
        {
            Complete(1, 1001, "adjacent_players", config,
                Players(("a", 3, 0f, 0f), ("b", 3, 0f, .54f)), Inputs(("a", true, 0f, 1f))),
            Complete(2, 1002, "collinear_first_target_only", config,
                Players(("a", 3, 0f, 0f), ("near", 3, 0f, 2f), ("far", 3, 0f, 4f)), Inputs(("a", true, 0f, 1f))),
            Complete(3, 1003, "eliminated_player_never_throws_or_hits", config,
                Players(("dead", 0, 0f, 2f), ("live", 3, 0f, 0f)), Inputs(("dead", true, 0f, -1f), ("live", true, 0f, 1f))),
            Complete(4, 1004, "simultaneous_mutual_hits", config,
                Players(("a", 1, 0f, -2f), ("b", 1, 0f, 2f)), Inputs(("a", true, 0f, 1f), ("b", true, 0f, -1f)))
        };
        for (var id = 5; id <= 200; id++) rounds.Add(RandomRound(id, (uint)(7000 + id), config));
        return new GoldenRoundsFile { Config = GoldenGameConfig.From(config), Rounds = rounds.ToArray(), Matches = CreateMatches(config) };
    }

    static GoldenMatch[] CreateMatches(GameConfig config)
    {
        var matches = new List<GoldenMatch>();
        for (var count = 2; count <= 6; count++)
        {
            var players = Enumerable.Range(0, count)
                .Select(index => new GoldenPlayer { Id = $"m{count}p{index}", Hp = 1, X = index * 2f, Y = 0f, FacingX = index == count - 1 ? -1f : 1f, FacingY = 0f })
                .ToArray();
            var turns = Enumerable.Range(0, config.MaxRounds)
                .Select(_ => new GoldenMatchRound { Inputs = players.Select(player => new GoldenInput { PlayerId = player.Id, HasAim = true, AimX = player.FacingX, AimY = 0f }).ToArray() })
                .ToArray();
            matches.Add(CompleteMatch(count - 1, (uint)(9000 + count), $"roster_{count}_eliminations_and_tiebreak", config, players, turns));
        }
        var hpTieWithHitLead = Players(("a", 3, 0f, 0f), ("b", 3, 4f, 0f));
        hpTieWithHitLead[0].FacingX = -1f;
        hpTieWithHitLead[1].FacingX = 1f;
        hpTieWithHitLead[1].HitsLanded = 1;
        matches.Add(CompleteMatch(5, 9010, "max_rounds_hp_tie_then_hits", config, hpTieWithHitLead, NoHitTurns(config.MaxRounds, hpTieWithHitLead)));

        var completeTie = Players(("a", 3, 0f, 0f), ("b", 3, 4f, 0f));
        completeTie[0].FacingX = -1f;
        completeTie[1].FacingX = 1f;
        matches.Add(CompleteMatch(6, 9011, "max_rounds_shared_tie", config, completeTie, NoHitTurns(config.MaxRounds, completeTie)));
        return matches.ToArray();
    }

    static GoldenMatchRound[] NoHitTurns(int count, GoldenPlayer[] players) => Enumerable.Range(0, count)
        .Select(_ => new GoldenMatchRound { Inputs = players.Select(player => new GoldenInput { PlayerId = player.Id, HasAim = true, AimX = player.FacingX, AimY = player.FacingY }).ToArray() })
        .ToArray();

    static GoldenMatch CompleteMatch(int id, uint seed, string scenario, GameConfig config, GoldenPlayer[] players, GoldenMatchRound[] rounds)
    {
        var match = new MatchState(config, players.Select(player => player.ToCore()));
        foreach (var turn in rounds)
        {
            if (match.Phase == Phase.MatchEnd) break;
            match.StartRound();
            foreach (var input in turn.Inputs) match.SetInput(input.ToCore());
            match.LockRound();
            match.BeginReveal();
            match.BeginThrowing();
            match.ResolveRound();
        }
        return new GoldenMatch
        {
            Id = id, Seed = seed, Scenario = scenario, Players = players, Rounds = rounds,
            Expected = new GoldenMatchExpected
            {
                RoundNumber = match.RoundNumber, Phase = match.Phase.ToString(), WinnerIds = match.WinnerIds.ToArray(),
                Players = match.Players.Select(GoldenPlayer.From).ToArray()
            }
        };
    }

    static GoldenRound RandomRound(int id, uint seed, GameConfig config)
    {
        var random = new SeededRandom(seed);
        var count = 2 + (int)(seed % 5);
        var specs = new (string id, int hp, float x, float y)[count];
        var inputs = new (string id, bool aim, float x, float y)[count];
        for (var i = 0; i < count; i++)
        {
            specs[i] = ($"p{i}", (int)(random.NextUInt() % 4), random.Range(config.MinX, config.MaxX), random.Range(config.MinY, config.MaxY));
            inputs[i] = (specs[i].id, (random.NextUInt() & 1) == 0, random.Range(-1f, 1f), random.Range(-1f, 1f));
        }
        if (Array.TrueForAll(specs, value => value.hp == 0)) specs[0].hp = config.StartHp;
        return Complete(id, seed, "seeded_random", config, Players(specs), Inputs(inputs));
    }

    static GoldenRound Complete(int id, uint seed, string scenario, GameConfig config, GoldenPlayer[] players, GoldenInput[] inputs)
    {
        var corePlayers = players.Select(player => player.ToCore()).ToList();
        var coreInputs = inputs.ToDictionary(input => input.PlayerId, input => input.ToCore());
        var resolution = RoundResolver.Resolve(corePlayers, coreInputs, config);
        return new GoldenRound
        {
            Id = id, Seed = seed, Scenario = scenario, Players = players, Inputs = inputs,
            Expected = new GoldenExpected
            {
                Hits = resolution.Hits.Select(hit => new GoldenHit { ShooterId = hit.ShooterId, TargetId = hit.TargetId, Distance = hit.Distance }).ToArray(),
                Players = corePlayers.Select(GoldenPlayer.From).ToArray()
            }
        };
    }

    static GoldenPlayer[] Players(params (string id, int hp, float x, float y)[] values) => values.Select(value => new GoldenPlayer { Id = value.id, Hp = value.hp, X = value.x, Y = value.y, FacingX = 0f, FacingY = 1f }).ToArray();
    static GoldenInput[] Inputs(params (string id, bool aim, float x, float y)[] values) => values.Select(value => new GoldenInput { PlayerId = value.id, HasAim = value.aim, AimX = value.x, AimY = value.y }).ToArray();
}
