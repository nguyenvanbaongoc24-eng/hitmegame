from pathlib import Path

SETUP_PATH = Path('unity-client/Assets/Editor/ProductionUISetup.cs')
with open(SETUP_PATH, 'rb') as f:
    raw = f.read().decode('utf-8')

# Normalize to LF
is_crlf = '\r\n' in raw
code = raw.replace('\r\n', '\n')

# 1. Add ArenaBackdrop after line 92
old_arena = """                sr.sprite = spriteArena;
                sr.sortingOrder = -2;
            }"""
new_arena = """                sr.sprite = spriteArena;
                sr.sortingOrder = -2;

                // A2: Fullscreen Village Court Backdrop (covers full camera frustum so no blank/solid bands above or below the gate)
                var backdropObj = GameObject.Find("ArenaBackdrop");
                if (backdropObj == null) backdropObj = new GameObject("ArenaBackdrop");
                backdropObj.transform.SetParent(arenaGo.transform);
                backdropObj.transform.position = new Vector3(0f, 0.80f, 0f);
                backdropObj.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
                var bsr = backdropObj.GetComponent<SpriteRenderer>();
                if (bsr == null) bsr = backdropObj.AddComponent<SpriteRenderer>();
                bsr.sprite = spriteBgHome ?? spriteArena;
                bsr.sortingOrder = -10;
                bsr.color = new Color(0.75f, 0.75f, 0.80f, 1f);
            }"""

if old_arena in code:
    code = code.replace(old_arena, new_arena)
    print("Added ArenaBackdrop.")
else:
    print("WARNING: old_arena not found")

# 2. Expand HomeBgVillage offsets
old_bg_offsets = """                bgRt.offsetMin = new Vector2(-60f, -120f);
                bgRt.offsetMax = new Vector2(60f, 120f);"""
new_bg_offsets = """                bgRt.offsetMin = new Vector2(-150f, -300f);
                bgRt.offsetMax = new Vector2(150f, 300f);"""

if old_bg_offsets in code:
    code = code.replace(old_bg_offsets, new_bg_offsets)
    print("Expanded HomeBgVillage offsets.")
else:
    print("WARNING: old_bg_offsets not found")

# 3. Top bar icons (Coin, Gem, Gear)
old_top_bar = """            var goldPill = CreateImage(homePanel.transform, "GoldPill", spriteBannerWood, new Vector2(0.58f, 0.960f), new Vector2(0.58f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(190f, 56f));
            var txtGold = CreateTMP(goldPill.transform, "Text", fontAsset, "1.250 +", 20, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtHomeGold = txtGold;

            // Hidden or compact gem pill
            var gemPill = CreateImage(homePanel.transform, "GemPill", spriteBannerWood, new Vector2(0.76f, 0.960f), new Vector2(0.76f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(110f, 56f));
            var txtGem = CreateTMP(gemPill.transform, "Text", fontAsset, "50", 18, new Color(0.40f, 0.85f, 1f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtHomeGems = txtGem;

            // Settings button on Home Screen (Top Right)
            var btnHomeSettings = CreateButton(homePanel.transform, "BtnHomeSettings", spriteCircleBack, new Vector2(0.92f, 0.960f), new Vector2(0.92f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(72f, 72f));
            CreateTMP(btnHomeSettings.transform, "Label", fontAsset, "", 34, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnHomeSettings = btnHomeSettings.GetComponent<Button>();"""

new_top_bar = """            var goldPill = CreateImage(homePanel.transform, "GoldPill", spriteBannerWood, new Vector2(0.58f, 0.960f), new Vector2(0.58f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(190f, 56f));
            if (spriteIconCoin != null)
            {
                var iconGo = CreateImage(goldPill.transform, "CoinIcon", spriteIconCoin, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(36f, 36f));
                iconGo.GetComponent<Image>().preserveAspect = true;
            }
            var txtGold = CreateTMP(goldPill.transform, "Text", fontAsset, "1.250 +", 20, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.60f, 0.50f), new Vector2(110f, 40f), wrap: false);
            uiCtrl.txtHomeGold = txtGold;

            // Hidden or compact gem pill
            var gemPill = CreateImage(homePanel.transform, "GemPill", spriteBannerWood, new Vector2(0.76f, 0.960f), new Vector2(0.76f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(110f, 56f));
            if (spriteIconGem != null)
            {
                var iconGemGo = CreateImage(gemPill.transform, "GemIcon", spriteIconGem, new Vector2(0.24f, 0.5f), new Vector2(0.24f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(32f, 32f));
                iconGemGo.GetComponent<Image>().preserveAspect = true;
            }
            var txtGem = CreateTMP(gemPill.transform, "Text", fontAsset, "50", 18, new Color(0.40f, 0.85f, 1f), TextAlignmentOptions.Center, new Vector2(0.65f, 0.50f), new Vector2(60f, 40f), wrap: false);
            uiCtrl.txtHomeGems = txtGem;

            // Settings button on Home Screen (Top Right)
            var btnHomeSettings = CreateButton(homePanel.transform, "BtnHomeSettings", spriteCircleBack, new Vector2(0.92f, 0.960f), new Vector2(0.92f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(72f, 72f));
            if (spriteIconGear != null)
            {
                var iconGearGo = CreateImage(btnHomeSettings.transform, "GearIcon", spriteIconGear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 40f));
                iconGearGo.GetComponent<Image>().preserveAspect = true;
            }
            uiCtrl.btnHomeSettings = btnHomeSettings.GetComponent<Button>();"""

if old_top_bar in code:
    code = code.replace(old_top_bar, new_top_bar)
    print("Updated top bar with coin, gem, gear icons.")
else:
    print("WARNING: old_top_bar not found")

# 4. In-game settings button icon
old_ingame_settings = """            // In-Game Settings Button (Lô 5: Overlay access during gameplay)
            var btnInGameSettings = CreateButton(hudPanel.transform, "BtnInGameSettings", spriteCircleBack, new Vector2(0.95f, 0.968f), new Vector2(0.95f, 0.968f), new Vector2(0.5f, 0.5f), new Vector2(72f, 72f));
            CreateTMP(btnInGameSettings.transform, "Label", fontAsset, "", 28, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnInGameSettings = btnInGameSettings.GetComponent<Button>();"""

new_ingame_settings = """            // In-Game Settings Button (Lô 5: Overlay access during gameplay)
            var btnInGameSettings = CreateButton(hudPanel.transform, "BtnInGameSettings", spriteCircleBack, new Vector2(0.95f, 0.968f), new Vector2(0.95f, 0.968f), new Vector2(0.5f, 0.5f), new Vector2(72f, 72f));
            if (spriteIconGear != null)
            {
                var iconGearGo = CreateImage(btnInGameSettings.transform, "GearIcon", spriteIconGear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(36f, 36f));
                iconGearGo.GetComponent<Image>().preserveAspect = true;
            }
            uiCtrl.btnInGameSettings = btnInGameSettings.GetComponent<Button>();"""

if old_ingame_settings in code:
    code = code.replace(old_ingame_settings, new_ingame_settings)
    print("Updated in-game settings button with gear icon.")
else:
    print("WARNING: old_ingame_settings not found")

# 5. Bottom nav bar: Clean active home tab, hide postponed tabs
old_bottom_nav = """            var btnHomeTab = CreateButton(navBarBg.transform, "BtnTabHome", null, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(240f, 65f));
            var txtHomeTab = CreateTMP(btnHomeTab.transform, "Label", fontAsset, "TRANG CHỦ", 20, new Color(0.00f, 0.898f, 1f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnHomeLogin = btnHomeTab.GetComponent<Button>();

            var btnLbBottom = CreateButton(navBarBg.transform, "BtnBottomLb", null, new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(240f, 65f));
            CreateTMP(btnLbBottom.transform, "Label", fontAsset, "XẾP HẠNG", 20, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnHomeLeaderboard = btnLbBottom.GetComponent<Button>();

            var btnShopBottom = CreateButton(navBarBg.transform, "BtnBottomShop", null, new Vector2(0.80f, 0.50f), new Vector2(0.80f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(240f, 65f));
            CreateTMP(btnShopBottom.transform, "Label", fontAsset, "NHIỆM VỤ", 20, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnHomeShop = btnShopBottom.GetComponent<Button>();

            var btnInvBottom = CreateButton(homePanel.transform, "BtnBottomInv", null, new Vector2(-10f, -10f), new Vector2(-10f, -10f), new Vector2(0.5f, 0.5f), Vector2.one);
            uiCtrl.btnHomeInventory = btnInvBottom.GetComponent<Button>();"""

new_bottom_nav = """            var btnHomeTab = CreateButton(navBarBg.transform, "BtnTabHome", null, new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(380f, 65f));
            var txtHomeTab = CreateTMP(btnHomeTab.transform, "Label", fontAsset, "TRANG CHỦ", 24, new Color(0.00f, 0.898f, 1f), TextAlignmentOptions.Center, wrap: false);
            txtHomeTab.fontStyle = FontStyles.Bold;
            uiCtrl.btnHomeLogin = btnHomeTab.GetComponent<Button>();

            // Beta 0.1: Postponed tabs (Xếp hạng, Nhiệm vụ, Túi đồ) are cleanly hidden without invisible hit targets
            var btnLbBottom = CreateButton(navBarBg.transform, "BtnBottomLb", null, new Vector2(-20f, -20f), new Vector2(-20f, -20f), new Vector2(0.5f, 0.5f), Vector2.one);
            uiCtrl.btnHomeLeaderboard = btnLbBottom.GetComponent<Button>();
            btnLbBottom.gameObject.SetActive(false);

            var btnShopBottom = CreateButton(navBarBg.transform, "BtnBottomShop", null, new Vector2(-20f, -20f), new Vector2(-20f, -20f), new Vector2(0.5f, 0.5f), Vector2.one);
            uiCtrl.btnHomeShop = btnShopBottom.GetComponent<Button>();
            btnShopBottom.gameObject.SetActive(false);

            var btnInvBottom = CreateButton(homePanel.transform, "BtnBottomInv", null, new Vector2(-20f, -20f), new Vector2(-20f, -20f), new Vector2(0.5f, 0.5f), Vector2.one);
            uiCtrl.btnHomeInventory = btnInvBottom.GetComponent<Button>();
            btnInvBottom.gameObject.SetActive(false);"""

if old_bottom_nav in code:
    code = code.replace(old_bottom_nav, new_bottom_nav)
    print("Updated bottom nav bar.")
else:
    print("WARNING: old_bottom_nav not found")

if is_crlf:
    code = code.replace('\n', '\r\n')

with open(SETUP_PATH, 'wb') as f:
    f.write(code.encode('utf-8'))

print("Completed ProductionUISetup.cs update.")
