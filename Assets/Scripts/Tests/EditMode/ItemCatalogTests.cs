using System.IO;
using NUnit.Framework;
using UnityEngine;
using HitMe.Economy;

namespace HitMe.Tests
{
    [TestFixture]
    public class ItemCatalogTests
    {
        private string _json;

        [SetUp]
        public void SetUp()
        {
            ItemCatalog.ResetForTesting();
            string dataPath = Path.Combine(Application.dataPath, "Data", "Items.json");
            if (File.Exists(dataPath))
            {
                _json = File.ReadAllText(dataPath);
            }
            else
            {
                _json = @"{
                    ""version"": 1,
                    ""items"": [
                        {""id"": ""dep_to_ong"", ""nameVi"": ""Dép tổ ong"", ""nameEn"": ""Slipper"", ""price"": 0, ""isStarter"": true},
                        {""id"": ""chao_ran"", ""nameVi"": ""Chảo rán"", ""nameEn"": ""Frying Pan"", ""price"": 199, ""isStarter"": false},
                        {""id"": ""vot_pickleball"", ""nameVi"": ""Vợt Pickleball"", ""nameEn"": ""Pickleball Paddle"", ""price"": 260, ""isStarter"": false}
                    ]
                }";
            }
            ItemCatalog.Initialize(_json);
        }

        [Test]
        public void Test_Initialize_LoadsItemsFromJson()
        {
            var items = ItemCatalog.GetItems();
            Assert.IsNotNull(items);
            Assert.GreaterOrEqual(items.Count, 3);
            Assert.IsNotNull(ItemCatalog.GetItem("dep_to_ong"));
        }

        [Test]
        public void Test_StarterItem_IsOwnedByDefault()
        {
            Assert.IsTrue(ItemCatalog.IsOwned("dep_to_ong"));
            Assert.AreEqual("dep_to_ong", ItemCatalog.EquippedWeaponId);
        }

        [Test]
        public void Test_BuyItem_DeductsGold_AndAddsToInventory()
        {
            int gold = 500;
            bool ok = ItemCatalog.BuyItem("chao_ran", ref gold, out string error);

            Assert.IsTrue(ok, $"Buy should succeed but got error: {error}");
            Assert.AreEqual(301, gold, "Gold should be deducted by 199");
            Assert.IsTrue(ItemCatalog.IsOwned("chao_ran"));
        }

        [Test]
        public void Test_BuyItem_FailsWhenInsufficientGold()
        {
            int gold = 50;
            bool ok = ItemCatalog.BuyItem("chao_ran", ref gold, out string error);

            Assert.IsFalse(ok);
            Assert.AreEqual("not_enough_gold", error);
            Assert.AreEqual(50, gold, "Gold should remain unchanged");
            Assert.IsFalse(ItemCatalog.IsOwned("chao_ran"));
        }

        [Test]
        public void Test_BuyItem_FailsWhenAlreadyOwned()
        {
            int gold = 1000;
            ItemCatalog.BuyItem("chao_ran", ref gold, out _);

            bool okSecond = ItemCatalog.BuyItem("chao_ran", ref gold, out string error);
            Assert.IsFalse(okSecond);
            Assert.AreEqual("already_owned", error);
        }

        [Test]
        public void Test_EquipItem_OnlyAllowsOwnedItems()
        {
            Assert.IsFalse(ItemCatalog.EquipItem("vot_pickleball"), "Cannot equip unowned item");

            int gold = 1000;
            ItemCatalog.BuyItem("vot_pickleball", ref gold, out _);
            Assert.IsTrue(ItemCatalog.EquipItem("vot_pickleball"), "Can equip owned item");
            Assert.AreEqual("vot_pickleball", ItemCatalog.EquippedWeaponId);
        }

        [Test]
        public void Test_PricingPolicy_MatchesSpecification()
        {
            var dep = ItemCatalog.GetItem("dep_to_ong");
            Assert.AreEqual(0, dep.price, "Starter slipper must be free (0 gold)");

            var chao = ItemCatalog.GetItem("chao_ran");
            Assert.AreEqual(199, chao.price, "First paid item base price is 199 gold");

            var vot = ItemCatalog.GetItem("vot_pickleball");
            Assert.AreEqual(260, vot.price, "Second paid item is round(199 * 1.3, -1) = 260 gold");
        }
    }
}
