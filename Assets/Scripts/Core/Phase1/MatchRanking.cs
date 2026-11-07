using System;
using System.Collections.Generic;

namespace HitMe.Core
{
    public sealed class RankedPlayer
    {
        public string PlayerId;
        public int Rank;
        public int Hp;
        public int Hits;
        public int Throws;
        public float Accuracy;
        public int EliminatedRound;
        public bool IsWinner;
    }

    public static class MatchRanking
    {
        public static RankedPlayer[] Rank(IReadOnlyList<PlayerState> players, IReadOnlyList<string> winnerIds)
        {
            var ordered = new List<PlayerState>(players);
            ordered.Sort((a, b) =>
            {
                bool aWinner = Contains(winnerIds, a.Id), bWinner = Contains(winnerIds, b.Id);
                if (aWinner != bWinner) return aWinner ? -1 : 1;
                if (a.IsAlive != b.IsAlive) return a.IsAlive ? -1 : 1;
                if (a.IsAlive)
                {
                    int hp = b.Hp.CompareTo(a.Hp);
                    if (hp != 0) return hp;
                    int hits = b.HitsLanded.CompareTo(a.HitsLanded);
                    if (hits != 0) return hits;
                }
                else
                {
                    int round = b.EliminatedAtRound.CompareTo(a.EliminatedAtRound);
                    if (round != 0) return round;
                    // A player still alive at the hard cap ranks above one killed in that round.
                    int hp = b.HardCapHp.CompareTo(a.HardCapHp);
                    if (hp != 0) return hp;
                    if (a.HardCapHp > 0)
                    {
                        int hits = b.HitsLanded.CompareTo(a.HitsLanded);
                        if (hits != 0) return hits;
                    }
                }
                return string.Compare(a.Id, b.Id, StringComparison.Ordinal);
            });
            var result = new RankedPlayer[ordered.Count];
            for (int i = 0; i < ordered.Count; i++)
            {
                var p = ordered[i];
                int rank = i + 1;
                if (i > 0 && SameRank(ordered[i - 1], p, winnerIds)) rank = result[i - 1].Rank;
                result[i] = new RankedPlayer
                {
                    PlayerId = p.Id, Rank = rank, Hp = p.Hp, Hits = p.HitsLanded,
                    Throws = p.Throws, Accuracy = p.Throws == 0 ? 0f : (float)p.HitsLanded / p.Throws,
                    EliminatedRound = p.EliminatedAtRound, IsWinner = Contains(winnerIds, p.Id)
                };
            }
            return result;
        }

        private static bool SameRank(PlayerState a, PlayerState b, IReadOnlyList<string> winners)
        {
            bool aw = Contains(winners, a.Id), bw = Contains(winners, b.Id);
            if (aw || bw) return aw && bw;
            if (a.IsAlive != b.IsAlive) return false;
            if (a.IsAlive) return a.Hp == b.Hp && a.HitsLanded == b.HitsLanded;
            if (a.EliminatedAtRound != b.EliminatedAtRound || a.HardCapHp != b.HardCapHp) return false;
            return a.HardCapHp == 0 || a.HitsLanded == b.HitsLanded;
        }

        private static bool Contains(IReadOnlyList<string> values, string id)
        {
            for (int i = 0; i < values.Count; i++) if (values[i] == id) return true;
            return false;
        }
    }
}
