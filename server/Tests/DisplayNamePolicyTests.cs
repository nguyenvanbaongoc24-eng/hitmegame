using System;
using System.Linq;
using HitMe.Server.Protocol;
using HitMe.Server.Authority;
using Xunit;

public sealed class DisplayNamePolicyTests
{
    [Theory]
    [InlineData("Nữ", true)]
    [InlineData("Nam Làng", true)]
    [InlineData("A", false)]
    [InlineData("<Admin>", false)]
    [InlineData("moderator", false)]
    [InlineData("Nam\nB", false)]
    [InlineData("Nam👑", false)]
    public void EnforcesGuestNameCharactersAndTerms(string name, bool expected) =>
        Assert.Equal(expected, new DisplayNamePolicy().IsAllowed(name));

    [Fact]
    public void TermsCanBeConfiguredWithoutHardCodedRoomNames()
    {
        var policy = DisplayNamePolicy.FromEnvironment(key => key == "BANNED_NAME_TERMS" ? "spam,scam" : null);
        Assert.False(policy.IsAllowed("Spam User"));
        Assert.True(policy.IsAllowed("Moderator"));
    }

    [Fact]
    public void SameNameCannotJoinOneRoomWithDifferentCasing()
    {
        var rooms = new RoomDirectory();
        var room = rooms.Create("a", "Nam Làng", null, out _);
        Assert.False(rooms.Join(room.Code, "b", "NAM LÀNG", out _, out var error));
        Assert.Equal("name_taken_in_room", error);
    }

    [Fact]
    public void SignedGuestIssuerRejectsInvalidName()
    {
        var issuer = new SignedGuestAuthVerifier(Enumerable.Repeat((byte)7, 32).ToArray());
        Assert.Throws<ArgumentException>(() => issuer.Issue("admin one"));
        Assert.Throws<ArgumentException>(() => issuer.Issue("a"));
    }
}
