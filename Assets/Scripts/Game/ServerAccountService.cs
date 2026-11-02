using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.Game
{
    [Serializable]
    public class ServerProfileDto
    {
        public string playerId;
        public string name;
        public int gold;
        public int matchesPlayed;
        public int bestRank;
        public string rank;
        public int points;
        public string[] purchasedItemIds;
        public string[] ownedItemIds;
        public string equippedWeaponId;
        public string equippedCharacterId;
        public string inventoryStatus;
    }

    [Serializable]
    public class ServerBuyItemResultDto
    {
        public bool success;
        public string itemId;
        public int newGold;
        public string error;
    }

    /// <summary>
    /// ServerAccountService — Implements IAccountService backed by Server Protocol v2 profile_state.
    /// Strictly authoritative: does not deduct gold or mutate inventory locally.
    /// Awaits server confirmation via WebSocket protocol v2 messages.
    /// </summary>
    public class ServerAccountService : IAccountService
    {
        private static ServerAccountService _instance;
        public static ServerAccountService Instance => _instance ?? (_instance = new ServerAccountService());

        public string PlayerId { get; private set; } = "";
        public string DisplayName { get; private set; } = "";
        public int Gold { get; private set; } = 0;
        public int Gems { get; private set; } = 0;
        public int NameChangeCount { get; private set; } = 0;
        public int MatchesPlayed { get; private set; } = 0;
        public int RankPoints { get; private set; } = 0;
        public string AvatarId { get; private set; } = "avatar_default";
        public bool CosmeticsEnabled { get; set; } = false;

        public bool IsDataLoaded { get; private set; } = false;

        public List<string> OwnedItemIds { get; private set; } = new List<string>();
        public string EquippedWeaponId { get; private set; } = "dep_to_ong";

        public event Action OnAccountUpdated;

        public ServerAccountService()
        {
            _instance = this;
        }

        public void ResetForTest()
        {
            PlayerId = "";
            DisplayName = "";
            Gold = 0;
            Gems = 0;
            NameChangeCount = 0;
            MatchesPlayed = 0;
            RankPoints = 0;
            IsDataLoaded = false;
            OwnedItemIds.Clear();
            EquippedWeaponId = "dep_to_ong";
            OnAccountUpdated?.Invoke();
        }

        public bool ApplyProfileState(string jsonPayload)
        {
            if (string.IsNullOrEmpty(jsonPayload)) return false;
            try
            {
                var dto = JsonUtility.FromJson<ServerProfileDto>(jsonPayload);
                if (dto == null) return false;

                PlayerId = dto.playerId ?? "";
                DisplayName = dto.name ?? "";
                Gold = dto.gold;
                MatchesPlayed = dto.matchesPlayed;
                RankPoints = dto.points;
                EquippedWeaponId = dto.equippedWeaponId ?? "dep_to_ong";

                OwnedItemIds.Clear();
                if (dto.ownedItemIds != null)
                {
                    OwnedItemIds.AddRange(dto.ownedItemIds);
                }

                IsDataLoaded = true;
                OnAccountUpdated?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ServerAccountService] ApplyProfileState error: {ex.Message}");
                return false;
            }
        }

        public bool ApplyBuyResult(string jsonPayload)
        {
            if (string.IsNullOrEmpty(jsonPayload)) return false;
            try
            {
                var res = JsonUtility.FromJson<ServerBuyItemResultDto>(jsonPayload);
                if (res != null && res.success)
                {
                    Gold = res.newGold;
                    if (!string.IsNullOrEmpty(res.itemId) && !OwnedItemIds.Contains(res.itemId))
                    {
                        OwnedItemIds.Add(res.itemId);
                    }
                    OnAccountUpdated?.Invoke();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Authoritative constraints: local modifications are disallowed without server ack
        public bool Rename(string newName)
        {
            // Must be requested via server message in online mode
            return false;
        }

        public bool AddGold(int amount)
        {
            // Server authority only
            return false;
        }

        public bool DeductGold(int amount)
        {
            // Server authority only
            return false;
        }
    }
}
