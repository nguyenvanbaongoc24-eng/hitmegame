using HitMe.Server.Authority;
using HitMe.Server.Protocol;

var project = Environment.GetEnvironmentVariable("SUPABASE_URL");
var key = Environment.GetEnvironmentVariable("SUPABASE_SECRET_KEY");
if (string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(key))
    throw new InvalidOperationException("Set SUPABASE_URL and SUPABASE_SECRET_KEY in the process environment.");
SupabaseServerKeyGuard.Validate(key);
var id = "test_codex_ban_" + Guid.NewGuid().ToString("N");
var now = DateTimeOffset.UtcNow;
var until = now.AddSeconds(120);
var url = new Uri(project);
var store = new SupabaseBanStore(url, key);
try
{
    store.Ban(id, until);
    if (!store.TryGetRemaining(id, now, out var initial) || initial != 120)
        throw new InvalidOperationException("Initial ban duration differs from 120 seconds.");
    // A fresh adapter has no local state, matching the persistence path after restart.
    var restarted = new SupabaseBanStore(url, key);
    if (!restarted.TryGetRemaining(id, now.AddSeconds(20), out var resumed) || resumed != 100)
        throw new InvalidOperationException("Fresh adapter did not retain the ban.");
    if (!restarted.TryGetRemaining(id, now.AddSeconds(18), out var slowClock) || slowClock != 102)
        throw new InvalidOperationException("Slow-clock boundary differs.");
    if (!restarted.TryGetRemaining(id, now.AddSeconds(22), out var fastClock) || fastClock != 98)
        throw new InvalidOperationException("Fast-clock boundary differs.");
    if (restarted.TryGetRemaining(id, now.AddSeconds(121), out _))
        throw new InvalidOperationException("Expired ban was still active.");
    Console.WriteLine("Passed! 5/5 live Supabase ban checks (test-prefixed row)");
}
finally
{
    // Only this run's test-prefixed ID is removed; no production ban can match it.
    using var http = new HttpClient();
    using var request = new HttpRequestMessage(HttpMethod.Delete,
        new Uri(url, "/rest/v1/server_bans?player_id=eq." + id));
    request.Headers.TryAddWithoutValidation("apikey", key);
    using var response = http.Send(request);
    if (!response.IsSuccessStatusCode)
        throw new HttpRequestException($"Test-row cleanup returned HTTP {(int)response.StatusCode}; inspect test-prefixed row.");
    Console.WriteLine("test-row cleanup: ok");
}
