using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HitMe.Server.Protocol;

/// <summary>
/// Stateless, signed guest identity. Stable across restarts only while the
/// server operator retains the same GUEST_TOKEN_SIGNING_KEY. This is not a
/// Supabase account and never grants wallet ownership.
/// </summary>
public sealed class SignedGuestAuthVerifier : IAuthVerifier
{
    private readonly byte[] key;
    private readonly TimeProvider clock;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public SignedGuestAuthVerifier(byte[] key, TimeProvider? clock = null)
    {
        if (key is null || key.Length < 32) throw new ArgumentException("Signing key must contain at least 32 bytes.", nameof(key));
        this.key = (byte[])key.Clone();
        this.clock = clock ?? TimeProvider.System;
    }

    public string Issue(string displayName, TimeSpan? lifetime = null)
    {
        var safeName = FakeAuthVerifier.SanitizeDisplayName(displayName ?? string.Empty);
        if (safeName.Length is < 1 or > 24) throw new ArgumentException("Guest name is required.", nameof(displayName));
        var duration = lifetime ?? TimeSpan.FromDays(30);
        if (duration <= TimeSpan.Zero || duration > TimeSpan.FromDays(30)) throw new ArgumentOutOfRangeException(nameof(lifetime));
        var claims = new GuestClaims("g_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant(),
            safeName, clock.GetUtcNow().Add(duration).ToUnixTimeSeconds());
        var payload = Base64Url(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(claims, Json)));
        var signedPart = "guest1." + payload;
        return signedPart + "." + Base64Url(HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(signedPart)));
    }

    public bool TryVerify(string? token, out PlayerIdentity identity, out string error)
    {
        identity = null!;
        error = "auth_invalid";
        if (string.IsNullOrEmpty(token)) { error = "auth_required"; return false; }
        if (token.Length > 512 || !token.StartsWith("guest1.", StringComparison.Ordinal)) return false;
        var separator = token.LastIndexOf('.');
        if (separator <= "guest1.".Length || separator == token.Length - 1) return false;
        var signedPart = token[..separator];
        try
        {
            var supplied = FromBase64Url(token[(separator + 1)..]);
            var expected = HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(signedPart));
            if (supplied.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(supplied, expected)) return false;
            var claims = JsonSerializer.Deserialize<GuestClaims>(FromBase64Url(token["guest1.".Length..separator]), Json);
            if (claims is null || claims.Id.Length != 34 || !claims.Id.StartsWith("g_", StringComparison.Ordinal)
                || !claims.Id[2..].All(Uri.IsHexDigit) || claims.Name.Length is < 1 or > 24
                || FakeAuthVerifier.SanitizeDisplayName(claims.Name) != claims.Name
                || claims.Expires <= clock.GetUtcNow().ToUnixTimeSeconds()) return false;
            identity = new PlayerIdentity(claims.Id, claims.Name);
            error = string.Empty;
            return true;
        }
        catch (Exception exception) when (exception is FormatException or JsonException or ArgumentException)
        { return false; }
    }

    private sealed record GuestClaims(string Id, string Name, long Expires);
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] FromBase64Url(string input)
    {
        if (input.Length % 4 == 1 || input.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_'))
            throw new FormatException("Invalid base64url token.");
        var padded = input.Replace('-', '+').Replace('_', '/').PadRight((input.Length + 3) / 4 * 4, '=');
        return Convert.FromBase64String(padded);
    }
}
