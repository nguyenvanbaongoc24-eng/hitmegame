using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class RoomLoadoutTests
{
    private sealed class Auth : IAuthVerifier
    {
        public bool TryVerify(string? token, out PlayerIdentity identity, out string error)
        {
            identity = new PlayerIdentity(token ?? "", token ?? "");
            error = string.Empty;
            return true;
        }
    }

    private sealed class Profiles : IPlayerProfileReader
    {
        public ProfileSnapshot Read(string playerId) => new(playerId, 0, 0, 0,
            PurchasedItemIds: playerId == "p1" ? new[] { "chao_ran" } : Array.Empty<string>());
    }

    private static ProtocolGateway Create() => new(new RoomDirectory(_ => "HM0001"),
        auth: new Auth(), profiles: new Profiles(),
        loadouts: new RoomLoadoutPolicy(new ItemCatalog(Path.Combine(AppContext.BaseDirectory, "Data", "Items.json"))));

    private static JsonElement Send(ProtocolGateway gateway, string connection, object request) =>
        JsonDocument.Parse(gateway.Handle(connection, JsonSerializer.Serialize(request))).RootElement.Clone();

    [Theory]
    [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public void RoomStateExposesStableSlotsAndSelectedLoadoutsForTwoToSixPeople(int count)
    {
        var gateway = Create();
        var first = Send(gateway, "c1", new { v = 2, type = "join", token = "p1", characterId = "co_ba", weaponId = "chao_ran" });
        Assert.Equal("co_ba", first.GetProperty("players")[0].GetProperty("characterId").GetString());
        for (var i = 2; i <= count; i++)
        {
            var state = Send(gateway, $"c{i}", new { v = 2, type = "join", token = $"p{i}", roomCode = "HM0001", characterId = "co_ba" });
            Assert.Equal(i, state.GetProperty("players").GetArrayLength());
            Assert.Equal(Enumerable.Range(0, i), state.GetProperty("players").EnumerateArray().Select(p => p.GetProperty("slot").GetInt32()));
            Assert.All(state.GetProperty("players").EnumerateArray(), p => Assert.Equal("co_ba", p.GetProperty("characterId").GetString()));
            Assert.Equal("chao_ran", state.GetProperty("players")[0].GetProperty("weaponId").GetString());
        }
    }

    [Theory]
    [InlineData("unknown", null, "character_not_found")]
    [InlineData("quay_boy", null, "character_locked")]
    [InlineData(null, "unknown", "weapon_not_found")]
    [InlineData(null, "chao_ran", "weapon_not_owned")]
    public void JoinRejectsInvalidOrUnownedSelections(string? characterId, string? weaponId, string code)
    {
        var gateway = Create();
        var reply = Send(gateway, "c1", new { v = 2, type = "join", token = "p2", characterId, weaponId });
        Assert.Equal(code, reply.GetProperty("code").GetString());
    }

    [Fact]
    public void DefaultsReadyChangeAndReconnectKeepSelection()
    {
        var gateway = Create();
        var first = Send(gateway, "c1", new { v = 2, type = "join", token = "p1" });
        Assert.Equal("nam_lang", first.GetProperty("players")[0].GetProperty("characterId").GetString());
        Assert.Equal("dep_to_ong", first.GetProperty("players")[0].GetProperty("weaponId").GetString());
        var reconnectToken = first.GetProperty("reconnectToken").GetString();
        var changed = Send(gateway, "c1", new { v = 2, type = "room_ready", ready = true, characterId = "nu_nang_dong", weaponId = "chao_ran" });
        Assert.Equal("nu_nang_dong", changed.GetProperty("players")[0].GetProperty("characterId").GetString());
        Assert.Equal("chao_ran", changed.GetProperty("players")[0].GetProperty("weaponId").GetString());
        gateway.Disconnect("c1");
        var resumed = Send(gateway, "c1r", new { v = 2, type = "join", token = "p1", reconnectToken });
        Assert.Equal("nu_nang_dong", resumed.GetProperty("players")[0].GetProperty("characterId").GetString());
        Assert.Equal("chao_ran", resumed.GetProperty("players")[0].GetProperty("weaponId").GetString());
        Assert.True(resumed.GetProperty("players")[0].GetProperty("ready").GetBoolean());
    }

    [Fact]
    public void InvalidReadySelectionDoesNotChangeExistingSelectionOrReadiness()
    {
        var gateway = Create();
        Send(gateway, "c1", new { v = 2, type = "join", token = "p1" });
        var error = Send(gateway, "c1", new { v = 2, type = "room_ready", ready = true, characterId = "meo_mun" });
        Assert.Equal("character_locked", error.GetProperty("code").GetString());
        var state = Send(gateway, "c1", new { v = 2, type = "room_ready", ready = false });
        Assert.Equal("nam_lang", state.GetProperty("players")[0].GetProperty("characterId").GetString());
        Assert.False(state.GetProperty("players")[0].GetProperty("ready").GetBoolean());
    }
}
