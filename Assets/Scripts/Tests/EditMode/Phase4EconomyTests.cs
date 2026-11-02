using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using HitMe.Economy;
using HitMe.Game;

namespace HitMe.Tests
{
    [TestFixture]
    public class Phase4EconomyTests
    {
        [SetUp]
        public void Setup()
        {
            ItemCatalog.ResetForTesting();
            RankService.ResetForTesting();
            QuestService.ResetForTesting();
            FakeAccountService.Instance.ResetForTest("Khách_Test", 500);
        }

        [TearDown]
        public void Teardown()
        {
            ItemCatalog.ResetForTesting();
            RankService.ResetForTesting();
            QuestService.ResetForTesting();
            FakeAccountService.Instance.ResetForTest("Khách_Test", 500);
        }

        [Test]
        public void Test_RankService_TiersAndPoints()
        {
            RankService.EnsureInitialized();
            var tiers = RankService.GetTiers();
            Assert.That(tiers.Count, Is.GreaterThanOrEqualTo(5));

            // Test tier thresholds
            Assert.That(RankService.GetTierForPoints(50).nameVi, Is.EqualTo("Đồng"));
            Assert.That(RankService.GetTierForPoints(150).nameVi, Is.EqualTo("Bạc"));
            Assert.That(RankService.GetTierForPoints(450).nameVi, Is.EqualTo("Vàng"));
            Assert.That(RankService.GetTierForPoints(750).nameVi, Is.EqualTo("Bạch Kim"));
            Assert.That(RankService.GetTierForPoints(1200).nameVi, Is.EqualTo("Kim Cương"));

            // Test delta calculation
            int p1_4p = RankService.CalculatePointsDelta(1, 4);
            int p4_4p = RankService.CalculatePointsDelta(4, 4);
            Assert.That(p1_4p, Is.EqualTo(25));
            Assert.That(p4_4p, Is.EqualTo(-15));

            // Test room size multipliers (2P = 0.6x, 6P = 1.2x)
            int p1_2p = RankService.CalculatePointsDelta(1, 2);
            int p1_6p = RankService.CalculatePointsDelta(1, 6);
            Assert.That(p1_2p, Is.EqualTo(15)); // 25 * 0.6 = 15
            Assert.That(p1_6p, Is.EqualTo(30)); // 25 * 1.2 = 30

            // Mock leaderboard has top 5
            var lb = RankService.GetMockLeaderboard();
            Assert.That(lb.Count, Is.EqualTo(5));
            Assert.That(lb[0].name, Is.EqualTo("Nguyễn_Vô_Địch"));
        }

        [Test]
        public void Test_QuestService_DailyProgressAndClaim()
        {
            QuestService.EnsureInitialized();
            var quests = QuestService.GetQuests();
            Assert.That(quests.Count, Is.EqualTo(3));

            var qMatches = quests[0];
            var qHits = quests[1];
            var qWin = quests[2];

            Assert.That(qMatches.id, Is.EqualTo("play_3_matches"));
            Assert.That(qHits.id, Is.EqualTo("land_5_hits"));
            Assert.That(qWin.id, Is.EqualTo("win_1_match"));

            // Initially not completed
            Assert.That(qMatches.IsCompleted, Is.False);
            Assert.That(QuestService.ClaimReward("play_3_matches", out _), Is.False);

            // Record 1 non-win match with 2 hits
            QuestService.RecordMatchPlayed(isWin: false, hitsLanded: 2);
            Assert.That(qMatches.currentProgress, Is.EqualTo(1));
            Assert.That(qHits.currentProgress, Is.EqualTo(2));
            Assert.That(qWin.currentProgress, Is.EqualTo(0));

            // Record 2nd match: win with 3 hits
            QuestService.RecordMatchPlayed(isWin: true, hitsLanded: 3);
            Assert.That(qMatches.currentProgress, Is.EqualTo(2));
            Assert.That(qHits.currentProgress, Is.EqualTo(5)); // Completed (target 5)
            Assert.That(qWin.currentProgress, Is.EqualTo(1));  // Completed (target 1)

            Assert.That(qHits.IsCompleted, Is.True);
            Assert.That(qWin.IsCompleted, Is.True);

            // Claim hit quest reward (+60 gold)
            bool claimHits = QuestService.ClaimReward("land_5_hits", out int hitGold);
            Assert.That(claimHits, Is.True);
            Assert.That(hitGold, Is.EqualTo(60));
            Assert.That(qHits.isClaimed, Is.True);

            // Cannot claim twice
            bool claimHitsAgain = QuestService.ClaimReward("land_5_hits", out _);
            Assert.That(claimHitsAgain, Is.False);
        }

        [Test]
        public void Test_Account_RenameRules_FirstFree_SecondCosts99()
        {
            var acc = FakeAccountService.Instance;
            acc.ResetForTest("Khách_Init", 500);

            // First rename is free
            bool r1 = acc.Rename("GameThu_VN", 0);
            Assert.That(r1, Is.True);
            Assert.That(acc.DisplayName, Is.EqualTo("GameThu_VN"));
            Assert.That(acc.Gold, Is.EqualTo(500), "First rename must not deduct gold");
            Assert.That(acc.NameChangeCount, Is.EqualTo(1));

            // Second rename costs 99 gold
            bool r2 = acc.Rename("ProPlayer_99", 99);
            Assert.That(r2, Is.True);
            Assert.That(acc.DisplayName, Is.EqualTo("ProPlayer_99"));
            Assert.That(acc.Gold, Is.EqualTo(401), "Second rename must deduct exactly 99 gold");
            Assert.That(acc.NameChangeCount, Is.EqualTo(2));

            // Cannot rename if balance is insufficient
            acc.DeductGold(400); // Leaves 1 gold
            bool r3 = acc.Rename("TooPoor", 99);
            Assert.That(r3, Is.False, "Cannot rename when insufficient gold");
            Assert.That(acc.DisplayName, Is.EqualTo("ProPlayer_99"));
        }

        [Test]
        public void Test_ItemCatalog_BuyAndEquipWeapons()
        {
            ItemCatalog.EnsureInitialized();
            var items = ItemCatalog.GetItems();
            Assert.That(items.Count, Is.GreaterThanOrEqualTo(7));

            // Starter weapon is dep_to_ong
            Assert.That(ItemCatalog.IsOwned("dep_to_ong"), Is.True);
            Assert.That(ItemCatalog.EquippedWeaponId, Is.EqualTo("dep_to_ong"));

            // Buy chao_ran (199G)
            int currentGold = 500;
            bool boughtChao = ItemCatalog.BuyItem("chao_ran", ref currentGold, out string err);
            Assert.That(boughtChao, Is.True);
            Assert.That(currentGold, Is.EqualTo(301));
            Assert.That(ItemCatalog.IsOwned("chao_ran"), Is.True);

            // Cannot buy duplicate item
            bool buyAgain = ItemCatalog.BuyItem("chao_ran", ref currentGold, out string err2);
            Assert.That(buyAgain, Is.False);
            Assert.That(err2, Is.EqualTo("already_owned"));

            // Equip newly bought weapon
            bool equipped = ItemCatalog.EquipItem("chao_ran");
            Assert.That(equipped, Is.True);
            Assert.That(ItemCatalog.EquippedWeaponId, Is.EqualTo("chao_ran"));

            // Cannot equip unowned weapon
            bool equipUnowned = ItemCatalog.EquipItem("xe_dap_phuong_hoang");
            Assert.That(equipUnowned, Is.False);
            Assert.That(ItemCatalog.EquippedWeaponId, Is.EqualTo("chao_ran"));
        }

        [Test]
        public void Test_MissingAssets_PlaceholderFlags()
        {
            ItemCatalog.EnsureInitialized();
            var missingList = new HashSet<string> { "coc_bia_hoi", "xe_dap_phuong_hoang", "xe_may_honda_cub", "chu_cho_vang" };

            foreach (var missingId in missingList)
            {
                var item = ItemCatalog.GetItem(missingId);
                Assert.That(item, Is.Not.Null, $"Item {missingId} must be defined in catalog");
                Assert.That(item.price, Is.GreaterThan(0));
            }
        }
    }
}
