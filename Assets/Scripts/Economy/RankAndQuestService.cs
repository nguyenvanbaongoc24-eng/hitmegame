using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.Economy
{
    [Serializable]
    public class RankTierDto
    {
        public string nameVi;
        public string nameEn;
        public int minPoints;
        public int maxPoints;
        public string badge;

        public string GetLocalizedName(bool isEnglish) => isEnglish ? nameEn : nameVi;
    }

    [Serializable]
    public class ScoringRulesDto
    {
        public int rank1 = 25;
        public int rank2 = 10;
        public int rank3 = -5;
        public int rank4 = -15;
        public int floor = 0;
        public float multiplier2P = 0.6f;
        public float multiplier4P = 1.0f;
        public float multiplier6P = 1.2f;
    }

    [Serializable]
    public class LeaderboardEntryDto
    {
        public int rank;
        public string name;
        public int points;
        public string tier;
    }

    [Serializable]
    public class RankConfigData
    {
        public List<RankTierDto> tiers = new List<RankTierDto>();
        public ScoringRulesDto scoringRules = new ScoringRulesDto();
        public List<LeaderboardEntryDto> mockLeaderboard = new List<LeaderboardEntryDto>();
    }

    [Serializable]
    public class QuestDto
    {
        public string id;
        public string titleVi;
        public string titleEn;
        public int target;
        public int rewardGold;
        public string descriptionVi;
        public int currentProgress;
        public bool isClaimed;

        public string GetLocalizedTitle(bool isEnglish) => isEnglish ? (string.IsNullOrEmpty(titleEn) ? titleVi : titleEn) : titleVi;
        public bool IsCompleted => currentProgress >= target;
    }

    [Serializable]
    public class QuestsConfigData
    {
        public string timezone = "Asia/Ho_Chi_Minh";
        public int resetHour = 0;
        public List<QuestDto> quests = new List<QuestDto>();
    }

    public static class RankService
    {
        private static RankConfigData _data;

        public static void EnsureInitialized()
        {
            if (_data != null && _data.tiers != null && _data.tiers.Count > 0) return;
            var asset = Resources.Load<TextAsset>("RankConfig");
            if (asset != null && !string.IsNullOrEmpty(asset.text))
            {
                _data = JsonUtility.FromJson<RankConfigData>(asset.text);
            }
            if (_data == null || _data.tiers == null || _data.tiers.Count == 0)
            {
                CreateFallbackConfig();
            }
        }

        private static void CreateFallbackConfig()
        {
            _data = new RankConfigData
            {
                tiers = new List<RankTierDto>
                {
                    new RankTierDto { nameVi = "Đồng", nameEn = "Bronze", minPoints = 0, maxPoints = 99, badge = "🥉" },
                    new RankTierDto { nameVi = "Bạc", nameEn = "Silver", minPoints = 100, maxPoints = 299, badge = "🥈" },
                    new RankTierDto { nameVi = "Vàng", nameEn = "Gold", minPoints = 300, maxPoints = 599, badge = "🥇" },
                    new RankTierDto { nameVi = "Bạch Kim", nameEn = "Platinum", minPoints = 600, maxPoints = 999, badge = "💎" },
                    new RankTierDto { nameVi = "Kim Cương", nameEn = "Diamond", minPoints = 1000, maxPoints = 99999, badge = "👑" }
                },
                scoringRules = new ScoringRulesDto(),
                mockLeaderboard = new List<LeaderboardEntryDto>
                {
                    new LeaderboardEntryDto { rank = 1, name = "Nguyễn_Vô_Địch", points = 1250, tier = "Kim Cương" },
                    new LeaderboardEntryDto { rank = 2, name = "Dép_Bay_Sát_Thủ", points = 980, tier = "Bạch Kim" },
                    new LeaderboardEntryDto { rank = 3, name = "Chảo_Thần_Sầu", points = 820, tier = "Bạch Kim" },
                    new LeaderboardEntryDto { rank = 4, name = "Bia_Hơi_Hà_Nội", points = 540, tier = "Vàng" },
                    new LeaderboardEntryDto { rank = 5, name = "Cub50_Bốc_Đầu", points = 310, tier = "Vàng" }
                }
            };
        }

        public static IReadOnlyList<RankTierDto> GetTiers()
        {
            EnsureInitialized();
            return _data.tiers;
        }

        public static RankTierDto GetTierForPoints(int points)
        {
            EnsureInitialized();
            foreach (var tier in _data.tiers)
            {
                if (points >= tier.minPoints && points <= tier.maxPoints) return tier;
            }
            return _data.tiers.Count > 0 ? _data.tiers[0] : null;
        }

        public static int CalculatePointsDelta(int placement, int totalPlayers)
        {
            EnsureInitialized();
            float baseDelta = 0;
            switch (placement)
            {
                case 1: baseDelta = _data.scoringRules.rank1; break;
                case 2: baseDelta = _data.scoringRules.rank2; break;
                case 3: baseDelta = _data.scoringRules.rank3; break;
                default: baseDelta = _data.scoringRules.rank4; break;
            }

            float multiplier = 1.0f;
            if (totalPlayers <= 2) multiplier = _data.scoringRules.multiplier2P;
            else if (totalPlayers >= 6) multiplier = _data.scoringRules.multiplier6P;
            else multiplier = _data.scoringRules.multiplier4P;

            return Mathf.RoundToInt(baseDelta * multiplier);
        }

        public static IReadOnlyList<LeaderboardEntryDto> GetMockLeaderboard()
        {
            EnsureInitialized();
            return _data.mockLeaderboard;
        }

        public static void ResetForTesting()
        {
            _data = null;
        }
    }

    public static class QuestService
    {
        private const string PrefsLastResetDate = "HitMe_Quest_LastResetDate";
        private const string PrefsQuestProgress = "HitMe_Quest_Progress_";
        private const string PrefsQuestClaimed = "HitMe_Quest_Claimed_";

        private static QuestsConfigData _config;
        private static readonly List<QuestDto> _runtimeQuests = new List<QuestDto>();

        public static void EnsureInitialized()
        {
            if (_runtimeQuests.Count > 0) return;

            var asset = Resources.Load<TextAsset>("QuestsConfig");
            if (asset != null && !string.IsNullOrEmpty(asset.text))
            {
                _config = JsonUtility.FromJson<QuestsConfigData>(asset.text);
            }

            if (_config == null || _config.quests == null || _config.quests.Count == 0)
            {
                CreateFallbackConfig();
            }

            CheckAndResetDaily();
            LoadState();
        }

        private static void CreateFallbackConfig()
        {
            _config = new QuestsConfigData
            {
                timezone = "Asia/Ho_Chi_Minh",
                resetHour = 0,
                quests = new List<QuestDto>
                {
                    new QuestDto { id = "play_3_matches", titleVi = "Chơi 3 trận", titleEn = "Play 3 matches", target = 3, rewardGold = 50, descriptionVi = "Hoàn thành 3 trận đấu bất kỳ." },
                    new QuestDto { id = "land_5_hits", titleVi = "Trúng 5 đòn", titleEn = "Land 5 hits", target = 5, rewardGold = 60, descriptionVi = "Ném trúng đối thủ 5 lần." },
                    new QuestDto { id = "win_1_match", titleVi = "Thắng 1 trận", titleEn = "Win 1 match", target = 1, rewardGold = 100, descriptionVi = "Chiến thắng 1 trận đấu." }
                }
            };
        }

        private static string GetTodayVnDateString()
        {
            // Vietnam Time = UTC + 7
            var vnTime = DateTime.UtcNow.AddHours(7);
            return vnTime.ToString("yyyy-MM-dd");
        }

        public static void CheckAndResetDaily()
        {
            string today = GetTodayVnDateString();
            string lastReset = PlayerPrefs.GetString(PrefsLastResetDate, string.Empty);
            if (lastReset != today)
            {
                // Reset daily quests
                foreach (var q in _config.quests)
                {
                    PlayerPrefs.DeleteKey(PrefsQuestProgress + q.id);
                    PlayerPrefs.DeleteKey(PrefsQuestClaimed + q.id);
                }
                PlayerPrefs.SetString(PrefsLastResetDate, today);
                PlayerPrefs.Save();
            }
        }

        private static void LoadState()
        {
            _runtimeQuests.Clear();
            foreach (var q in _config.quests)
            {
                var copy = new QuestDto
                {
                    id = q.id,
                    titleVi = q.titleVi,
                    titleEn = q.titleEn,
                    target = q.target,
                    rewardGold = q.rewardGold,
                    descriptionVi = q.descriptionVi,
                    currentProgress = PlayerPrefs.GetInt(PrefsQuestProgress + q.id, 0),
                    isClaimed = PlayerPrefs.GetInt(PrefsQuestClaimed + q.id, 0) == 1
                };
                _runtimeQuests.Add(copy);
            }
        }

        public static IReadOnlyList<QuestDto> GetQuests()
        {
            EnsureInitialized();
            return _runtimeQuests;
        }

        public static void RecordMatchPlayed(bool isWin, int hitsLanded)
        {
            EnsureInitialized();
            foreach (var q in _runtimeQuests)
            {
                if (q.id == "play_3_matches")
                {
                    q.currentProgress = Mathf.Min(q.target, q.currentProgress + 1);
                    PlayerPrefs.SetInt(PrefsQuestProgress + q.id, q.currentProgress);
                }
                else if (q.id == "land_5_hits")
                {
                    q.currentProgress = Mathf.Min(q.target, q.currentProgress + hitsLanded);
                    PlayerPrefs.SetInt(PrefsQuestProgress + q.id, q.currentProgress);
                }
                else if (q.id == "win_1_match" && isWin)
                {
                    q.currentProgress = Mathf.Min(q.target, q.currentProgress + 1);
                    PlayerPrefs.SetInt(PrefsQuestProgress + q.id, q.currentProgress);
                }
            }
            PlayerPrefs.Save();
        }

        public static bool ClaimReward(string questId, out int goldReward)
        {
            goldReward = 0;
            EnsureInitialized();
            var q = _runtimeQuests.Find(it => it.id == questId);
            if (q == null || !q.IsCompleted || q.isClaimed) return false;

            q.isClaimed = true;
            goldReward = q.rewardGold;
            PlayerPrefs.SetInt(PrefsQuestClaimed + q.id, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static void ResetForTesting()
        {
            _config = null;
            _runtimeQuests.Clear();
            PlayerPrefs.DeleteKey(PrefsLastResetDate);
            PlayerPrefs.DeleteKey(PrefsQuestProgress + "play_3_matches");
            PlayerPrefs.DeleteKey(PrefsQuestClaimed + "play_3_matches");
            PlayerPrefs.DeleteKey(PrefsQuestProgress + "land_5_hits");
            PlayerPrefs.DeleteKey(PrefsQuestClaimed + "land_5_hits");
            PlayerPrefs.DeleteKey(PrefsQuestProgress + "win_1_match");
            PlayerPrefs.DeleteKey(PrefsQuestClaimed + "win_1_match");
            PlayerPrefs.Save();
        }
    }
}
