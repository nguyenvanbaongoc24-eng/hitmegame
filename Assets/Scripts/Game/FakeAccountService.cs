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
        public bool CosmeticsEnabled { get; set; } = false; // Default off per rule v1.2

        public const int RenameCostGold = 100;

        public event Action OnAccountUpdated;

        public FakeAccountService()
        {
            _instance = this;
        }

        public bool Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return false;
            newName = newName.Trim();
            if (newName.Length < 2 || newName.Length > 16) return false;

            if (NameChangeCount > 0)
            {
                if (Gold < RenameCostGold) return false;
                Gold -= RenameCostGold;
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
            CosmeticsEnabled = false;
            OnAccountUpdated?.Invoke();
        }
    }
}
