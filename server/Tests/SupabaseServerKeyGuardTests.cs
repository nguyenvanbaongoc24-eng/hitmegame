using System;
using System.Text;
using HitMe.Server.Protocol;
using Xunit;

public sealed class SupabaseServerKeyGuardTests
{
    [Fact]
    public void AcceptsSecretAndLegacyServiceRoleButRejectsClientKeys()
    {
        SupabaseServerKeyGuard.Validate("sb_secret_example");
        var legacy = "e30." + Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"role\":\"service_role\"}")) + ".signature";
        SupabaseServerKeyGuard.Validate(legacy);
        Assert.Throws<InvalidOperationException>(() => SupabaseServerKeyGuard.Validate("sb_publishable_example"));
        Assert.Throws<InvalidOperationException>(() => SupabaseServerKeyGuard.Validate("e30.eyJyb2xlIjoiYW5vbiJ9.signature"));
        Assert.Throws<InvalidOperationException>(() => SupabaseServerKeyGuard.Validate(string.Empty));
    }
}
