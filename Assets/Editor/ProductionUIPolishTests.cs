using System.Collections.Generic;
using HitMe.Core;
using HitMe.Game;
using HitMe.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HitMe.Editor
{
    public sealed class ProductionUIPolishTests
    {
        private const string GrayboxScenePath = "Assets/Scenes/Graybox.unity";

        [Test]
        public void P1_MatchStatePhase_IsSingleSourceOfTruth_AndRound5EntersMatchEndImmediately()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            var match = Object.FindFirstObjectByType<MatchController>();
            Assert.That(ui, Is.Not.Null, "ProductionUIController must exist in Graybox scene");
            Assert.That(match, Is.Not.Null, "MatchController must exist in Graybox scene");

            ui.ResolveHudElementContainers();

            var config = match.configAsset != null ? match.configAsset.ToCore() : new GameConfig();
            config.MaxRounds = 5;

            var players = new List<PlayerState>
            {
                new PlayerState("player", "Nam Làng", false, 3, new Vec2(-2.5f, -2.0f), Vec2.Up),
                new PlayerState("bot-1", "Anh Teen", true, 3, new Vec2(-1.5f, -2.0f), Vec2.Up),
                new PlayerState("bot-2", "Cô Ba", true, 3, new Vec2(-0.5f, -2.0f), Vec2.Up),
                new PlayerState("bot-3", "Chó Vàng", true, 3, new Vec2(0.5f, -2.0f), Vec2.Up),
                new PlayerState("bot-4", "Nữ Năng Động", true, 3, new Vec2(1.5f, -2.0f), Vec2.Up),
                new PlayerState("bot-5", "Mèo Mun", true, 3, new Vec2(2.5f, -2.0f), Vec2.Up)
            };

            var state = new MatchState(config, players);

            // Simulate 5 rounds reaching maxRounds = 5 with multiple survivors (like user screenshot)
            for (int r = 1; r <= 5; r++)
            {
                state.StartRound();
                for (int i = 0; i < state.Players.Count; i++)
                {
                    var p = state.Players[i];
                    if (!p.IsAlive) continue;
                    float x = -2.5f + i * 1.0f;
                    state.SetInput(new RoundInput(p.Id)
                    {
                        HasPlacement = true,
                        Placement = new Vec2(x, -2.0f),
                        HasAim = true,
                        AimDirection = Vec2.Up,
                        IsReady = true
                    });
                }
                state.LockRound();
                state.BeginReveal();
                state.BeginThrowing();
                if (r == 5)
                {
                    // Give players varied HP & HitsLanded before Round 5 resolves
                    state.Players[0].Hp = 2; state.Players[0].HitsLanded = 3;
                    state.Players[1].Hp = 2; state.Players[1].HitsLanded = 3;
                    state.Players[2].Hp = 1; state.Players[2].HitsLanded = 2;
                    state.Players[3].Hp = 0; state.Players[3].HitsLanded = 1;
                    state.Players[4].Hp = 2; state.Players[4].HitsLanded = 1;
                    state.Players[5].Hp = 0; state.Players[5].HitsLanded = 0;
                }
                state.ResolveRound();
            }

            // Round 5/5 must immediately be Phase.MatchEnd in MatchState
            Assert.That(state.RoundNumber, Is.EqualTo(5));
            Assert.That(state.Phase, Is.EqualTo(Phase.MatchEnd), "Round 5/5 reaching maxRounds must set Phase.MatchEnd immediately");

            ui.ShowScreen(ProductionScreen.Gameplay);
            ui.ApplyPhaseState(state.Phase);
            ui.PopulateResultFromMatchState(state);

            // P1 assertions: when MatchEnd, timer, SẴN SÀNG, instruction HUD, and KẾT QUẢ VÒNG are hidden
            Assert.That(ui.CurrentScreen, Is.EqualTo(ProductionScreen.Result), "Must switch to Result screen on MatchEnd");
            Assert.That(ui.resultPanel.activeInHierarchy, Is.True, "ResultPanel must be active on MatchEnd");
            Assert.That(ui.gameplayHudPanel.activeInHierarchy, Is.False, "GameplayHudPanel must be inactive on MatchEnd");
            Assert.That(ui.txtTimer.gameObject.activeInHierarchy, Is.False, "Timer must be hidden on MatchEnd");
            Assert.That(ui.btnInGameReady.gameObject.activeInHierarchy, Is.False, "SẴN SÀNG button must be hidden on MatchEnd");
            Assert.That(ui.txtPhaseBanner.gameObject.activeInHierarchy, Is.False, "PhaseBanner ('KẾT QUẢ VÒNG') must be hidden on MatchEnd");
            Assert.That(ui.txtWeaponIndicator.gameObject.activeInHierarchy, Is.False, "WeaponPill must be hidden on MatchEnd");

            // Verify all 6 rows are populated with Rank, Name, HP + HIT, and tie/win status
            Assert.That(ui.resultRows.Count, Is.EqualTo(6));
            for (int i = 0; i < 6; i++)
            {
                var row = ui.resultRows[i];
                Assert.That(row.root.activeSelf, Is.True, $"Result row {i} must be active");
                Assert.That(row.txtRank.text, Is.EqualTo("#" + (i + 1)));
                Assert.That(row.txtHp.text, Does.Contain("HP:").And.Contain("HIT:"), $"Row {i} must show both final HP and hit count");
            }

            // Top 2 tied on Alive + HP (2) + HitsLanded (3) -> both should be marked ★ ĐỒNG HẠNG 1
            Assert.That(ui.resultRows[0].txtStatus.text, Does.Contain("ĐỒNG HẠNG 1"));
            Assert.That(ui.resultRows[1].txtStatus.text, Does.Contain("ĐỒNG HẠNG 1"));
            // Eliminated players at bottom
            Assert.That(ui.resultRows[4].txtStatus.text, Does.Contain("Đã loại"));
            Assert.That(ui.resultRows[5].txtStatus.text, Does.Contain("Đã loại"));

            // Replay button must be wired and active
            Assert.That(ui.btnReplay, Is.Not.Null);
            Assert.That(ui.btnReplay.gameObject.activeInHierarchy, Is.True);
        }

        [Test]
        public void P1_TopPlacingControls_OnlyVisibleInPlacingPhase()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);

            ui.ShowScreen(ProductionScreen.Gameplay);

            ui.ApplyPhaseState(Phase.Placing);
            Assert.That(ui.txtRound.gameObject.activeInHierarchy, Is.True, "Round visible in Placing");
            Assert.That(ui.txtTimer.gameObject.activeInHierarchy, Is.True, "Timer visible in Placing");
            Assert.That(ui.btnInGameReady.gameObject.activeInHierarchy, Is.True, "Ready button visible in Placing");
            Assert.That(ui.txtPhaseBanner.gameObject.activeInHierarchy, Is.True, "PhaseBanner visible in Placing");

            Phase[] nonPlacingPhases = { Phase.Locked, Phase.Reveal, Phase.Throwing, Phase.RoundEnd };
            foreach (var phase in nonPlacingPhases)
            {
                ui.ApplyPhaseState(phase);
                Assert.That(ui.txtRound.gameObject.activeInHierarchy, Is.False, $"Round must be hidden in {phase}");
                Assert.That(ui.txtTimer.gameObject.activeInHierarchy, Is.False, $"Timer must be hidden in {phase}");
                Assert.That(ui.btnInGameReady.gameObject.activeInHierarchy, Is.False, $"Ready button must be hidden in {phase}");
                Assert.That(ui.txtPhaseBanner.gameObject.activeInHierarchy, Is.True, $"PhaseBanner must remain visible in {phase}");
                Assert.That(ui.txtWeaponIndicator.gameObject.activeInHierarchy, Is.True, $"WeaponPill must remain visible in {phase}");
            }
        }

        [Test]
        public void P2_PortraitCanvasScaler_AndSafeAreaFitter_ClampsWideDesktopAndIphoneSafeArea()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);

            var scaler = ui.GetComponent<CanvasScaler>();
            Assert.That(scaler, Is.Not.Null);
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1080f, 2340f)));

            var fitter = ui.GetComponentInChildren<SafeAreaFitter>(true);
            Assert.That(fitter, Is.Not.Null, "SafeAreaFitter must exist on SafeAreaRoot under ProductionCanvas");
            var safeRt = fitter.GetComponent<RectTransform>();

            // 1. Desktop Portrait 1080x2340
            fitter.ApplyForScreen(1080, 2340, new Rect(0f, 0f, 1080f, 2340f), false);
            Assert.That(safeRt.anchorMin.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(safeRt.anchorMax.x, Is.EqualTo(1f).Within(0.001f));

            // 2. iPhone Portrait 1080x2340 with top notch (102px) & bottom home indicator (80px)
            fitter.ApplyForScreen(1080, 2340, new Rect(0f, 0f, 1080f, 2340f), true);
            Assert.That(safeRt.anchorMin.y, Is.GreaterThanOrEqualTo(80f / 2340f - 0.0001f), "iPhone bottom safe inset applied");
            Assert.That(safeRt.anchorMax.y, Is.LessThanOrEqualTo(1f - 102f / 2340f + 0.0001f), "iPhone top safe inset applied");

            // 3. Wide Desktop 1920x1080 -> HUD must stay anchored inside centered 9:19.5 portrait column
            fitter.ApplyForScreen(1920, 1080, new Rect(0f, 0f, 1920f, 1080f), false);
            float expectedPortraitWidthNorm = (1080f * (1080f / 2340f)) / 1920f; // ~0.2596
            float actualWidthNorm = safeRt.anchorMax.x - safeRt.anchorMin.x;
            Assert.That(actualWidthNorm, Is.EqualTo(expectedPortraitWidthNorm).Within(0.005f),
                "On 1920x1080 wide desktop, SafeAreaRoot must clamp HUD width to centered 1080:2340 portrait column");
            Assert.That(safeRt.anchorMin.x, Is.GreaterThan(0.35f));
            Assert.That(safeRt.anchorMax.x, Is.LessThan(0.65f));
        }

        [Test]
        public void P2_P3_HudDoesNotCoverArenaGate_AndWeaponPillIsRightBelowArena()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            var cam = GameObject.Find("Main Camera")?.GetComponent<Camera>();
            var bgSr = GameObject.Find("ArenaBackground")?.GetComponent<SpriteRenderer>();

            Assert.That(ui, Is.Not.Null);
            Assert.That(cam, Is.Not.Null);
            Assert.That(bgSr, Is.Not.Null);

            float camBottomWorldY = cam.transform.position.y - cam.orthographicSize;
            float camHeightWorld = cam.orthographicSize * 2f;

            float arenaTopScreenNormY = (bgSr.bounds.max.y - camBottomWorldY) / camHeightWorld;
            float arenaBottomScreenNormY = (bgSr.bounds.min.y - camBottomWorldY) / camHeightWorld;

            var fitter = ui.GetComponentInChildren<SafeAreaFitter>(true);
            var safeRt = fitter.GetComponent<RectTransform>();
            var hpRt = ui.hpCardsContainer.GetComponent<RectTransform>();
            var weaponRt = ui.weaponPillObj.GetComponent<RectTransform>();

            float hpBottomInSafe = hpRt.anchorMin.y - (hpRt.sizeDelta.y * 0.5f) / 2340f;
            float weaponTopInSafe = weaponRt.anchorMax.y + (weaponRt.sizeDelta.y * 0.5f) / 2340f;

            // Check on both Desktop Portrait and iPhone Safe Area
            foreach (bool isIphone in new[] { false, true })
            {
                fitter.ApplyForScreen(1080, 2340, new Rect(0f, 0f, 1080f, 2340f), isIphone);
                float safeSpanY = safeRt.anchorMax.y - safeRt.anchorMin.y;

                float hpBottomScreenNormY = safeRt.anchorMin.y + hpBottomInSafe * safeSpanY;
                Assert.That(hpBottomScreenNormY, Is.GreaterThan(arenaTopScreenNormY),
                    $"HpContainer bottom ({hpBottomScreenNormY:F3}) must stay above ArenaBackground top ({arenaTopScreenNormY:F3}) [isIphone={isIphone}]");

                float weaponTopScreenNormY = safeRt.anchorMin.y + weaponTopInSafe * safeSpanY;
                float gapBelowArena = arenaBottomScreenNormY - weaponTopScreenNormY;
                Assert.That(gapBelowArena, Is.GreaterThanOrEqualTo(-0.005f).And.LessThan(0.05f),
                    $"WeaponPill must sit right below the bottom of the arena (gap={gapBelowArena:F3}) [isIphone={isIphone}]");
            }
        }

        [Test]
        public void P3_SixPlayerCards_TwoRows_NoOverlappingText_AndTopPlacingRowAligned()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.hpCards.Count, Is.EqualTo(6));

            // 1. Verify TopPlacingBar has RoundPill, TimerPill, and BtnReady in the same horizontal row
            Assert.That(ui.topPlacingBar, Is.Not.Null);
            Assert.That(ui.roundPill.transform.parent, Is.EqualTo(ui.topPlacingBar.transform));
            Assert.That(ui.timerPill.transform.parent, Is.EqualTo(ui.topPlacingBar.transform));
            Assert.That(ui.btnInGameReady.transform.parent, Is.EqualTo(ui.topPlacingBar.transform));

            var roundRt = ui.roundPill.GetComponent<RectTransform>();
            var timerRt = ui.timerPill.GetComponent<RectTransform>();
            var readyRt = ui.btnInGameReady.GetComponent<RectTransform>();
            Assert.That(roundRt.anchorMin.y, Is.EqualTo(timerRt.anchorMin.y).Within(0.001f));
            Assert.That(timerRt.anchorMin.y, Is.EqualTo(readyRt.anchorMin.y).Within(0.001f));

            // 2. Verify 6 HP cards are in 2 rows x 3 columns without overlapping each other
            var containerRt = ui.hpCardsContainer.GetComponent<RectTransform>();
            Vector2 containerSize = containerRt.sizeDelta;
            Rect[] cardRects = new Rect[6];
            for (int i = 0; i < 6; i++)
            {
                var card = ui.hpCards[i];
                var rt = card.root.GetComponent<RectTransform>();
                Vector2 center = new Vector2(rt.anchorMin.x * containerSize.x, rt.anchorMin.y * containerSize.y);
                cardRects[i] = new Rect(center - rt.sizeDelta * 0.5f, rt.sizeDelta);

                // Verify internal text rects (Name, Hearts, HpNum) do not overlap inside each card
                Rect rName = GetLocalBoxInCard(card.txtName.rectTransform, rt.sizeDelta);
                Rect rHearts = GetLocalBoxInCard(card.txtHearts.rectTransform, rt.sizeDelta);
                Rect rStatus = GetLocalBoxInCard(card.txtHpNum.rectTransform, rt.sizeDelta);

                Assert.That(rName.Overlaps(rHearts), Is.False, $"Card {i}: Name and Hearts text rects must not overlap");
                Assert.That(rName.Overlaps(rStatus), Is.False, $"Card {i}: Name and Status text rects must not overlap");
                Assert.That(rHearts.Overlaps(rStatus), Is.False, $"Card {i}: Hearts and Status text rects must not overlap");
            }

            // Cards 0..2 in top row, Cards 3..5 in bottom row
            Assert.That(cardRects[0].center.y, Is.GreaterThan(cardRects[3].center.y), "Row 0 must be above Row 1");
            for (int i = 0; i < 6; i++)
            {
                for (int j = i + 1; j < 6; j++)
                {
                    Assert.That(cardRects[i].Overlaps(cardRects[j]), Is.False, $"HP Card {i} and Card {j} must not overlap");
                }
            }
        }

        private static Rect GetLocalBoxInCard(RectTransform childRt, Vector2 cardSize)
        {
            Vector2 anchorPos = new Vector2(childRt.anchorMin.x * cardSize.x, childRt.anchorMin.y * cardSize.y);
            Vector2 size = childRt.sizeDelta;
            Vector2 min = anchorPos - Vector2.Scale(size, childRt.pivot);
            return new Rect(min, size);
        }
    }
}
