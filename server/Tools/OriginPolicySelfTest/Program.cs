using HitMe.Server.Protocol;

var exact = new OriginPolicy(new[] { "https://hitmegame-iota.vercel.app" });
var preview = new OriginPolicy(null, allowMissingOrigin: true, allowVercelPreviewOrigins: true);
var empty = new OriginPolicy();
var checks = new (string Name, bool Actual, bool Expected)[]
{
    ("exact HTTPS origin", exact.IsAllowed("https://hitmegame-iota.vercel.app"), true),
    ("case and trailing slash normalization", exact.IsAllowed("HTTPS://HITMEGAME-IOTA.VERCEL.APP/"), true),
    ("default HTTPS port normalization", exact.IsAllowed("https://hitmegame-iota.vercel.app:443"), true),
    ("different port rejected", exact.IsAllowed("https://hitmegame-iota.vercel.app:8443"), false),
    ("http versus https rejected", exact.IsAllowed("http://hitmegame-iota.vercel.app"), false),
    ("foreign origin rejected", exact.IsAllowed("https://foreign.example"), false),
    ("missing origin rejected by default", exact.IsAllowed(null), false),
    ("empty config rejects browser", empty.IsAllowed("https://hitmegame-iota.vercel.app"), false),
    ("empty config rejects missing origin", empty.IsAllowed(""), false),
    ("preview rejected while disabled", exact.IsAllowed("https://hitmegame-pr-123.vercel.app"), false),
    ("preview allowed only with flag", preview.IsAllowed("https://hitmegame-pr-123.vercel.app"), true),
    ("missing origin allowed only with flag", preview.IsAllowed(null), true),
};
var failed = checks.Where(check => check.Actual != check.Expected).ToArray();
foreach (var check in checks) Console.WriteLine($"{(check.Actual == check.Expected ? "PASS" : "FAIL")}: {check.Name}");
if (failed.Length > 0)
{
    Console.Error.WriteLine($"FAILED {failed.Length}/{checks.Length}: {string.Join(", ", failed.Select(item => item.Name))}");
    Environment.ExitCode = 1;
}
else Console.WriteLine($"Passed! {checks.Length}/{checks.Length} OriginPolicy self-test");
