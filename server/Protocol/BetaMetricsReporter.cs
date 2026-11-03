using System.Net.Http.Json;
using HitMe.Server.Authority;

namespace HitMe.Server.Protocol;

public sealed class SupabaseBetaMetricsStore
{
    private readonly Uri endpoint;
    private readonly string key;
    private readonly HttpClient http;

    public SupabaseBetaMetricsStore(Uri projectUrl, string secretKey, HttpClient? http = null)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("HTTPS required.");
        SupabaseServerKeyGuard.Validate(secretKey);
        endpoint = new Uri(projectUrl, "/rest/v1/beta_events");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    }

    public async Task<bool> SaveAsync(object counters, string serverVersion, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new { server_version = serverVersion, counters })
        };
        request.Headers.TryAddWithoutValidation("apikey", key);
        using var response = await http.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

public sealed class BetaMetricsReporter : BackgroundService
{
    private readonly SupabaseBetaMetricsStore store;
    private readonly ServerMetrics metrics;
    private readonly RoomDirectory rooms;
    private readonly WebSocketConnectionRegistry connections;
    private readonly ClientVersionPolicy versions;
    private readonly ILogger<BetaMetricsReporter> logger;

    public BetaMetricsReporter(SupabaseBetaMetricsStore store, ServerMetrics metrics, RoomDirectory rooms,
        WebSocketConnectionRegistry connections, ClientVersionPolicy versions, ILogger<BetaMetricsReporter> logger)
    {
        this.store = store;
        this.metrics = metrics;
        this.rooms = rooms;
        this.connections = connections;
        this.versions = versions;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var snapshot = rooms.Snapshot();
                var counters = metrics.Snapshot(connections.Count, snapshot.Count,
                    snapshot.Count(room => room.RoundSession is { } active &&
                        active.Match.State.Phase != HitMe.Core.Phase.MatchEnd));
                if (!await store.SaveAsync(counters, versions.ServerVersion, stoppingToken))
                    logger.LogWarning("Beta metrics snapshot was rejected by storage");
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning("Beta metrics snapshot could not reach storage");
            }
        }
    }
}
