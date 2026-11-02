using System;
using System.Collections.Generic;

namespace HitMe.Game
{
    public interface IAccountService
    {
        string PlayerId { get; }
        string DisplayName { get; }
        int Gold { get; }
        int Gems { get; }
        int NameChangeCount { get; }
        bool CosmeticsEnabled { get; set; }

        bool Rename(string newName);
        bool AddGold(int amount);
        bool DeductGold(int amount);
    }

    /// <summary>
    /// Pure in-memory FakeAccountService for UI/UX production demonstration.
    /// Does not call Supabase or any network backend.
    /// </summary>
    public class FakeAccountService : IAccountService
    {
        private static FakeAccountService _instance;
        public static FakeAccountService Instance => _instance ?? (_instance = new FakeAccountService());

        public string PlayerId { get; private set; } = "HM-9582";
        public string DisplayName { get; private set; } = "Khách_4826";
        public int Gold { get; private set; } = 500;
        public int Gems { get; private set; } = 20;
        public int NameChangeCount { get; private set; } = 0;
        public int MatchesPlayed { get; set; } = 0;
        public int RankPoints { get; set; } = 150;
        public string AvatarId { get; set; } = "avatar_default";
        public bool CosmeticsEnabled { get; set; } = false; // Default off per rule v1.2

        public int RenameCostGold { get; set; } = 100;

        public event Action OnAccountUpdated;

        public FakeAccountService()
        {
            _instance = this;
        }

        public bool Rename(string newName) => Rename(newName, -1);

        public bool Rename(string newName, int customCost)
        {
            if (string.IsNullOrWhiteSpace(newName)) return false;
            newName = newName.Trim();
            if (newName.Length < 2 || newName.Length > 16) return false;

            int effectiveCost = customCost >= 0 ? customCost : RenameCostGold;
            if (NameChangeCount > 0)
            {
                if (Gold < effectiveCost) return false;
                Gold -= effectiveCost;
            }

            DisplayName = newName;
            NameChangeCount++;
            OnAccountUpdated?.Invoke();
            return true;
        }

        public bool AddGold(int amount)
        {
            if (amount <= 0) return false;
            Gold += amount;
            OnAccountUpdated?.Invoke();
            return true;
        }

        public bool DeductGold(int amount)
        {
            if (amount <= 0 || Gold < amount) return false;
            Gold -= amount;
            OnAccountUpdated?.Invoke();
            return true;
        }

        public void ResetForTest(string name = "Khách_4826", int gold = 500)
        {
            DisplayName = name;
            Gold = gold;
            Gems = 20;
            NameChangeCount = 0;
            MatchesPlayed = 0;
            RankPoints = 150;
            AvatarId = "avatar_default";
            RenameCostGold = 100;
            CosmeticsEnabled = false;
            OnAccountUpdated?.Invoke();
        }
    }
}
