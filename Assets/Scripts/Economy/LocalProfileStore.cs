using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.Economy
{
    [Serializable]
    public sealed class LocalProfileData
    {
        public int schemaVersion = 1;
        public string lastQuestResetLocalDate = "";
        public int botMatchesPlayed = 0;
        public int botMatchesWon = 0;
        public int totalHitsLanded = 0;
        public int maxRoundSurvived = 0;
        public int localGold = 500;
        public int sampleRankPoints = 320;
    }

    /// <summary>
    /// Round 38 U2: Safe local storage wrapper around PlayerPrefs (with in-memory fallback
    /// so iOS Safari Private Mode / restricted WebGL storage never throws).
    /// Includes schemaVersion = 1 migration support and QuestRewardsEnabled = false default.
    /// </summary>
    public static class LocalProfileStore
    {
        public const int CurrentSchemaVersion = 1;
        public const string PrefKeySchema = "HitMe_Profile_SchemaVersion";
        public const string PrefKeyProfileJson = "HitMe_Profile_Data_v1";

        /// <summary>
        /// Default false per Round 38 U2: shows experimental claim badge without mutating permanent gold
        /// unless enabled by project owner.
        /// </summary>
        public static bool QuestRewardsEnabled { get; set; } = false;

        private static readonly Dictionary<string, string> _memoryFallback = new Dictionary<string, string>();
        private static LocalProfileData _cached;

        public static LocalProfileData Load()
        {
            if (_cached != null) return _cached;

            int savedSchema = GetInt(PrefKeySchema, 0);
            string raw = GetString(PrefKeyProfileJson, string.Empty);

            if (!string.IsNullOrEmpty(raw))
            {
                try
                {
                    _cached = JsonUtility.FromJson<LocalProfileData>(raw);
                }
                catch
                {
                    _cached = null;
                }
            }

            if (_cached == null)
            {
                _cached = new LocalProfileData { schemaVersion = CurrentSchemaVersion };
            }

            if (savedSchema < CurrentSchemaVersion || _cached.schemaVersion < CurrentSchemaVersion)
            {
                Migrate(_cached, savedSchema);
                Save();
            }

            return _cached;
        }

        private static void Migrate(LocalProfileData data, int fromVersion)
        {
            // Schema v0 -> v1 migration preserves any existing counters safely
            data.schemaVersion = CurrentSchemaVersion;
            if (data.localGold <= 0) data.localGold = 500;
            if (data.sampleRankPoints <= 0) data.sampleRankPoints = 320;
        }

        public static void Save()
        {
            if (_cached == null) return;
            _cached.schemaVersion = CurrentSchemaVersion;
            SetInt(PrefKeySchema, CurrentSchemaVersion);
            try
            {
                SetString(PrefKeyProfileJson, JsonUtility.ToJson(_cached));
                PlayerPrefs.Save();
            }
            catch
            {
                // Safari Private Mode / WebGL QuotaExceededError safely ignored
            }
        }

        public static string GetString(string key, string defaultValue = "")
        {
            try
            {
                if (PlayerPrefs.HasKey(key)) return PlayerPrefs.GetString(key, defaultValue);
            }
            catch { }
            return _memoryFallback.TryGetValue(key, out var val) ? val : defaultValue;
        }

        public static void SetString(string key, string value)
        {
            _memoryFallback[key] = value ?? string.Empty;
            try
            {
                PlayerPrefs.SetString(key, value ?? string.Empty);
            }
            catch { }
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            try
            {
                if (PlayerPrefs.HasKey(key)) return PlayerPrefs.GetInt(key, defaultValue);
            }
            catch { }
            if (_memoryFallback.TryGetValue(key, out var s) && int.TryParse(s, out var parsed))
            {
                return parsed;
            }
            return defaultValue;
        }

        public static void SetInt(string key, int value)
        {
            _memoryFallback[key] = value.ToString();
            try
            {
                PlayerPrefs.SetInt(key, value);
            }
            catch { }
        }

        public static void DeleteKey(string key)
        {
            _memoryFallback.Remove(key);
            try
            {
                PlayerPrefs.DeleteKey(key);
            }
            catch { }
        }
    }
}
