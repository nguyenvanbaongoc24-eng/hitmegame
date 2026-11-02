using System.Text.Json;

namespace HitMe.Server.Authority;

/// <summary>
/// Local durable fallback for AFK bans. A shared database is required when
/// multiple server instances serve the same players.
/// </summary>
public sealed class FileBanStore : IBanStore
{
    private readonly object gate = new();
    private readonly string path;
    private readonly Dictionary<string, DateTimeOffset> bans;

    public FileBanStore(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Ban file path is required.", nameof(path));
        this.path = Path.GetFullPath(path);
        bans = File.Exists(this.path)
            ? JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset>>(File.ReadAllText(this.path))
                ?? new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal)
            : new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);
    }

    public void Ban(string playerId, DateTimeOffset until)
    {
        if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player ID is required.", nameof(playerId));
        lock (gate)
        {
            bans[playerId] = until;
            Save();
        }
    }

    public bool TryGetRemaining(string playerId, DateTimeOffset now, out int remainingSeconds)
    {
        lock (gate)
        {
            if (!bans.TryGetValue(playerId, out var until)) { remainingSeconds = 0; return false; }
            if (until <= now)
            {
                bans.Remove(playerId);
                Save();
                remainingSeconds = 0;
                return false;
            }
            remainingSeconds = Math.Max(1, (int)Math.Ceiling((until - now).TotalSeconds));
            return true;
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(bans));
        File.Move(temporary, path, true);
    }
}
