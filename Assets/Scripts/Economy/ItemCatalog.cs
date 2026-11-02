using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.Economy
{
    [Serializable]
    public class ItemDto
    {
        public string id;
        public string nameVi;
        public string nameEn;
        public string category;
        public int price;
        public string rarity;
        public bool isStarter;
        public string iconSprite;
        public string projectileSprite;
        public string hitVfx;
        public string hitSfx;
        public string descriptionVi;
        public string descriptionEn;

        public string GetLocalizedName(bool isEnglish) => isEnglish ? (string.IsNullOrEmpty(nameEn) ? nameVi : nameEn) : nameVi;
        public string GetLocalizedDesc(bool isEnglish) => isEnglish ? (string.IsNullOrEmpty(descriptionEn) ? descriptionVi : descriptionEn) : descriptionVi;
    }

    [Serializable]
    public class ItemCatalogData
    {
        public int version;
        public string currency;
        public List<ItemDto> items;
    }

    public static class ItemCatalog
    {
        private static ItemCatalogData _data;
        private static readonly HashSet<string> _ownedItems = new HashSet<string> { "dep_to_ong" };
        private static string _equippedWeaponId = "dep_to_ong";

        public static void Initialize(string jsonContent)
        {
            if (!string.IsNullOrEmpty(jsonContent))
            {
                _data = JsonUtility.FromJson<ItemCatalogData>(jsonContent);
            }
            _ownedItems.Add("dep_to_ong");
            LoadFromPrefs();
        }

        public static void EnsureInitialized()
        {
            if (_data != null && _data.items != null && _data.items.Count > 0) return;
            var asset = Resources.Load<TextAsset>("Items");
            if (asset != null && !string.IsNullOrEmpty(asset.text))
            {
                Initialize(asset.text);
            }
        }

        public static IReadOnlyList<ItemDto> GetItems()
        {
            EnsureInitialized();
            if (_data == null || _data.items == null) return Array.Empty<ItemDto>();
            return _data.items;
        }

        public static ItemDto GetItem(string id)
        {
            EnsureInitialized();
            if (_data == null || _data.items == null) return null;
            return _data.items.Find(it => it.id == id);
        }

        public static bool IsOwned(string id)
        {
            EnsureInitialized();
            return _ownedItems.Contains(id);
        }

        public static string EquippedWeaponId => _equippedWeaponId;

        public static bool BuyItem(string id, ref int currentGold, out string error)
        {
            error = string.Empty;
            var item = GetItem(id);
            if (item == null)
            {
                error = "item_not_found";
                return false;
            }
            if (_ownedItems.Contains(id))
            {
                error = "already_owned";
                return false;
            }
            if (currentGold < item.price)
            {
                error = "not_enough_gold";
                return false;
            }

            currentGold -= item.price;
            _ownedItems.Add(id);
            SaveToPrefs();
            return true;
        }

        public static bool EquipItem(string id)
        {
            if (!_ownedItems.Contains(id)) return false;
            _equippedWeaponId = id;
            PlayerPrefs.SetString("HitMe_EquippedWeapon", id);
            PlayerPrefs.Save();
            return true;
        }

        public static void ResetForTesting()
        {
            PlayerPrefs.DeleteKey("HitMe_OwnedItems");
            PlayerPrefs.DeleteKey("HitMe_EquippedWeapon");
            PlayerPrefs.Save();
            _ownedItems.Clear();
            _ownedItems.Add("dep_to_ong");
            _equippedWeaponId = "dep_to_ong";
        }

        private static void SaveToPrefs()
        {
            PlayerPrefs.SetString("HitMe_OwnedItems", string.Join(",", _ownedItems));
            PlayerPrefs.SetString("HitMe_EquippedWeapon", _equippedWeaponId);
            PlayerPrefs.Save();
        }

        private static void LoadFromPrefs()
        {
            string ownedStr = PlayerPrefs.GetString("HitMe_OwnedItems", "dep_to_ong");
            foreach (var id in ownedStr.Split(','))
            {
                if (!string.IsNullOrEmpty(id)) _ownedItems.Add(id);
            }
            _equippedWeaponId = PlayerPrefs.GetString("HitMe_EquippedWeapon", "dep_to_ong");
        }
    }
}
