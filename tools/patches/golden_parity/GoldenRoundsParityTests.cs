/* AG integration: copy this file into an NUnit EditMode assembly and copy
 * server/Tests/golden/rounds.json to Assets/Tests/Data/golden/rounds.json.
 * The relative path below deliberately uses Application.dataPath in Unity;
 * no absolute machine path is required. This patch verifies round-level Core
 * parity (hits and every player state), not network transport or UI state.
 */
using System;
using System.Collections.Generic;
using System.IO;
using HitMe.Core;
using NUnit.Framework;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#else
using System.Text.Json;
#endif

public sealed class GoldenRoundsParityTests
{
    const float Epsilon = 0.0001f;

    [Test]
    public void EveryGoldenRoundMatchesPhase1Core()
    {
        var file = GoldenJson.Read(GoldenJsonPath());
        Assert.That(file.SchemaVersion, Is.EqualTo(1));
        Assert.That(file.GameConfigVersion, Is.EqualTo("phase1-v1.2"));
        Assert.That(file.Rounds, Has.Length.EqualTo(200));

        for (var roundIndex = 0; roundIndex < file.Rounds.Length; roundIndex++)
        {
            var round = file.Rounds[roundIndex];
            var players = new List<PlayerState>();
            var inputs = new Dictionary<string, RoundInput>();
            for (var index = 0; index < round.Players.Length; index++) players.Add(round.Players[index].ToCore());
            for (var index = 0; index < round.Inputs.Length; index++) inputs.Add(round.Inputs[index].PlayerId, round.Inputs[index].ToCore());
            var actual = RoundResolver.Resolve(players, inputs, file.Config.ToCore());
            var at = $"round {round.Id} ({round.Scenario})";

            Assert.That(actual.Hits, Has.Count.EqualTo(round.Expected.Hits.Length), $"{at}: hit count");
            for (var index = 0; index < actual.Hits.Count; index++)
            {
                Assert.That(actual.Hits[index].ShooterId, Is.EqualTo(round.Expected.Hits[index].ShooterId), $"{at}: hit {index} shooter");
                Assert.That(actual.Hits[index].TargetId, Is.EqualTo(round.Expected.Hits[index].TargetId), $"{at}: hit {index} target");
                Assert.That(actual.Hits[index].Distance, Is.EqualTo(round.Expected.Hits[index].Distance).Within(Epsilon), $"{at}: hit {index} distance");
            }
            Assert.That(players, Has.Count.EqualTo(round.Expected.Players.Length), $"{at}: player count");
            for (var index = 0; index < players.Count; index++) AssertPlayer(round.Expected.Players[index], players[index], at, index);
        }
    }

    static string GoldenJsonPath()
    {
#if UNITY_5_3_OR_NEWER
        return Path.Combine(Application.dataPath, "Tests", "Data", "golden", "rounds.json");
#else
        return Path.Combine(AppContext.BaseDirectory, "golden", "rounds.json");
#endif
    }

    static void AssertPlayer(GoldenPlayer expected, PlayerState actual, string at, int index)
    {
        Assert.That(actual.Id, Is.EqualTo(expected.Id), $"{at}: player {index} id");
        Assert.That(actual.Hp, Is.EqualTo(expected.Hp), $"{at}: player {index} hp");
        Assert.That(actual.HitsLanded, Is.EqualTo(expected.HitsLanded), $"{at}: player {index} hits landed");
        Assert.That(actual.Position.X, Is.EqualTo(expected.X).Within(Epsilon), $"{at}: player {index} x");
        Assert.That(actual.Position.Y, Is.EqualTo(expected.Y).Within(Epsilon), $"{at}: player {index} y");
        Assert.That(actual.Facing.X, Is.EqualTo(expected.FacingX).Within(Epsilon), $"{at}: player {index} facing x");
        Assert.That(actual.Facing.Y, Is.EqualTo(expected.FacingY).Within(Epsilon), $"{at}: player {index} facing y");
    }
}

[Serializable] public sealed class GoldenRoundsFile { public int SchemaVersion; public string GameConfigVersion = ""; public GoldenGameConfig Config = new(); public GoldenRound[] Rounds = Array.Empty<GoldenRound>(); }
[Serializable] public sealed class GoldenGameConfig
{
    public int StartHp; public float ArenaWidth; public float ArenaHeight; public float PlayerRadius; public int DamagePerHit; public int MaxHitsPerThrow; public int MaxRounds; public float MinSpawnDistance;
    public GameConfig ToCore() => new() { StartHp = StartHp, ArenaWidth = ArenaWidth, ArenaHeight = ArenaHeight, PlayerRadius = PlayerRadius, DamagePerHit = DamagePerHit, MaxHitsPerThrow = MaxHitsPerThrow, MaxRounds = MaxRounds, MinSpawnDistance = MinSpawnDistance };
}
[Serializable] public sealed class GoldenRound { public int Id; public uint Seed; public string Scenario = ""; public GoldenPlayer[] Players = Array.Empty<GoldenPlayer>(); public GoldenInput[] Inputs = Array.Empty<GoldenInput>(); public GoldenExpected Expected = new(); }
[Serializable] public sealed class GoldenPlayer
{
    public string Id = ""; public int Hp; public float X; public float Y; public float FacingX; public float FacingY; public int HitsLanded;
    public PlayerState ToCore() => new(Id, Id, false, Hp, new Vec2(X, Y), new Vec2(FacingX, FacingY)) { HitsLanded = HitsLanded };
}
[Serializable] public sealed class GoldenInput { public string PlayerId = ""; public bool HasAim; public float AimX; public float AimY; public RoundInput ToCore() => new(PlayerId) { HasAim = HasAim, AimDirection = new Vec2(AimX, AimY), IsReady = true }; }
[Serializable] public sealed class GoldenHit { public string ShooterId = ""; public string TargetId = ""; public float Distance; }
[Serializable] public sealed class GoldenExpected { public GoldenHit[] Hits = Array.Empty<GoldenHit>(); public GoldenPlayer[] Players = Array.Empty<GoldenPlayer>(); }

public static class GoldenJson
{
    public static GoldenRoundsFile Read(string path)
    {
        var json = File.ReadAllText(path);
#if UNITY_5_3_OR_NEWER
        var parsed = JsonUtility.FromJson<GoldenRoundsFile>(json);
#else
        var parsed = JsonSerializer.Deserialize<GoldenRoundsFile>(json, new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true });
#endif
        if (parsed == null) throw new InvalidOperationException("Invalid golden rounds JSON.");
        return parsed;
    }
}
