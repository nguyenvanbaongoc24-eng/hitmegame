using System;
using System.Collections.Generic;

namespace HitMe.Game
{
    [Serializable]
    public class WeaponCosmeticItem
    {
        public string id;
        public string nameVi;
        public string nameEn;
        public string descriptionVi;
        public string descriptionEn;
        public int priceGold;
        public string iconAssetPath;
        public bool hasVisualEffect;
        public bool isOwned;
        public bool isEquipped;

        public WeaponCosmeticItem(string id, string nameVi, string nameEn, string descVi, string descEn, int price, string iconPath, bool hasEffect, bool owned = false, bool equipped = false)
        {
            this.id = id;
            this.nameVi = nameVi;
            this.nameEn = nameEn;
            this.descriptionVi = descVi;
            this.descriptionEn = descEn;
            this.priceGold = price;
            this.iconAssetPath = iconPath;
            this.hasVisualEffect = hasEffect;
            this.isOwned = owned;
            this.isEquipped = equipped;
        }
    }

    public interface IInventoryService
    {
        IReadOnlyList<WeaponCosmeticItem> GetAllWeapons();
        WeaponCosmeticItem GetEquippedWeapon();
        bool BuyWeapon(string itemId, IAccountService account);
        bool EquipWeapon(string itemId);
        bool IsWeaponOwned(string itemId);
    }

    /// <summary>
    /// Pure in-memory FakeInventoryService for UI/UX production demonstration.
    /// Manages cosmetic Vietnamese weapons catalog and equipped state.
    /// Does not alter in-game combat balance or physics.
    /// </summary>
    public class FakeInventoryService : IInventoryService
    {
        private static FakeInventoryService _instance;
        public static FakeInventoryService Instance => _instance ?? (_instance = new FakeInventoryService());

        private readonly List<WeaponCosmeticItem> _weapons = new List<WeaponCosmeticItem>();

        public event Action OnInventoryUpdated;

        public FakeInventoryService()
        {
            _instance = this;
            InitializeCatalog();
        }

        private void InitializeCatalog()
        {
            _weapons.Clear();
            // Default Slipper (Always owned & equipped by default)
            _weapons.Add(new WeaponCosmeticItem(
                "dep_to_ong",
                "Dép Tổ Ong", "Honeycomb Slipper",
                "Vũ khí huyền thoại làng quê Việt Nam. Chuẩn xác, bay thẳng.",
                "Legendary Vietnamese village slipper. Accurate, straight trajectory.",
                0, "Assets/Art/Weapons/dep_to_ong_icon.png", true, owned: true, equipped: true
            ));

            // Chảo Chống Dính (100 Vàng)
            _weapons.Add(new WeaponCosmeticItem(
                "chao_chong_dinh",
                "Chảo Chống Dính", "Non-stick Frying Pan",
                "Chảo rán quen thuộc của mẹ. Tiếng va chạm cực đanh.",
                "Mom's favorite kitchen pan. Crisp metallic impact.",
                100, "Assets/Art/Weapons/chao_chong_dinh_icon.png", true, owned: false, equipped: false
            ));

            // Cốc Bia Hơi (150 Vàng = 100 * 1.5)
            _weapons.Add(new WeaponCosmeticItem(
                "coc_bia",
                "Cốc Bia Hơi", "Draught Beer Mug",
                "Cốc thủy tinh bọt xanh vỉa hè Hà Nội chiều hè.",
                "Classic Hanoi street beer glass with frothy head.",
                150, "Assets/Art/Weapons/coc_bia_icon.png", true, owned: false, equipped: false
            ));

            // Điếu Cày (225 Vàng = 150 * 1.5)
            _weapons.Add(new WeaponCosmeticItem(
                "dieu_cay",
                "Điếu Cày", "Bamboo Waterpipe",
                "Điếu cày nứa gốc đa quán nước đầu làng.",
                "Traditional bamboo waterpipe from village banyan tree.",
                225, "Assets/Art/Weapons/dieu_cay_icon.png", true, owned: false, equipped: false
            ));

            // Búa Đóng Đinh (335 Vàng = 225 * 1.5)
            _weapons.Add(new WeaponCosmeticItem(
                "bua_dong_dinh",
                "Búa Đóng Đinh", "Claw Hammer",
                "Búa sắt cầm chắc nịch của bác thợ mộc trong làng.",
                "Sturdy carpenter iron hammer with firm grip.",
                335, "Assets/Art/Weapons/bua_dong_dinh_icon.png", true, owned: false, equipped: false
            ));

            // Vợt Pickleball (500 Vàng = 335 * 1.5)
            _weapons.Add(new WeaponCosmeticItem(
                "vot_pickleball",
                "Vợt Pickleball", "Pickleball Paddle",
                "Môn thể thao thịnh hành trên mọi sân tập thể.",
                "Trendy paddle sweeping across municipal sports courts.",
                500, "Assets/Art/Weapons/vot_pickleball_icon.png", true, owned: false, equipped: false
            ));

            // Xe Đạp Phượng Hoàng (750 Vàng = 500 * 1.5) - Thiếu sprite hiệu ứng
            _weapons.Add(new WeaponCosmeticItem(
                "xe_dap",
                "Xe Đạp Phượng Hoàng", "Phoenix Bicycle",
                "Kỷ vật thời bao cấp vang bóng một thời. (Thiếu hiệu ứng)",
                "Vintage subsidy-era bicycle memorabilia. (Missing effect)",
                750, "Thiếu", false, owned: false, equipped: false
            ));

            // Honda Cũ (1125 Vàng = 750 * 1.5) - Thiếu sprite hiệu ứng
            _weapons.Add(new WeaponCosmeticItem(
                "honda_cu",
                "Honda Cũ", "Vintage Honda Cub",
                "Chiếc xe máy huyền thoại qua từng con ngõ. (Thiếu hiệu ứng)",
                "Legendary cub motorcycle running through every alley. (Missing effect)",
                1125, "Thiếu", false, owned: false, equipped: false
            ));
        }

        public IReadOnlyList<WeaponCosmeticItem> GetAllWeapons() => _weapons;

        public WeaponCosmeticItem GetEquippedWeapon()
        {
            return _weapons.Find(w => w.isEquipped) ?? _weapons[0];
        }

        public bool IsWeaponOwned(string itemId)
        {
            var item = _weapons.Find(w => w.id == itemId);
            return item != null && item.isOwned;
        }

        public bool BuyWeapon(string itemId, IAccountService account)
        {
            var item = _weapons.Find(w => w.id == itemId);
            if (item == null || item.isOwned || account == null) return false;

            if (account.DeductGold(item.priceGold))
            {
                item.isOwned = true;
                OnInventoryUpdated?.Invoke();
                return true;
            }
            return false;
        }

        public bool EquipWeapon(string itemId)
        {
            var target = _weapons.Find(w => w.id == itemId);
            if (target == null || !target.isOwned) return false;

            foreach (var w in _weapons)
            {
                w.isEquipped = (w.id == itemId);
            }
            OnInventoryUpdated?.Invoke();
            return true;
        }

        public void ResetForTest()
        {
            InitializeCatalog();
            OnInventoryUpdated?.Invoke();
        }
    }
}
