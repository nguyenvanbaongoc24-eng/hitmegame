using System.Collections.Generic;

namespace HitMe.Core
{
    public static class RoundResolver
    {
        public static RoundResolution Resolve(IList<PlayerState> players, IDictionary<string, RoundInput> inputs, GameConfig config)
        {
            var snapshot = new List<PlayerState>();
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                snapshot.Add(new PlayerState(p.Id, p.Name, p.IsBot, p.Hp, p.Position, p.Facing));
            }

            var result = new RoundResolution();
            for (int i = 0; i < snapshot.Count; i++)
            {
                var shooter = snapshot[i];
                RoundInput input;
                if (!shooter.IsAlive || !inputs.TryGetValue(shooter.Id, out input) || !input.HasAim) continue;
                var hits = HitCalculator.CalculateHits(shooter, snapshot, input.AimDirection, config);
                for (int h = 0; h < hits.Count; h++)
                {
                    var hit = hits[h];
                    result.Hits.Add(hit);
                    int damage;
                    result.Damage.TryGetValue(hit.TargetId, out damage);
                    result.Damage[hit.TargetId] = damage + config.DamagePerHit;
                }
            }

            for (int i = 0; i < players.Count; i++)
            {
                int damage;
                if (result.Damage.TryGetValue(players[i].Id, out damage))
                    players[i].Hp = players[i].Hp - damage < 0 ? 0 : players[i].Hp - damage;
            }
            return result;
        }
    }
}
