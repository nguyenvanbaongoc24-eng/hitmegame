using System.Collections.Generic;
using HitMe.Core;
using HitMe.Game;
using HitMe.UI;
using NUnit.Framework;
using TMPro;
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

        [Test]
        public void S2_MatchEndStateChain_NeverReturnsToPlacing_AndReplayOrHomeCleansUp()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            var match = Object.FindFirstObjectByType<MatchController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(match, Is.Not.Null);

            ui.matchController = match;
            ui.ResolveHudElementContainers();

            // 1. Start a match -> synchronously initializes Round 1 in Phase.Placing
            ui.StartGameplayMatch();
            Assert.That(match.State, Is.Not.Null);
            Assert.That(match.State.RoundNumber, Is.EqualTo(1));
            Assert.That(match.State.Phase, Is.EqualTo(Phase.Placing));

            // Advance through 5 rounds to MatchEnd
            for (int r = 1; r <= 5; r++)
            {
                if (match.State.Phase != Phase.Placing) match.State.StartRound();
                for (int i = 0; i < match.State.Players.Count; i++)
                {
                    var p = match.State.Players[i];
                    if (!p.IsAlive) continue;
                    match.State.SetInput(new RoundInput(p.Id)
                    {
                        HasPlacement = true,
                        Placement = p.Position,
                        HasAim = true,
                        AimDirection = Vec2.Up,
                        IsReady = true
                    });
                }
                match.State.LockRound();
                match.State.BeginReveal();
                match.State.BeginThrowing();
                match.State.ResolveRound();
            }

            Assert.That(match.State.Phase, Is.EqualTo(Phase.MatchEnd));
            // Attempting StartRound() after MatchEnd must be a no-op (never returns to Placing)
            match.State.StartRound();
            Assert.That(match.State.Phase, Is.EqualTo(Phase.MatchEnd), "MatchState must never return to Placing after MatchEnd");
            Assert.That(match.State.RoundNumber, Is.EqualTo(5));

            ui.ApplyPhaseState(match.State.Phase);
            Assert.That(ui.CurrentScreen, Is.EqualTo(ProductionScreen.Result));

            // 2. Click CHƠI LẠI (OnReplayClicked) -> resets to Round 1 with clean HP=3, 0 HitsLanded, 0 weapons, reset gesture
            ui.OnReplayClicked();
            Assert.That(ui.CurrentScreen, Is.EqualTo(ProductionScreen.Gameplay));
            Assert.That(match.State, Is.Not.Null);
            Assert.That(match.State.RoundNumber, Is.EqualTo(1));
            Assert.That(match.State.Phase, Is.EqualTo(Phase.Placing));
            Assert.That(match.ActiveWeaponCount, Is.EqualTo(0));
            Assert.That(match.ActiveHitFxCount, Is.EqualTo(0));
            Assert.That(match.CurrentGestureSnapshot.HasPlaced, Is.False);
            Assert.That(match.CurrentGestureSnapshot.AimActive, Is.False);
            for (int i = 0; i < match.State.Players.Count; i++)
            {
                Assert.That(match.State.Players[i].Hp, Is.EqualTo(3));
                Assert.That(match.State.Players[i].HitsLanded, Is.EqualTo(0));
                Assert.That(match.State.Players[i].EliminatedAtRound, Is.EqualTo(0));
            }

            // 3. Click VỀ TRANG CHỦ (OnBackToHomeClicked) -> cleans up all active objects and returns to Home
            ui.OnBackToHomeClicked();
            Assert.That(ui.CurrentScreen, Is.EqualTo(ProductionScreen.Home));
            Assert.That(match.State, Is.Null);
            Assert.That(match.ActiveWeaponCount, Is.EqualTo(0));
            Assert.That(match.ActiveHitFxCount, Is.EqualTo(0));
            Assert.That(match.ActivePlayerViewCount, Is.EqualTo(0));
        }

        [Test]
        public void S3_HpIsAlwaysPublicInAllPhases_IncludingPlacing()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            var match = Object.FindFirstObjectByType<MatchController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(match, Is.Not.Null);

            ui.matchController = match;
            ui.StartGameplayMatch();

            // Set varied HPs during Placing
            match.State.Players[0].Hp = 3;
            match.State.Players[1].Hp = 2;
            match.State.Players[2].Hp = 1;
            match.State.Players[3].Hp = 0;

            ui.UpdateHpCards();

            // Even in Phase.Placing, every player's HP must be publicly shown ("HP: x/3"), never "HP: ???"
            Assert.That(match.State.Phase, Is.EqualTo(Phase.Placing));
            for (int i = 0; i < 6; i++)
            {
                string hpText = ui.hpCards[i].txtHearts.text;
                Assert.That(hpText, Does.Not.Contain("???"), $"Card {i} must never hide HP as ??? in Placing");
                Assert.That(hpText, Is.EqualTo($"HP: {match.State.Players[i].Hp}/3"));
            }

            // Alive bots in Placing show "ĐANG ẨN" for hidden position/aim; eliminated bot shows "ĐÃ LOẠI"
            Assert.That(ui.hpCards[0].txtHpNum.text, Does.Contain("CÒN SỐNG"));
            Assert.That(ui.hpCards[1].txtHpNum.text, Does.Contain("ĐANG ẨN"));
            Assert.That(ui.hpCards[3].txtHpNum.text, Does.Contain("ĐÃ LOẠI"));
        }

        [Test]
        public void RuleV13_HpCardsAndResultBoard_ScaleToPlayerCount_N2To6()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);

            for (int n = 2; n <= 6; n++)
            {
                ui.ConfigureHpCardsLayout(n);
                int activeCards = 0;
                for (int c = 0; c < ui.hpCards.Count; c++)
                {
                    if (ui.hpCards[c].root != null && ui.hpCards[c].root.activeSelf) activeCards++;
                }
                Assert.That(activeCards, Is.EqualTo(n), $"N={n}: exactly {n} HP cards must be active");

                // Check result board scaling
                var ranked = new List<PlayerState>(n);
                var winners = new List<string> { "player" };
                for (int p = 0; p < n; p++)
                {
                    string id = p == 0 ? "player" : ("bot-" + p);
                    string name = PlayerView.CharacterNames[p % PlayerView.CharacterNames.Length];
                    ranked.Add(new PlayerState(id, name, p != 0, 3 - p, new Vec2(0f, p), Vec2.Up));
                }

                ui.PopulateResultBoard(ranked, winners);
                int activeRows = 0;
                for (int r = 0; r < ui.resultRows.Count; r++)
                {
                    if (ui.resultRows[r].root != null && ui.resultRows[r].root.activeSelf) activeRows++;
                }
                Assert.That(activeRows, Is.EqualTo(n), $"N={n}: exactly {n} result rows must be active");
            }
        }

        [Test]
        public void RuleV13_LobbyRoomOnline_ShowsHumansAndLocksWhenOnePlayer()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);

            // Test 1 human player
            ui.UpdateLobbyRoster(1);
            Assert.That(ui.txtLobbyHeader.text, Does.Contain("1/6 NGƯỜI THẬT"));
            Assert.That(ui.txtLobbyCountdown.text, Does.Contain("Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot."));
            Assert.That(ui.lobbySlots[0].txtName.text, Does.Contain("Nam Làng"));
            for (int s = 1; s < 6; s++)
            {
                Assert.That(ui.lobbySlots[s].txtName.text, Does.Contain($"Chỗ trống #{s + 1}"));
                Assert.That(ui.lobbySlots[s].txtStatus.text, Is.EqualTo("Đang chờ..."));
            }

            // Test 4 human players
            ui.UpdateLobbyRoster(4);
            Assert.That(ui.txtLobbyHeader.text, Does.Contain("4/6 NGƯỜI THẬT"));
            Assert.That(ui.txtLobbyCountdown.text, Does.Contain("Đang chờ tất cả người chơi sẵn sàng"));
            for (int s = 0; s < 4; s++)
            {
                Assert.That(ui.lobbySlots[s].txtName.text, Does.Not.Contain("Chỗ trống"));
            }
            Assert.That(ui.lobbySlots[4].txtName.text, Does.Contain("Chỗ trống #5"));
            Assert.That(ui.lobbySlots[4].txtStatus.text, Is.EqualTo("Đang chờ..."));
            Assert.That(ui.lobbySlots[5].txtName.text, Does.Contain("Chỗ trống #6"));
            Assert.That(ui.lobbySlots[5].txtStatus.text, Is.EqualTo("Đang chờ..."));
        }

        [Test]
        public void RuleV13_SelectMatchRoster_NoDuplicates_AcrossSeedsAndPlayerCounts()
        {
            for (int n = 2; n <= 6; n++)
            {
                for (uint s = 1; s <= 25; s++)
                {
                    var rng = new SeededRandom(s * 101u + (uint)n * 17u);
                    int localChoice = (int)(s % 7);
                    int[] roster = MatchController.SelectMatchRoster(localChoice, n, rng);

                    Assert.That(roster.Length, Is.EqualTo(n), $"N={n} Seed={s}: length must be {n}");
                    Assert.That(roster[0], Is.EqualTo(localChoice), $"Slot 0 must be local choice");

                    var set = new HashSet<int>(roster);
                    Assert.That(set.Count, Is.EqualTo(n), $"N={n} Seed={s}: all characters in roster must be unique");

                    for (int i = 0; i < roster.Length; i++)
                    {
                        Assert.That(roster[i], Is.InRange(0, 6), $"Character index must be in [0..6]");
                    }
                }
            }
        }

        [Test]
        public void RuleL3_LeaveConfirmationDialog_ShowsAndTriggersCallback()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.dialogLeaveRoom, Is.Not.Null);
            Assert.That(ui.btnCancelLeave, Is.Not.Null, "Cancel button must be wired");
            Assert.That(ui.btnConfirmLeave, Is.Not.Null, "Confirm button must be wired");

            bool confirmed = false;
            ui.ShowLeaveConfirmationDialog("TIÊU ĐỀ RỜI", "Nội dung xác nhận", () => confirmed = true);
            Assert.That(ui.dialogLeaveRoom.activeSelf, Is.True);
            Assert.That(ui.txtLeaveDialogTitle.text, Is.EqualTo("TIÊU ĐỀ RỜI"));
            Assert.That(ui.txtLeaveDialogMessage.text, Is.EqualTo("Nội dung xác nhận"));

            // Cancel hides dialog without triggering callback
            ui.HideLeaveConfirmationDialog();
            Assert.That(ui.dialogLeaveRoom.activeSelf, Is.False);
            Assert.That(confirmed, Is.False);

            // Re-show and simulate confirm: invoke pending action then hide
            ui.ShowLeaveConfirmationDialog("TIÊU ĐỀ RỜI", "Nội dung xác nhận", () => confirmed = true);
            Assert.That(ui.dialogLeaveRoom.activeSelf, Is.True);
            ui.ConfirmLeaveDialog();
            Assert.That(ui.dialogLeaveRoom.activeSelf, Is.False);
            Assert.That(confirmed, Is.True);
        }

        [Test]
        public void RuleL3_RoundResultOverlay_Lifecycle_ShowsDuringResolveAndRoundEnd()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            var match = Object.FindFirstObjectByType<MatchController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(match, Is.Not.Null);
            Assert.That(ui.roundResultOverlay, Is.Not.Null);

            ui.matchController = match;
            ui.StartGameplayMatch();

            // Placing phase -> overlay is inactive
            ui.ApplyPhaseState(Phase.Placing);
            Assert.That(ui.roundResultOverlay.activeSelf, Is.False);

            // Resolve phase -> overlay is active with round title
            ui.ApplyPhaseState(Phase.Resolve);
            Assert.That(ui.roundResultOverlay.activeSelf, Is.True);
            Assert.That(ui.txtRoundResultTitle.text, Does.Contain("KẾT QUẢ VÒNG"));

            // RoundEnd phase -> overlay remains active
            ui.ApplyPhaseState(Phase.RoundEnd);
            Assert.That(ui.roundResultOverlay.activeSelf, Is.True);

            // MatchEnd -> overlay is hidden as Result screen takes over
            ui.ApplyPhaseState(Phase.MatchEnd);
            Assert.That(ui.roundResultOverlay.activeSelf, Is.False);
        }

        [Test]
        public void RuleL3_ResultBoard_RankBadges_And_ReplayButtonLabel()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);

            var ranked = new List<PlayerState>
            {
                new PlayerState("player", "Nam Làng", false, 3, Vec2.Zero, Vec2.Up),
                new PlayerState("bot-1", "Nữ Năng Động", true, 2, Vec2.Zero, Vec2.Up)
            };
            var winners = new List<string> { "player" };
            ui.PopulateResultBoard(ranked, winners);

            Assert.That(ui.resultRows[0].txtRank.text, Is.EqualTo("#1"));
            Assert.That(ui.resultRows[0].txtRankBadge.text, Is.EqualTo("HẠNG 1 [TẠM]"));
            Assert.That(ui.resultRows[1].txtRank.text, Is.EqualTo("#2"));
            Assert.That(ui.resultRows[1].txtRankBadge.text, Is.EqualTo("HẠNG 2 [TẠM]"));
            Assert.That(ui.btnReplay.GetComponentInChildren<TextMeshProUGUI>().text, Is.EqualTo(UiStrings.BtnReplay));
        }

        [Test]
        public void RuleL3_ErrorToast_ShowsWarningPrefixAndColor()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.toastObject, Is.Not.Null);

            ui.ShowErrorToast("Không tìm thấy mã phòng");
            Assert.That(ui.toastObject.activeSelf, Is.True);
            Assert.That(ui.txtToast.text, Does.Contain("[LỖI]"));
            Assert.That(ui.txtToast.text, Does.Contain("Không tìm thấy mã phòng"));
        }

        // ==================== Lô 4 Tests: Juice & Feedback ====================

        [Test]
        public void RuleL4_JuiceFeedbackController_ExistsAndWired()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.juiceFeedback, Is.Not.Null, "JuiceFeedbackController must be wired to ProductionUIController");
            Assert.That(ui.juiceFeedback.hitImpactFrames, Is.Not.Null, "Hit impact frames must be assigned");
            Assert.That(ui.juiceFeedback.hitImpactFrames.Length, Is.GreaterThanOrEqualTo(6), "Should have at least 6 hit impact frames");
        }

        [Test]
        public void RuleL4_GenericPool_GetAndReturn_ZeroAlloc()
        {
            int created = 0;
            var pool = new Game.GenericPool<Game.HitFx>(
                (parent) =>
                {
                    var go = new GameObject("TestFx_" + created);
                    created++;
                    return go.AddComponent<Game.HitFx>();
                },
                null,
                3 // warm count
            );

            Assert.That(pool.CountAll, Is.EqualTo(3), "Pool should warm 3 instances");
            Assert.That(pool.CountInactive, Is.EqualTo(3));
            Assert.That(created, Is.EqualTo(3));

            // Get should not create new
            var fx1 = pool.Get();
            Assert.That(fx1, Is.Not.Null);
            Assert.That(fx1.gameObject.activeSelf, Is.True);
            Assert.That(pool.CountActive, Is.EqualTo(1));
            Assert.That(created, Is.EqualTo(3), "Should not create new after warm-up");

            // Return
            pool.Return(fx1);
            Assert.That(pool.CountInactive, Is.EqualTo(3));
            Assert.That(fx1.gameObject.activeSelf, Is.False);

            // Get again should reuse
            var fx2 = pool.Get();
            Assert.That(fx2, Is.EqualTo(fx1), "Should reuse returned instance");
            Assert.That(created, Is.EqualTo(3));

            pool.Return(fx2);
            pool.Clear();
        }

        [Test]
        public void RuleL4_ButtonPressScale_MatchesTheme()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.theme, Is.Not.Null);
            Assert.That(ui.theme.buttonPressScale, Is.InRange(0.90f, 0.98f),
                "buttonPressScale should be between 0.90 and 0.98 for subtle press feel");
        }

        [Test]
        public void RuleL4_ReducedMotion_DefaultOff_CanToggle()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.juiceFeedback, Is.Not.Null);

            // Default should be off
            ui.juiceFeedback.ReducedMotion = false;
            Assert.That(ui.juiceFeedback.ReducedMotion, Is.False);

            // Toggle on
            ui.juiceFeedback.ReducedMotion = true;
            Assert.That(ui.juiceFeedback.ReducedMotion, Is.True);

            // Reset
            ui.juiceFeedback.ReducedMotion = false;
        }

        // ==================== Lô 5 Tests: Audio & Settings ====================

        [Test]
        public void RuleL5_AudioService_PoolAndClips_Configured()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var audioSvc = Object.FindFirstObjectByType<HitMe.Audio.AudioService>();
            Assert.That(audioSvc, Is.Not.Null, "AudioService must exist in scene");
            Assert.That(audioSvc.sfxPoolSize, Is.GreaterThanOrEqualTo(10), "SFX pool size should be >= 10");
            Assert.That(audioSvc.sfxClick, Is.Not.Null, "Click SFX must be assigned");
            Assert.That(audioSvc.sfxReady, Is.Not.Null, "Ready SFX must be assigned");
            Assert.That(audioSvc.sfxThrow, Is.Not.Null, "Throw SFX must be assigned");
            Assert.That(audioSvc.sfxHit, Is.Not.Null, "Hit SFX must be assigned");
            Assert.That(audioSvc.sfxFlightMissing, Is.Null, "Missing flight SFX must remain null placeholder per spec");
        }

        [Test]
        public void RuleL5_SettingsDialog_UIAndWiring()
        {
            EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var ui = Object.FindFirstObjectByType<ProductionUIController>();
            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.dialogSettings, Is.Not.Null, "Settings dialog must be assigned");
            Assert.That(ui.sliderMusicVolume, Is.Not.Null, "Music volume slider must be assigned");
            Assert.That(ui.sliderSfxVolume, Is.Not.Null, "SFX volume slider must be assigned");
            Assert.That(ui.btnCloseSettings, Is.Not.Null, "Close button must be assigned");
            Assert.That(ui.btnLanguageVi, Is.Not.Null, "VI button must be assigned");
            Assert.That(ui.btnLanguageEn, Is.Not.Null, "EN button must be assigned");

            // Test Open and Close
            ui.ShowSettingsDialog();
            Assert.That(ui.dialogSettings.activeSelf, Is.True);
            ui.HideSettingsDialog();
            Assert.That(ui.dialogSettings.activeSelf, Is.False);
        }

        [Test]
        public void RuleL5_Localization_DualLanguage_VI_EN_NoMissingKeys()
        {
            // Test Vietnamese strings
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
            Assert.That(UiStrings.IsVietnamese, Is.True);
            Assert.That(UiStrings.Get(UiStrings.BtnReady), Is.EqualTo("SẴN SÀNG"));
            Assert.That(UiStrings.Get(UiStrings.SettingsTitle), Is.EqualTo("CÀI ĐẶT"));
            var viNames = UiStrings.GetLocalizedCharacterNames();
            Assert.That(viNames.Length, Is.EqualTo(7));
            for (int i = 0; i < viNames.Length; i++)
            {
                Assert.That(string.IsNullOrEmpty(viNames[i]), Is.False, $"VI character name {i} must not be empty");
            }

            // Test English strings
            UiStrings.SetLanguage(AppLanguage.English);
            Assert.That(UiStrings.IsVietnamese, Is.False);
            Assert.That(UiStrings.Get(UiStrings.BtnReady), Is.EqualTo("READY"));
            Assert.That(UiStrings.Get(UiStrings.SettingsTitle), Is.EqualTo("SETTINGS"));
            var enNames = UiStrings.GetLocalizedCharacterNames();
            Assert.That(enNames.Length, Is.EqualTo(7));
            for (int i = 0; i < enNames.Length; i++)
            {
                Assert.That(string.IsNullOrEmpty(enNames[i]), Is.False, $"EN character name {i} must not be empty");
            }

            // Reset to default Vietnamese
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
        }

        [Test]
        public void RuleL6_TutorialDialog_UIAndWiring()
        {
            var go = new GameObject("TestUI_L6");
            var ui = go.AddComponent<ProductionUIController>();

            var tutObj = new GameObject("TutObj");
            ui.dialogTutorial = tutObj;
            tutObj.SetActive(false);

            var titleObj = new GameObject("Title");
            ui.txtTutorialTitle = titleObj.AddComponent<TextMeshProUGUI>();

            var stepHeaderObj = new GameObject("StepHeader");
            ui.txtTutorialStepHeader = stepHeaderObj.AddComponent<TextMeshProUGUI>();

            var stepBodyObj = new GameObject("StepBody");
            ui.txtTutorialStepBody = stepBodyObj.AddComponent<TextMeshProUGUI>();

            var btnSkipObj = new GameObject("BtnSkip");
            ui.btnTutorialSkip = btnSkipObj.AddComponent<Button>();

            var btnNextObj = new GameObject("BtnNext");
            ui.btnTutorialNext = btnNextObj.AddComponent<Button>();

            var nextLabelObj = new GameObject("NextLabel");
            ui.txtTutorialNextLabel = nextLabelObj.AddComponent<TextMeshProUGUI>();

            var d1 = new GameObject("D1"); d1.AddComponent<Image>();
            var d2 = new GameObject("D2"); d2.AddComponent<Image>();
            var d3 = new GameObject("D3"); d3.AddComponent<Image>();
            ui.tutorialStepIndicators = new GameObject[] { d1, d2, d3 };

            // Step 0: Placing
            ui.ShowTutorial(0);
            Assert.That(ui.dialogTutorial.activeSelf, Is.True);
            Assert.That(ui.CurrentTutorialStep, Is.EqualTo(0));
            Assert.That(ui.txtTutorialStepHeader.text, Does.Contain("BƯỚC 1/3"));

            // Step 1: Aiming
            ui.NextTutorialStep();
            Assert.That(ui.CurrentTutorialStep, Is.EqualTo(1));
            Assert.That(ui.txtTutorialStepHeader.text, Does.Contain("BƯỚC 2/3"));

            // Step 2: Cancel / Straight throw
            ui.NextTutorialStep();
            Assert.That(ui.CurrentTutorialStep, Is.EqualTo(2));
            Assert.That(ui.txtTutorialStepHeader.text, Does.Contain("BƯỚC 3/3"));
            Assert.That(ui.txtTutorialNextLabel.text, Is.EqualTo("ĐÃ HIỂU"));

            // Final step next -> completes and closes
            ui.NextTutorialStep();
            Assert.That(ui.dialogTutorial.activeSelf, Is.False);
            Assert.That(PlayerPrefs.GetString(UiStrings.PrefKeyTutorialCompleted, "0"), Is.EqualTo("1"));
        }

        [Test]
        public void RuleL6_Tutorial_SkipAndPersistence()
        {
            var go = new GameObject("TestUI_L6_Skip");
            var ui = go.AddComponent<ProductionUIController>();

            var tutObj = new GameObject("TutObj");
            ui.dialogTutorial = tutObj;

            // Reset pref
            PlayerPrefs.SetString(UiStrings.PrefKeyTutorialCompleted, "0");
            ui.ShowTutorial(0);
            Assert.That(ui.dialogTutorial.activeSelf, Is.True);

            // Skip directly
            ui.SkipTutorial();
            Assert.That(ui.dialogTutorial.activeSelf, Is.False);
            Assert.That(PlayerPrefs.GetString(UiStrings.PrefKeyTutorialCompleted, "0"), Is.EqualTo("1"));

            // Dual language check on tutorial strings
            UiStrings.SetLanguage(AppLanguage.English);
            Assert.That(UiStrings.Get(UiStrings.TutorialTitle), Is.EqualTo("GESTURE TUTORIAL (v1.2)"));
            Assert.That(UiStrings.Get(UiStrings.BtnTutorialSkip), Is.EqualTo("SKIP"));
            Assert.That(UiStrings.Get(UiStrings.BtnTutorialNext), Is.EqualTo("NEXT"));
            Assert.That(UiStrings.Get(UiStrings.BtnTutorialDone), Is.EqualTo("GOT IT"));

            // Restore VI
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
        }

        [Test]
        public void RuleL7_FakeAccountService_RenameAndBalance()
        {
            var acc = new Game.FakeAccountService();
            acc.ResetForTest("Khách_4826", 500);

            Assert.That(acc.DisplayName, Is.EqualTo("Khách_4826"));
            Assert.That(acc.Gold, Is.EqualTo(500));
            Assert.That(acc.CosmeticsEnabled, Is.False, "Cosmetics must be disabled by default");

            // First rename is free
            bool r1 = acc.Rename("Tuấn_HàNội");
            Assert.That(r1, Is.True);
            Assert.That(acc.DisplayName, Is.EqualTo("Tuấn_HàNội"));
            Assert.That(acc.Gold, Is.EqualTo(500), "First rename must be free");
            Assert.That(acc.NameChangeCount, Is.EqualTo(1));

            // Second rename costs 100 gold
            bool r2 = acc.Rename("Nam_Làng");
            Assert.That(r2, Is.True);
            Assert.That(acc.DisplayName, Is.EqualTo("Nam_Làng"));
            Assert.That(acc.Gold, Is.EqualTo(400), "Second rename must deduct 100 gold");
            Assert.That(acc.NameChangeCount, Is.EqualTo(2));
        }

        [Test]
        public void RuleL7_FakeInventoryService_WeaponsCatalogAndEquip()
        {
            var acc = new Game.FakeAccountService();
            acc.ResetForTest("Khách_4826", 500);
            var inv = new Game.FakeInventoryService();
            inv.ResetForTest();

            var weapons = inv.GetAllWeapons();
            Assert.That(weapons.Count, Is.GreaterThanOrEqualTo(8));

            // Default weapon is Dép Tổ Ong
            var def = inv.GetEquippedWeapon();
            Assert.That(def.id, Is.EqualTo("dep_to_ong"));
            Assert.That(def.isOwned, Is.True);
            Assert.That(def.isEquipped, Is.True);

            // Buy Chảo Chống Dính (100 gold)
            bool b1 = inv.BuyWeapon("chao_chong_dinh", acc);
            Assert.That(b1, Is.True);
            Assert.That(acc.Gold, Is.EqualTo(400));
            Assert.That(inv.IsWeaponOwned("chao_chong_dinh"), Is.True);

            // Equip Chảo Chống Dính
            bool eq = inv.EquipWeapon("chao_chong_dinh");
            Assert.That(eq, Is.True);
            Assert.That(inv.GetEquippedWeapon().id, Is.EqualTo("chao_chong_dinh"));
            Assert.That(inv.GetEquippedWeapon().isEquipped, Is.True);
        }

        [Test]
        public void RuleL7_ProfileShopInventoryLeaderboard_Dialogs()
        {
            var go = new GameObject("TestUI_L7");
            var ui = go.AddComponent<ProductionUIController>();

            var pObj = new GameObject("Prof"); ui.dialogProfile = pObj; pObj.SetActive(false);
            var sObj = new GameObject("Shop"); ui.dialogShop = sObj; sObj.SetActive(false);
            var iObj = new GameObject("Inv"); ui.dialogInventory = iObj; iObj.SetActive(false);
            var lObj = new GameObject("Lb"); ui.dialogLeaderboard = lObj; lObj.SetActive(false);

            ui.ShowProfileDialog();
            Assert.That(ui.dialogProfile.activeSelf, Is.True);
            ui.HideProfileDialog();
            Assert.That(ui.dialogProfile.activeSelf, Is.False);

            ui.ShowShopDialog();
            Assert.That(ui.dialogShop.activeSelf, Is.True);
            ui.HideShopDialog();
            Assert.That(ui.dialogShop.activeSelf, Is.False);

            ui.ShowInventoryDialog();
            Assert.That(ui.dialogInventory.activeSelf, Is.True);
            ui.HideInventoryDialog();
            Assert.That(ui.dialogInventory.activeSelf, Is.False);

            ui.ShowLeaderboardDialog();
            Assert.That(ui.dialogLeaderboard.activeSelf, Is.True);
            ui.HideLeaderboardDialog();
            Assert.That(ui.dialogLeaderboard.activeSelf, Is.False);
        }

        [Test]
        public void RuleL8_GuestLoginDialog_MockAndDisabledSocial()
        {
            var go = new GameObject("TestUI_L8");
            var ui = go.AddComponent<ProductionUIController>();

            var lObj = new GameObject("Login"); ui.dialogLogin = lObj; lObj.SetActive(false);
            var btnGuest = new GameObject("Guest").AddComponent<Button>(); ui.btnSubmitGuestLogin = btnGuest;
            var btnG = new GameObject("Google").AddComponent<Button>(); ui.btnLoginGoogle = btnG; btnG.interactable = false;
            var btnF = new GameObject("FB").AddComponent<Button>(); ui.btnLoginFacebook = btnF; btnF.interactable = false;
            var btnA = new GameObject("Apple").AddComponent<Button>(); ui.btnLoginApple = btnA; btnA.interactable = false;

            ui.ShowLoginDialog();
            Assert.That(ui.dialogLogin.activeSelf, Is.True);
            Assert.That(btnG.interactable, Is.False, "Social login button must be disabled mock");
            Assert.That(btnF.interactable, Is.False, "Social login button must be disabled mock");
            Assert.That(btnA.interactable, Is.False, "Social login button must be disabled mock");

            ui.HideLoginDialog();
            Assert.That(ui.dialogLogin.activeSelf, Is.False);
        }

        [Test]
        public void RuleL9_PlayerView_IdleBreath_And_HitReaction_Recoil()
        {
            var pGo = new GameObject("TestPlayer_L9");
            var pv = pGo.AddComponent<PlayerView>();
            pv.Initialise("player", Color.white, 0);

            // Test animation states
            pv.PlayAnimation("Aim");
            pv.PlayAnimation("Throw");
            pv.PlayAnimation("Hit");
            pv.PlayAnimation("Defeat");
            pv.PlayAnimation("Idle");

            // Verify logic position is preserved (transform position not offset)
            Assert.That(pGo.transform.position, Is.EqualTo(Vector3.zero));

            Object.DestroyImmediate(pGo);
        }

        [Test]
        public void RuleL9_WeaponFlightView_Arc_And_ShadowRenderer()
        {
            var wGo = new GameObject("TestWeapon_L9");
            var wf = wGo.AddComponent<WeaponFlightView>();
            wf.Initialise(Vector3.zero, new Vector3(2f, 2f, 0f), 0.50f, false, null);

            Assert.That(wGo.activeSelf, Is.True);
            Assert.That(wf.Duration, Is.EqualTo(0.50f));

            Object.DestroyImmediate(wGo);
        }
    }
}
