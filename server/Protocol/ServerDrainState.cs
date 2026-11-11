namespace HitMe.Server.Protocol;

/// <summary>Safe liveness state exposed by health checks during graceful shutdown.</summary>
public sealed class ServerDrainState
{
    private int draining;
    public bool IsDraining => Volatile.Read(ref draining) != 0;
    public void BeginDraining() => Interlocked.Exchange(ref draining, 1);
}
