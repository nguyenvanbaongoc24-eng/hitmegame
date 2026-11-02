using System.Text.Json;

namespace HitMe.Server.Protocol;

/// <summary>Reject public client keys before a persistent server starts.</summary>
public static class SupabaseServerKeyGuard
{
    public static void Validate(string? value)
    {
        if (!string.IsNullOrEmpty(value) && value.StartsWith("sb_secret_", StringComparison.Ordinal)
            && value.Length > "sb_secret_".Length) return;

        // Older projects may still use a JWT service_role key. The role check
        // catches an accidental anon JWT; Supabase verifies the signature.
        if (!string.IsNullOrEmpty(value))
        {
            var pieces = value.Split('.');
            if (pieces.Length == 3)
            {
                try
                {
                    var encoded = pieces[1].Replace('-', '+').Replace('_', '/');
                    var payload = Convert.FromBase64String(encoded.PadRight((encoded.Length + 3) / 4 * 4, '='));
                    using var claims = JsonDocument.Parse(payload);
                    if (claims.RootElement.TryGetProperty("role", out var role)
                        && role.GetString() == "service_role") return;
                }
                catch (Exception exception) when (exception is FormatException or JsonException)
                { /* invalid public or malformed key */ }
            }
        }
        throw new InvalidOperationException("Supabase persistence requires a server-only Secret key, not a publishable or anon key.");
    }
}
