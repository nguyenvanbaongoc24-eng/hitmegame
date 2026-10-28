using System.Collections.Generic;

namespace HitMe.Core
{
    public static class HitCalculator
    {
        public static Vec2 GetWallPoint(Vec2 origin, Vec2 direction, GameConfig config)
        {
            var dir = direction.Normalized;
            float tx = dir.X > 0.00001f ? (config.MaxX - origin.X) / dir.X : (dir.X < -0.00001f ? (config.MinX - origin.X) / dir.X : float.MaxValue);
            float ty = dir.Y > 0.00001f ? (config.MaxY - origin.Y) / dir.Y : (dir.Y < -0.00001f ? (config.MinY - origin.Y) / dir.Y : float.MaxValue);
            float distance = tx < ty ? tx : ty;
            return origin + dir * distance;
        }

        public static List<HitEvent> CalculateHits(PlayerState shooter, IList<PlayerState> snapshot, Vec2 direction, GameConfig config)
        {
            var allHits = new List<HitEvent>();
            if (shooter == null || !shooter.IsAlive) return allHits;
            var origin = shooter.Position;
            var end = GetWallPoint(origin, direction, config);
            var segment = end - origin;
            var length = segment.Length;
            var unit = segment / length;

            for (int i = 0; i < snapshot.Count; i++)
            {
                var target = snapshot[i];
                if (target == null || !target.IsAlive || target.Id == shooter.Id) continue;
                float projected = Vec2.Dot(target.Position - origin, unit);
                if (projected < 0f || projected > length) continue;
                var closest = origin + unit * projected;
                if (Vec2.Distance(target.Position, closest) <= config.PlayerRadius)
                    allHits.Add(new HitEvent { ShooterId = shooter.Id, TargetId = target.Id, Point = closest, Distance = projected });
            }
            allHits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            if (allHits.Count > config.MaxHitsPerThrow)
                allHits.RemoveRange(config.MaxHitsPerThrow, allHits.Count - config.MaxHitsPerThrow);
            return allHits;
        }
    }
}
