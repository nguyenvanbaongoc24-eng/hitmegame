using System.Text.Json;
using HitMe.Core;

var samples = args.Length > 0 ? int.Parse(args[0]) : 1000;
if (samples < 1) throw new ArgumentOutOfRangeException(nameof(samples));
var rows = new List<object>();
for (var count = 2; count <= 6; count++)
for (var level = 0; level < 3; level++)
{
    var rounds = new int[samples];
    var sudden = 0;
    var hardcap = 0;
    var tiebreak = 0;
    for (var sample = 0; sample < samples; sample++)
    {
        var config = new GameConfig { MaxPlayers = count, Seed = (uint)(100000 + count * 10000 + level * 1000 + sample) };
        var random = new SeededRandom(config.Seed);
        var players = new List<PlayerState>();
        var brains = new List<Phase1BotBrain>();
        for (var i = 0; i < count; i++)
        {
            var position = new Vec2(random.Range(config.MinX, config.MaxX), random.Range(config.MinY, config.MaxY));
            players.Add(new PlayerState($"bot-{i}", $"Bot {i + 1}", true, config.StartHp, position, Vec2.Up));
            brains.Add(new Phase1BotBrain((BotLevel)level, random));
        }
        var match = new MatchState(config, players);
        var guard = 0;
        while (match.Phase != Phase.MatchEnd && guard++ < 50)
        {
            match.StartRound();
            foreach (var (player, index) in players.Select((p, i) => (p, i)))
                if (player.IsAlive) match.SetInput(brains[index].MakeInput(player, config));
            match.LockRound();
            match.BeginReveal();
            match.BeginThrowing();
            match.ResolveRound();
        }
        if (match.Phase != Phase.MatchEnd || match.WinnerIds.Count < 1)
            throw new InvalidOperationException($"Unfinished match count={count} level={level} sample={sample}");
        rounds[sample] = match.RoundNumber;
        if (match.NormalRoundNumber >= config.SuddenDeathRound) sudden++;
        if (match.NormalRoundNumber >= config.HardCapRound) hardcap++;
        if (match.TiebreakCount > 0) tiebreak++;
    }
    Array.Sort(rounds);
    rows.Add(new {
        players = count,
        difficulty = ((BotLevel)level).ToString(),
        matches = samples,
        median = rounds[(samples - 1) / 2],
        p90 = rounds[(int)Math.Ceiling(samples * .9) - 1],
        max = rounds[^1],
        suddenDeathRate = (double)sudden / samples,
        hardCapRate = (double)hardcap / samples,
        tiebreakRate = (double)tiebreak / samples
    });
}
Console.WriteLine(JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
