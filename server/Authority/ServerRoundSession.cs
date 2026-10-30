using System.Text.Json;
using HitMe.Core;

namespace HitMe.Server.Authority;

public sealed record OutboundMessage(string PlayerId, string Payload);

/// <summary>
/// Owns the protocol boundary of a running round. No private commit is ever
/// serialized before Round.IsLocked becomes true.
/// </summary>
public sealed class ServerRoundSession
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly ServerMatch match;
    private readonly string[] playerIds;
    private readonly AfkPolicy afkPolicy;
    private readonly IBanStore banStore;
    private readonly HashSet<string> connectedPlayers;
    private readonly Dictionary<string, int> missedCommitRounds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTimeOffset> lastEmoteAt = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> emotesThisRound = new(StringComparer.Ordinal);
    private TimeProvider? timeProvider;
    private Round? round;
    private int publishedRoundId;

    public ServerRoundSession(GameConfig config, IEnumerable<string> playerIds, AfkPolicy? afkPolicy = null, IBanStore? banStore = null)
    {
        this.playerIds = playerIds?.ToArray() ?? throw new ArgumentNullException(nameof(playerIds));
        match = new ServerMatch(config, this.playerIds);
        this.afkPolicy = afkPolicy ?? new AfkPolicy();
        this.banStore = banStore ?? new InMemoryBanStore();
        connectedPlayers = new HashSet<string>(this.playerIds, StringComparer.Ordinal);
    }

    public ServerMatch Match => match;
    public Round ActiveRound => round ?? throw new InvalidOperationException("No active round.");

    public IReadOnlyList<OutboundMessage> BeginRound(TimeProvider? clock = null)
    {
        // Retain the room's server clock so automatically started later rounds
        // use the same clock as the first placement window.
        timeProvider = clock ?? timeProvider ?? TimeProvider.System;
        var active = match.BeginRound(timeProvider);
        round = active;
        emotesThisRound.Clear();
        return Broadcast(new
        {
            v = 1,
            type = "round_start",
            roundId = active.RoundId,
            deadline = active.Deadline.ToUnixTimeMilliseconds()
        });
    }

    public bool TryCommit(string playerId, RoundCommit commit, out string error)
    {
        return ActiveRound.TryCommit(playerId, commit, out error);
    }

    public IReadOnlyList<OutboundMessage> TryMarkReady(string playerId, out string error)
    {
        if (!ActiveRound.TryMarkReady(playerId, out error)) return Array.Empty<OutboundMessage>();
        return PublishIfLocked();
    }

    public IReadOnlyList<OutboundMessage> Tick()
    {
        ActiveRound.TryLockIfDue();
        return PublishIfLocked();
    }

    public void SetConnected(string playerId, bool connected)
    {
        if (connected) connectedPlayers.Add(playerId);
        else
        {
            connectedPlayers.Remove(playerId);
            // A disconnect is handled by reconnect grace, never as AFK.
            missedCommitRounds.Remove(playerId);
        }
    }

    /// <summary>Expired reconnect grace removes a player from the match without an AFK ban.</summary>
    public void EliminateDisconnected(string playerId)
    {
        connectedPlayers.Remove(playerId);
        var player = match.State.FindPlayer(playerId);
        if (player != null) player.Hp = 0;
    }

    public bool TrySendEmote(string playerId, string? emoteId, DateTimeOffset now, out string error)
    {
        if (round is null || round.IsLocked || match.State.Phase != Phase.Placing) { error = "wrong_phase"; return false; }
        if (!connectedPlayers.Contains(playerId) || match.State.FindPlayer(playerId)?.IsAlive != true) { error = "not_in_round"; return false; }
        if (string.IsNullOrWhiteSpace(emoteId) || !AllowedEmotes.Contains(emoteId)) { error = "invalid_emote"; return false; }
        if (lastEmoteAt.TryGetValue(playerId, out var last) && now - last < TimeSpan.FromSeconds(1)) { error = "emote_rate_limited"; return false; }
        var count = emotesThisRound.TryGetValue(playerId, out var sent) ? sent : 0;
        if (count >= 3) { error = "emote_phase_limit"; return false; }
        lastEmoteAt[playerId] = now;
        emotesThisRound[playerId] = count + 1;
        error = string.Empty;
        return true;
    }

    public static readonly ISet<string> AllowedEmotes = new HashSet<string>(StringComparer.Ordinal)
    { "happy", "sad", "angry", "surprised", "thumbs_up" };

    private IReadOnlyList<OutboundMessage> PublishIfLocked()
    {
        var active = ActiveRound;
        if (!active.IsLocked || publishedRoundId == active.RoundId) return Array.Empty<OutboundMessage>();

        ApplyAfkBeforeResolution(active);
        var resolution = match.ResolveLockedRound();
        publishedRoundId = active.RoundId;
        var messages = new List<OutboundMessage>();
        messages.AddRange(Broadcast(new
        {
            v = 1,
            type = "reveal",
            roundId = active.RoundId,
            players = match.State.Players.Select(player => new
            {
                id = player.Id,
                position = new { x = player.Position.X, y = player.Position.Y },
                aim = match.State.LockedInputs.TryGetValue(player.Id, out var input)
                    ? new { x = input.AimDirection.X, y = input.AimDirection.Y }
                    : null
            })
        }));
        messages.AddRange(Broadcast(new
        {
            v = 1,
            type = "round_result",
            roundId = active.RoundId,
            hits = resolution.Hits.Select(hit => new
            {
                shooterId = hit.ShooterId,
                targetId = hit.TargetId,
                point = new { x = hit.Point.X, y = hit.Point.Y },
                distance = hit.Distance
            }),
            players = match.State.Players.Select(player => new
            {
                id = player.Id,
                hp = player.Hp,
                eliminated = !player.IsAlive
            })
        }));

        if (match.State.Phase == Phase.MatchEnd)
        {
            messages.AddRange(Broadcast(new
            {
                v = 1,
                type = "match_end",
                winners = match.State.WinnerIds
            }));
        }
        else
        {
            // The next placement window is server-owned too.  Starting it here
            // guarantees that a full multiplayer match progresses beyond round 1.
            messages.AddRange(BeginRound());
        }

        return messages;
    }

    private void ApplyAfkBeforeResolution(Round active)
    {
        foreach (var playerId in playerIds)
        {
            var player = match.State.FindPlayer(playerId);
            if (player?.IsAlive != true || !connectedPlayers.Contains(playerId)) continue;
            if (active.Commits.ContainsKey(playerId))
            {
                missedCommitRounds.Remove(playerId);
                continue;
            }

            var misses = missedCommitRounds.TryGetValue(playerId, out var previous) ? previous + 1 : 1;
            missedCommitRounds[playerId] = misses;
            if (misses < afkPolicy.RoundsWithoutValidCommitBeforeElimination) continue;
            player.Hp = 0;
            banStore.Ban(playerId, (active.LockedAt ?? active.Deadline) + TimeSpan.FromSeconds(afkPolicy.BanSeconds));
        }
    }

    private IReadOnlyList<OutboundMessage> Broadcast(object payload)
    {
        var json = JsonSerializer.Serialize(payload, Json);
        return playerIds.Select(playerId => new OutboundMessage(playerId, json)).ToArray();
    }
}
