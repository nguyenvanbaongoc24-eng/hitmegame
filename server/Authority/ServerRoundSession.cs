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
    private Round? round;
    private int publishedRoundId;

    public ServerRoundSession(GameConfig config, IEnumerable<string> playerIds)
    {
        this.playerIds = playerIds?.ToArray() ?? throw new ArgumentNullException(nameof(playerIds));
        match = new ServerMatch(config, this.playerIds);
    }

    public ServerMatch Match => match;
    public Round ActiveRound => round ?? throw new InvalidOperationException("No active round.");

    public IReadOnlyList<OutboundMessage> BeginRound(TimeProvider? clock = null)
    {
        var active = match.BeginRound(clock);
        round = active;
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

    private IReadOnlyList<OutboundMessage> PublishIfLocked()
    {
        var active = ActiveRound;
        if (!active.IsLocked || publishedRoundId == active.RoundId) return Array.Empty<OutboundMessage>();

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

    private IReadOnlyList<OutboundMessage> Broadcast(object payload)
    {
        var json = JsonSerializer.Serialize(payload, Json);
        return playerIds.Select(playerId => new OutboundMessage(playerId, json)).ToArray();
    }
}
