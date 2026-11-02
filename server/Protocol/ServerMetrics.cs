using System.Collections.Concurrent;

namespace HitMe.Server.Protocol;

/// <summary>Aggregate operational counters only; never records raw payloads, tokens or positions.</summary>
public sealed class ServerMetrics
{
    private const int MaxSamples = 1024;
    private readonly ConcurrentDictionary<string, long> errors = new(StringComparer.Ordinal);
    private readonly Queue<double> broadcastMilliseconds = new();
    private readonly Queue<double> roundResolutionMilliseconds = new();
    private readonly object gate = new();

    public void RecordError(string code) => errors.AddOrUpdate(code, 1, (_, count) => count + 1);

    public void RecordBroadcast(double milliseconds)
    {
        lock (gate)
        {
            broadcastMilliseconds.Enqueue(milliseconds);
            while (broadcastMilliseconds.Count > MaxSamples) broadcastMilliseconds.Dequeue();
        }
    }

    public void RecordRoundResolution(double milliseconds)
    {
        lock (gate)
        {
            roundResolutionMilliseconds.Enqueue(milliseconds);
            while (roundResolutionMilliseconds.Count > MaxSamples) roundResolutionMilliseconds.Dequeue();
        }
    }

    public object Snapshot(int connections, int rooms, int activeMatches)
    {
        double p95;
        int samples;
        double roundMedian;
        int roundSamples;
        lock (gate)
        {
            var sorted = broadcastMilliseconds.OrderBy(value => value).ToArray();
            samples = sorted.Length;
            p95 = samples == 0 ? 0 : sorted[(int)Math.Ceiling(samples * .95) - 1];
            var rounds = roundResolutionMilliseconds.OrderBy(value => value).ToArray();
            roundSamples = rounds.Length;
            roundMedian = roundSamples == 0 ? 0 : (rounds[(roundSamples - 1) / 2] + rounds[roundSamples / 2]) / 2;
        }
        return new
        {
            connections, rooms, activeMatches,
            broadcastP95Ms = Math.Round(p95, 3), broadcastSamples = samples,
            roundResolutionMedianMs = Math.Round(roundMedian, 3), roundResolutionSamples = roundSamples,
            errorsByCode = errors.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value)
        };
    }
}
