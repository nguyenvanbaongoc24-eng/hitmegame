using System;
using HitMe.Server.Authority;
using Xunit;

public sealed class ApprovedGoldByRoomSizeTests
{
    [Theory]
    [InlineData(2, 100, 0, 0)]
    [InlineData(3, 150, 80, 40)]
    [InlineData(4, 150, 80, 40)]
    [InlineData(5, 200, 120, 60)]
    [InlineData(6, 200, 120, 60)]
    public void ApprovedAmountsDependOnRoomSize(int count, int first, int second, int third)
    {
        var policy = new ApprovedGoldByRoomSize(count);
        Assert.Equal(first, policy.GoldFor(new MatchPlayerResult("a", 1, 1, 1)));
        Assert.Equal(second, policy.GoldFor(new MatchPlayerResult("b", 2, 1, 1)));
        Assert.Equal(third, policy.GoldFor(new MatchPlayerResult("c", 3, 1, 1)));
        Assert.Equal(0, policy.GoldFor(new MatchPlayerResult("d", 4, 1, 1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    public void RejectsUnsupportedRoomSize(int count) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ApprovedGoldByRoomSize(count));
}
