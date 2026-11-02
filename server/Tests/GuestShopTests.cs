using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class GuestShopTests
{
    private static ItemCatalog Catalog() => new(Path.Combine(AppContext.BaseDirectory, "Data", "Items.json"));
    private static MatchResult Reward(string playerId) => new(Guid.NewGuid(), DateTimeOffset.UtcNow,
        ImmutableArray.Create(new MatchPlayerResult(playerId, 1, 3, 1),
            new MatchPlayerResult("other", 2, 2, 0)), 2, MatchResultFactory.ConfigVersion);

    [Fact]
    public void BuyAndEquipUseServerPriceAndIdempotentRequestId()
    {
        const string player = "g_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var profiles = new InMemoryProfileStore();
        Assert.True(profiles.TryApplyMatch(Reward(player), new FixedGoldByRank(new[] { 500, 0 })));
        var shop = new InMemoryGuestShopService(profiles, Catalog());
        var buy = new ShopRequest("buy", Guid.NewGuid(), "chao_ran", null);
        Assert.Equal(301, shop.Execute(player, buy).Gold);
        Assert.Equal(301, shop.Execute(player, buy).Gold);
        Assert.Equal("request_id_reused", Assert.Throws<ShopException>(() =>
            shop.Execute(player, buy with { ItemId = "vot_pickleball" })).Code);
        Assert.Equal("already_owned", Assert.Throws<ShopException>(() =>
            shop.Execute(player, buy with { RequestId = Guid.NewGuid() })).Code);
        Assert.Equal("chao_ran", shop.Execute(player,
            new ShopRequest("equip", Guid.NewGuid(), "chao_ran", null)).EquippedWeaponId);
        Assert.Contains("chao_ran", profiles.GetOrCreate(player).PurchasedItemIds!);
    }

    [Fact]
    public void RenameFirstFreeThenCostsConfiguredGoldAndRejectsInvalidNames()
    {
        const string player = "g_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        var profiles = new InMemoryProfileStore { RenameCostAfterFirst = 200 };
        Assert.True(profiles.TryApplyMatch(Reward(player), new FixedGoldByRank(new[] { 500, 0 })));
        var shop = new InMemoryGuestShopService(profiles, Catalog());
        var first = new ShopRequest("rename", Guid.NewGuid(), null, "First Name");
        Assert.Equal(500, shop.Execute(player, first).Gold);
        Assert.Equal(500, shop.Execute(player, first).Gold);
        var second = shop.Execute(player, new ShopRequest("rename", Guid.NewGuid(), null, "Second Name"));
        Assert.Equal(300, second.Gold);
        Assert.Equal(2, second.RenameCount);
        Assert.Equal("invalid_name", Assert.Throws<ShopException>(() => shop.Execute(player,
            new ShopRequest("rename", Guid.NewGuid(), null, "<script>"))).Code);
    }

    [Fact]
    public void V2ShopRequiresJoinAndReturnsAuthoritativeProfile()
    {
        var profiles = new InMemoryProfileStore();
        var gateway = new ProtocolGateway(new RoomDirectory(), profiles: new InMemoryPlayerProfileReader(profiles),
            shop: new InMemoryGuestShopService(profiles, Catalog()));
        var id = Guid.NewGuid();
        static JsonElement Parse(string payload) => JsonDocument.Parse(payload).RootElement.Clone();
        var command = JsonSerializer.Serialize(new { v = 2, type = "rename_player", requestId = id, newName = "New Name" });
        Assert.Equal("not_joined", Parse(gateway.Handle("a", command)).GetProperty("code").GetString());
        gateway.Handle("a", JsonSerializer.Serialize(new { v = 2, type = "join", token = "guest:alice" }));
        var result = gateway.HandleMessages("a", command);
        Assert.Equal(2, result.Count);
        Assert.Equal("shop_result", Parse(result[0].Payload).GetProperty("type").GetString());
        var profile = Parse(result[1].Payload);
        Assert.Equal("New Name", profile.GetProperty("name").GetString());
        Assert.Equal(0, profile.GetProperty("gold").GetInt32());
        Assert.Equal("authoritative", profile.GetProperty("inventoryStatus").GetString());
        Assert.Equal("invalid_shop_request", Parse(gateway.Handle("a",
            "{\"v\":2,\"type\":\"buy_item\",\"requestId\":\"not-a-guid\",\"itemId\":\"chao_ran\"}"))
            .GetProperty("code").GetString());
    }
}
