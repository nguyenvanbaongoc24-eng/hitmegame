using System.Collections.Generic;

namespace HitMe.Core
{
    public static class RoundResolver
    {
        private static readonly List<HitEvent> SharedHitsBuffer = new List<HitEvent>(8);

        public static RoundResolution Resolve(IList<PlayerState> players, IDictionary<string, RoundInput> inputs, GameConfig config)
        {
            var result = new RoundResolution();
            for (int i = 0; i < players.Count; i++)
            {
                var shooter = players[i];
                RoundInput input;
                if (!shooter.IsAlive || !inputs.TryGetValue(shooter.Id, out input) || !input.HasAim) continue;
                shooter.Throws++;
                HitCalculator.CalculateHits(shooter, players, input.AimDirection, config, SharedHitsBuffer);
                for (int h = 0; h < SharedHitsBuffer.Count; h++)
                {
                    var hit = SharedHitsBuffer[h];
                    result.Hits.Add(hit);
                    int damage;
                    result.Damage.TryGetValue(hit.TargetId, out damage);
                    result.Damage[hit.TargetId] = damage + config.DamagePerHit;
                }
                if (SharedHitsBuffer.Count > 0)
                {
                    shooter.HitsLanded += SharedHitsBuffer.Count;
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
