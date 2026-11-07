#nullable disable
using System;
using System.Collections.Generic;

namespace HitMe.Core
{
    public sealed class RankedPlayer
    {
        public PlayerState Player { get; private set; }
        public string Id { get { return Player.Id; } }
        public string Name { get { return Player.Name; } }
        public bool IsAlive { get { return Player.IsAlive; } }
        public int Hp { get { return Player.Hp; } }
        public int HitsLanded { get { return Player.HitsLanded; } }
        public int Throws { get { return Player.Throws; } }
        public float Accuracy { get { return Player.Throws == 0 ? 0f : (float)Player.HitsLanded / Player.Throws; } }
        public int EliminatedAtRound { get { return Player.EliminatedAtRound; } }
        public int Rank { get; internal set; }
        public int DisplayIndex { get; internal set; }
        public bool IsSharedRank { get; internal set; }
        public bool IsWinner { get; internal set; }

        public RankedPlayer(PlayerState player, int rank, int displayIndex, bool isSharedRank, bool isWinner)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Rank = rank;
            DisplayIndex = displayIndex;
            IsSharedRank = isSharedRank;
            IsWinner = isWinner;
        }
    }

    public sealed class MatchRankingResult
    {
        public RankedPlayer[] RankedPlayers { get; private set; }
        public string[] WinnerIds { get; private set; }
        public bool IsSharedWin { get { return WinnerIds.Length > 1; } }

        public MatchRankingResult(RankedPlayer[] rankedPlayers, string[] winnerIds)
        {
            RankedPlayers = rankedPlayers ?? Array.Empty<RankedPlayer>();
            WinnerIds = winnerIds ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Pure C# authoritative ranking and tie-break evaluator shared by Server and Unity Client.
    /// Rules:
    /// 1. Alive players rank above eliminated players, ordered by Hp descending, then HitsLanded descending.
    /// 2. Equal (IsAlive, Hp, HitsLanded) for alive players => shared rank (co-winners if Rank == 1).
    /// 3. Eliminated players rank by EliminatedAtRound descending (eliminated later => higher rank),
    ///    then HitsLanded descending, then shared rank.
    /// 4. Local player ("player") / Name ordering is strictly for display order among tied players
    ///    and never affects competitive Rank or WinnerIds.
    /// </summary>
    public static class MatchRanking
    {
        public static readonly Comparison<PlayerState> DisplayOrderComparison = CompareDisplayOrder;

        // v1.3 ranking is selected only for the new uncapped rule set. The
        // legacy ranking API remains available for archived v1.2 vectors.
        public static RankedPlayer[] RankV13(IReadOnlyList<PlayerState> players, IReadOnlyList<string> winners)
        {
            var sorted = new List<PlayerState>(players);
            sorted.Sort((a, b) => CompareV13(a, b, winners));
            var ranked = new RankedPlayer[sorted.Count];
            for (int i = 0; i < sorted.Count; i++)
            {
                int rank = i + 1;
                if (i > 0 && SameV13Rank(sorted[i - 1], sorted[i], winners)) rank = ranked[i - 1].Rank;
                bool shared = (i > 0 && SameV13Rank(sorted[i - 1], sorted[i], winners))
                    || (i + 1 < sorted.Count && SameV13Rank(sorted[i], sorted[i + 1], winners));
                ranked[i] = new RankedPlayer(sorted[i], rank, i, shared, ContainsWinner(winners, sorted[i].Id));
            }
            return ranked;
        }

        private static bool ContainsWinner(IReadOnlyList<string> winners, string id)
        {
            for (int i = 0; i < winners.Count; i++) if (winners[i] == id) return true;
            return false;
        }

        private static int CompareV13(PlayerState a, PlayerState b, IReadOnlyList<string> winners)
        {
            bool aw = ContainsWinner(winners, a.Id), bw = ContainsWinner(winners, b.Id);
            if (aw != bw) return aw ? -1 : 1;
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
                int hardCapHp = b.HardCapHp.CompareTo(a.HardCapHp);
                if (hardCapHp != 0) return hardCapHp;
                if (a.HardCapHp > 0)
                {
                    int hits = b.HitsLanded.CompareTo(a.HitsLanded);
                    if (hits != 0) return hits;
                }
            }
            return string.Compare(a.Id, b.Id, StringComparison.Ordinal);
        }

        private static bool SameV13Rank(PlayerState a, PlayerState b, IReadOnlyList<string> winners)
        {
            bool aw = ContainsWinner(winners, a.Id), bw = ContainsWinner(winners, b.Id);
            if (aw || bw) return aw && bw;
            if (a.IsAlive != b.IsAlive) return false;
            if (a.IsAlive) return a.Hp == b.Hp && a.HitsLanded == b.HitsLanded;
            if (a.EliminatedAtRound != b.EliminatedAtRound || a.HardCapHp != b.HardCapHp) return false;
            return a.HardCapHp == 0 || a.HitsLanded == b.HitsLanded;
        }

        public static int CompareCompetitive(PlayerState a, PlayerState b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            if (a.IsAlive != b.IsAlive)
            {
                return b.IsAlive.CompareTo(a.IsAlive);
            }

            if (a.IsAlive)
            {
                if (a.Hp != b.Hp) return b.Hp.CompareTo(a.Hp);
                if (a.HitsLanded != b.HitsLanded) return b.HitsLanded.CompareTo(a.HitsLanded);
                return 0;
            }

            if (a.EliminatedAtRound != b.EliminatedAtRound)
            {
                return b.EliminatedAtRound.CompareTo(a.EliminatedAtRound);
            }

            if (a.HitsLanded != b.HitsLanded)
            {
                return b.HitsLanded.CompareTo(a.HitsLanded);
            }

            return 0;
        }

        public static int CompareDisplayOrder(PlayerState a, PlayerState b)
        {
            int competitive = CompareCompetitive(a, b);
            if (competitive != 0) return competitive;
            if (a == null || b == null) return 0;

            bool aIsLocal = string.Equals(a.Id, "player", StringComparison.Ordinal);
            bool bIsLocal = string.Equals(b.Id, "player", StringComparison.Ordinal);
            if (aIsLocal != bIsLocal) return aIsLocal ? -1 : 1;

            int nameCmp = string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            if (nameCmp != 0) return nameCmp;

            return string.Compare(a.Id, b.Id, StringComparison.Ordinal);
        }

        public static RankedPlayer[] Rank(IReadOnlyList<PlayerState> players, RoundLimitTiebreakPolicy tiebreakPolicy = RoundLimitTiebreakPolicy.HighestHp)
        {
            return Evaluate(players, tiebreakPolicy).RankedPlayers;
        }

        public static string[] Winners(IReadOnlyList<PlayerState> players, RoundLimitTiebreakPolicy tiebreakPolicy = RoundLimitTiebreakPolicy.HighestHp)
        {
            return Evaluate(players, tiebreakPolicy).WinnerIds;
        }

        public static MatchRankingResult Evaluate(IReadOnlyList<PlayerState> players, RoundLimitTiebreakPolicy tiebreakPolicy = RoundLimitTiebreakPolicy.HighestHp)
        {
            if (players == null || players.Count == 0)
            {
                return new MatchRankingResult(Array.Empty<RankedPlayer>(), Array.Empty<string>());
            }

            var sorted = new List<PlayerState>(players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null) sorted.Add(players[i]);
            }

            sorted.Sort(DisplayOrderComparison);

            int count = sorted.Count;
            var ranks = new int[count];
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    ranks[i] = 1;
                }
                else if (CompareCompetitive(sorted[i - 1], sorted[i]) == 0)
                {
                    ranks[i] = ranks[i - 1];
                }
                else
                {
                    ranks[i] = i + 1;
                }
            }

            bool anyAlive = count > 0 && sorted[0].IsAlive;
            var winnerIdList = new List<string>();
            for (int i = 0; i < count; i++)
            {
                bool isWinner;
                if (anyAlive && tiebreakPolicy == RoundLimitTiebreakPolicy.SharedWin)
                {
                    isWinner = sorted[i].IsAlive;
                }
                else
                {
                    isWinner = ranks[i] == 1;
                }

                if (isWinner)
                {
                    winnerIdList.Add(sorted[i].Id);
                }
            }

            var resultPlayers = new RankedPlayer[count];
            for (int i = 0; i < count; i++)
            {
                int myRank = ranks[i];
                bool shared = (i > 0 && ranks[i - 1] == myRank) || (i + 1 < count && ranks[i + 1] == myRank);
                bool isWinner = (anyAlive && tiebreakPolicy == RoundLimitTiebreakPolicy.SharedWin)
                    ? sorted[i].IsAlive
                    : (myRank == 1);
                resultPlayers[i] = new RankedPlayer(sorted[i], myRank, i, shared, isWinner);
            }

            return new MatchRankingResult(resultPlayers, winnerIdList.ToArray());
        }

        public static void FillWinners(IReadOnlyList<PlayerState> players, RoundLimitTiebreakPolicy tiebreakPolicy, List<string> outWinnerIds)
        {
            if (outWinnerIds == null) return;
            outWinnerIds.Clear();
            if (players == null || players.Count == 0) return;

            bool anyAlive = false;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].IsAlive)
                {
                    anyAlive = true;
                    break;
                }
            }

            if (anyAlive && tiebreakPolicy == RoundLimitTiebreakPolicy.SharedWin)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i] != null && players[i].IsAlive)
                    {
                        outWinnerIds.Add(players[i].Id);
                    }
                }
                return;
            }

            PlayerState best = null;
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p == null) continue;
                if (best == null || CompareCompetitive(p, best) < 0)
                {
                    best = p;
                }
            }

            if (best == null) return;

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p != null && CompareCompetitive(p, best) == 0)
                {
                    outWinnerIds.Add(p.Id);
                }
            }
        }

        public static void BuildRankingsAndWinners(MatchState state, List<PlayerState> outRanked, List<string> outWinnerIds)
        {
            if (outRanked != null) outRanked.Clear();
            if (outWinnerIds != null) outWinnerIds.Clear();
            if (state == null) return;

            if (outRanked != null)
            {
                for (int i = 0; i < state.Players.Count; i++)
                {
                    outRanked.Add(state.Players[i]);
                }
                if (state.Config.MaxRounds == 0)
                {
                    var v13 = RankV13(state.Players, state.WinnerIds);
                    outRanked.Clear();
                    for (int i = 0; i < v13.Length; i++) outRanked.Add(v13[i].Player);
                }
                else outRanked.Sort(DisplayOrderComparison);
            }

            if (outWinnerIds != null)
            {
                if (state.WinnerIds != null && state.WinnerIds.Count > 0)
                {
                    for (int i = 0; i < state.WinnerIds.Count; i++)
                    {
                        outWinnerIds.Add(state.WinnerIds[i]);
                    }
                }
                else if (state.Config.MaxRounds != 0)
                {
                    FillWinners(state.Players, state.Config.RoundLimitTiebreak, outWinnerIds);
                }
            }
        }
    }
}
