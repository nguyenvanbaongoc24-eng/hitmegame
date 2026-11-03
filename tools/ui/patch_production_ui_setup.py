import sys
from pathlib import Path

path = Path('unity-client/Assets/Editor/ProductionUISetup.cs')
raw = path.read_text(encoding='utf-8')
code = raw.replace('\r\n', '\n')

# 1. Add new sprite variables in BuildProductionUI()
old_sprites = """            var spriteIconCheck = LoadSprite(KitPath + "ui_icon_check.png");"""

new_sprites = """            var spriteIconCheck = LoadSprite(KitPath + "ui_icon_check.png");

            // 3D Visual UI Kit & Badges
            var spriteLogoHitMe = LoadSprite(KitPath + "logo_hit_me.png");
            var spriteBtnPrimaryYellow = LoadSprite(KitPath + "btn_primary_yellow_normal.png") ?? spriteBtnYellow;
            var spriteBtnBlue3D = LoadSprite(KitPath + "btn_blue_normal.png") ?? spriteBtnBlue;
            var spriteBtnGreen3D = LoadSprite(KitPath + "btn_green_normal.png") ?? spriteBtnGreen;
            var spriteBtnNavy3D = LoadSprite(KitPath + "btn_navy_normal.png") ?? spriteBannerWood;
            var spriteDockBg = LoadSprite(KitPath + "dock_bg.png");
            var spritePanelCardDarkGold = LoadSprite(KitPath + "panel_card_dark_gold.png") ?? spritePanelDarkGold;
            var spritePillCoin = LoadSprite(KitPath + "pill_coin.png") ?? spriteBannerWood;
            var spriteCircleNavy = LoadSprite(KitPath + "btn_circle_navy.png") ?? spriteCircleBack;
            var spriteCircleGold = LoadSprite(KitPath + "btn_circle_gold.png") ?? spriteCircleBack;
            var spriteAvatarRing = LoadSprite(KitPath + "avatar_ring_cyan.png");
            var spritePortraitNamLang = LoadSprite(KitPath + "portrait_namlang.png");
            var spriteIconPlay = LoadSprite(KitPath + "ui_icon_play.png");
            var spriteIconPersonAdd = LoadSprite(KitPath + "ui_icon_person_add.png");
            var spriteIconPeople = LoadSprite(KitPath + "ui_icon_people.png");
            var spriteIconBot = LoadSprite(KitPath + "ui_icon_bot.png");
            var spriteIconHome = LoadSprite(KitPath + "ui_icon_home.png");
            var spriteIconTrophy = LoadSprite(KitPath + "ui_icon_trophy.png");
            var spriteIconGift = LoadSprite(KitPath + "ui_icon_gift.png");
            var spriteIconChevronLeft = LoadSprite(KitPath + "ui_icon_chevron_left.png");
            var spriteIconChevronRight = LoadSprite(KitPath + "ui_icon_chevron_right.png");
            var spriteIconWeaponDep = LoadSprite(KitPath + "ui_icon_weapon_dep.png") ?? spriteWeaponDep;"""

if old_sprites in code:
    code = code.replace(old_sprites, new_sprites)
    print("Added 3D UI Kit sprite variables.")
else:
    print("WARNING: old_sprites not found")

# 2. Update ArenaBackdrop scale to 2.8f
old_backdrop = """                backdropObj.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
                var bsr = backdropObj.GetComponent<SpriteRenderer>();
                if (bsr == null) bsr = backdropObj.AddComponent<SpriteRenderer>();
                bsr.sprite = spriteBgHome ?? spriteArena;
                bsr.sortingOrder = -10;
                bsr.color = new Color(0.75f, 0.75f, 0.80f, 1f);"""

new_backdrop = """                backdropObj.transform.localScale = new Vector3(2.8f, 2.8f, 1f);
                var bsr = backdropObj.GetComponent<SpriteRenderer>();
                if (bsr == null) bsr = backdropObj.AddComponent<SpriteRenderer>();
                bsr.sprite = spriteBgHome ?? spriteArena;
                bsr.sortingOrder = -10;
                bsr.color = Color.white;"""

if old_backdrop in code:
    code = code.replace(old_backdrop, new_backdrop)
    print("Updated ArenaBackdrop scale and color.")
else:
    print("WARNING: old_backdrop not found")

# 3. Update Top Bar on Home Panel (Profile header, Gold pill, Settings button)
old_top_bar = """            // Top Bar: Player profile, Currency Chips, and Settings (Safe Area row Y = 0.960f)
            var btnProfHeader = CreateButton(homePanel.transform, "BtnHomeProfileHeader", spriteCardSelected, new Vector2(0.18f, 0.960f), new Vector2(0.18f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(250f, 68f));
            var txtProfHeader = CreateTMP(btnProfHeader.transform, "Name", fontAsset, "Nam Làng\\n<size=16><color=#aaccff>Hồ sơ</color></size>", 20, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnHomeProfile = btnProfHeader.GetComponent<Button>();
            uiCtrl.txtHomePlayerName = txtProfHeader;

            var goldPill = CreateImage(homePanel.transform, "GoldPill", spriteBannerWood, new Vector2(0.58f, 0.960f), new Vector2(0.58f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(190f, 56f));
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

new_top_bar = """            // Top Bar: Player profile (Avatar + Ring), Currency Chips, and Settings (Safe Area row Y = 0.960f)
            var btnProfHeader = CreateButton(homePanel.transform, "BtnHomeProfileHeader", spriteCardSelected, new Vector2(0.18f, 0.960f), new Vector2(0.18f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(250f, 72f));
            if (spriteAvatarRing != null)
            {
                var ringGo = CreateImage(btnProfHeader.transform, "AvatarRing", spriteAvatarRing, new Vector2(0.16f, 0.50f), new Vector2(0.16f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(58f, 58f));
                ringGo.GetComponent<Image>().preserveAspect = true;
                if (spritePortraitNamLang != null)
                {
                    var headGo = CreateImage(ringGo.transform, "AvatarHead", spritePortraitNamLang, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(46f, 46f));
                    headGo.GetComponent<Image>().preserveAspect = true;
                }
            }
            var txtProfHeader = CreateTMP(btnProfHeader.transform, "Name", fontAsset, "Nam Làng\\n<size=15><color=#aaccff>Hồ sơ</color></size>", 20, colorGoldSub, TextAlignmentOptions.Left, new Vector2(0.64f, 0.50f), new Vector2(140f, 48f), wrap: false);
            uiCtrl.btnHomeProfile = btnProfHeader.GetComponent<Button>();
            uiCtrl.txtHomePlayerName = txtProfHeader;

            var goldPill = CreateImage(homePanel.transform, "GoldPill", spritePillCoin, new Vector2(0.58f, 0.960f), new Vector2(0.58f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(190f, 56f));
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
            var btnHomeSettings = CreateButton(homePanel.transform, "BtnHomeSettings", spriteCircleNavy, new Vector2(0.92f, 0.960f), new Vector2(0.92f, 0.960f), new Vector2(0.50f, 0.50f), new Vector2(72f, 72f));
            if (spriteIconGear != null)
            {
                var iconGearGo = CreateImage(btnHomeSettings.transform, "GearIcon", spriteIconGear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 40f));
                iconGearGo.GetComponent<Image>().preserveAspect = true;
            }
            uiCtrl.btnHomeSettings = btnHomeSettings.GetComponent<Button>();"""

if old_top_bar in code:
    code = code.replace(old_top_bar, new_top_bar)
    print("Updated Top Bar with avatar ring & icons.")
else:
    print("WARNING: old_top_bar not found")

# 4. Update Home Header with 3D logo
old_header = """            // Home Header & Logo (Centered below top bar, Y = 0.840f)
            var homeHeader = CreateImage(homePanel.transform, "HomeBanner", spriteBannerWood, new Vector2(0.5f, 0.840f), new Vector2(0.5f, 0.840f), new Vector2(0.5f, 0.5f), new Vector2(720f, 130f));
            var logoTxt = CreateTMP(homeHeader.transform, "LogoTxt", fontAsset, UiStrings.GameTitle, 68, colorGoldSub, TextAlignmentOptions.Center);
            logoTxt.fontStyle = FontStyles.Bold;
            FillRect(logoTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            uiCtrl.txtHomeLogo = logoTxt;"""

new_header = """            // Home Header & 3D Logo (Centered below top bar, Y = 0.835f)
            var headerSprite = spriteLogoHitMe != null ? spriteLogoHitMe : spriteBannerWood;
            var homeHeader = CreateImage(homePanel.transform, "HomeBanner", headerSprite, new Vector2(0.5f, 0.835f), new Vector2(0.5f, 0.835f), new Vector2(0.5f, 0.5f), new Vector2(620f, 294f));
            homeHeader.GetComponent<Image>().preserveAspect = true;
            var logoTxt = CreateTMP(homeHeader.transform, "LogoTxt", fontAsset, spriteLogoHitMe != null ? "" : UiStrings.GameTitle, 68, colorGoldSub, TextAlignmentOptions.Center);
            logoTxt.fontStyle = FontStyles.Bold;
            FillRect(logoTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            uiCtrl.txtHomeLogo = logoTxt;"""

if old_header in code:
    code = code.replace(old_header, new_header)
    print("Updated Home Header with 3D Logo.")
else:
    print("WARNING: old_header not found")

# 5. Update Navigation arrows and Loadout cards
old_arrows = """            // Hero Navigation Arrows (? and ?)
            var btnPrevHero = CreateButton(homePanel.transform, "BtnHeroPrev", spriteCircleBack, new Vector2(0.20f, 0.620f), new Vector2(0.20f, 0.620f), new Vector2(0.5f, 0.5f), new Vector2(72f, 72f));
            CreateTMP(btnPrevHero.transform, "Label", fontAsset, "<", 32, colorGoldSub, TextAlignmentOptions.Center);
            btnPrevHero.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ChangePracticeCharacter(-1);
            });

            var btnNextHero = CreateButton(homePanel.transform, "BtnHeroNext", spriteCircleBack, new Vector2(0.80f, 0.620f), new Vector2(0.80f, 0.620f), new Vector2(0.5f, 0.5f), new Vector2(72f, 72f));
            CreateTMP(btnNextHero.transform, "Label", fontAsset, ">", 32, colorGoldSub, TextAlignmentOptions.Center);
            btnNextHero.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ChangePracticeCharacter(1);
            });

            // 2 Loadout Selector Cards side by side (Y = 0.420f)
            // Left Card: Character Selector
            var charCard = CreatePanel(homePanel.transform, "CharCard", new Vector2(0.285f, 0.420f), new Vector2(0.285f, 0.420f), new Vector2(0.5f, 0.5f), new Vector2(430f, 96f), spritePanelDarkGold, Color.white);
            var charCardBtn = charCard.AddComponent<Button>();
            charCardBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); uiCtrl.ChangePracticeCharacter(1); });
            var charIcon = CreateImage(charCard.transform, "Icon", configAsset?.characterIdleSprites?[0] ?? LoadSprite(SpritesPath + "cau_be_lang_idle.png"), new Vector2(0.15f, 0.5f), new Vector2(0.15f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(68f, 68f));
            charIcon.GetComponent<Image>().preserveAspect = true;
            CreateTMP(charCard.transform, "Title", fontAsset, "NHÂN VẬT", 18, colorGoldSub, TextAlignmentOptions.Left, new Vector2(0.60f, 0.68f), new Vector2(250f, 26f));
            var charNameTxt = CreateTMP(charCard.transform, "CharName", fontAsset, "Nam Làng", 26, colorCream, TextAlignmentOptions.Left, new Vector2(0.60f, 0.32f), new Vector2(250f, 34f));
            charNameTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtHomeCharName = charNameTxt;

            // Right Card: Weapon Selector
            var weaponCard = CreatePanel(homePanel.transform, "WeaponCard", new Vector2(0.715f, 0.420f), new Vector2(0.715f, 0.420f), new Vector2(0.5f, 0.5f), new Vector2(430f, 96f), spritePanelDarkGold, Color.white);
            var weaponIconHome = CreateImage(weaponCard.transform, "Icon", spriteWeaponDep, new Vector2(0.15f, 0.5f), new Vector2(0.15f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(64f, 64f));
            weaponIconHome.GetComponent<Image>().preserveAspect = true;
            CreateTMP(weaponCard.transform, "Title", fontAsset, "VŨ KHÍ", 18, colorGoldSub, TextAlignmentOptions.Left, new Vector2(0.60f, 0.68f), new Vector2(250f, 26f));
            var weaponTxtHome = CreateTMP(weaponCard.transform, "Text", fontAsset, "Dép Tổ Ong", 26, colorCream, TextAlignmentOptions.Left, new Vector2(0.60f, 0.32f), new Vector2(250f, 34f));
            weaponTxtHome.fontStyle = FontStyles.Bold;"""

new_arrows = """            // Hero Navigation Arrows (3D Navy Circles + Chevron Icons)
            var btnPrevHero = CreateButton(homePanel.transform, "BtnHeroPrev", spriteCircleNavy, new Vector2(0.18f, 0.620f), new Vector2(0.18f, 0.620f), new Vector2(0.5f, 0.5f), new Vector2(76f, 76f));
            if (spriteIconChevronLeft != null)
            {
                var iconPrev = CreateImage(btnPrevHero.transform, "Icon", spriteIconChevronLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 40f));
                iconPrev.GetComponent<Image>().preserveAspect = true;
            }
            btnPrevHero.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ChangePracticeCharacter(-1);
            });

            var btnNextHero = CreateButton(homePanel.transform, "BtnHeroNext", spriteCircleNavy, new Vector2(0.82f, 0.620f), new Vector2(0.82f, 0.620f), new Vector2(0.5f, 0.5f), new Vector2(76f, 76f));
            if (spriteIconChevronRight != null)
            {
                var iconNext = CreateImage(btnNextHero.transform, "Icon", spriteIconChevronRight, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 40f));
                iconNext.GetComponent<Image>().preserveAspect = true;
            }
            btnNextHero.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ChangePracticeCharacter(1);
            });

            // 2 Loadout Selector Cards side by side (Y = 0.420f)
            // Left Card: Character Selector
            var charCard = CreatePanel(homePanel.transform, "CharCard", new Vector2(0.285f, 0.420f), new Vector2(0.285f, 0.420f), new Vector2(0.5f, 0.5f), new Vector2(430f, 100f), spritePanelCardDarkGold, Color.white);
            var charCardBtn = charCard.AddComponent<Button>();
            charCardBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); uiCtrl.ChangePracticeCharacter(1); });
            var charIcon = CreateImage(charCard.transform, "Icon", spritePortraitNamLang ?? configAsset?.characterIdleSprites?[0] ?? LoadSprite(SpritesPath + "cau_be_lang_idle.png"), new Vector2(0.16f, 0.5f), new Vector2(0.16f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(68f, 68f));
            charIcon.GetComponent<Image>().preserveAspect = true;
            CreateTMP(charCard.transform, "Title", fontAsset, "NHÂN VẬT", 18, colorGoldSub, TextAlignmentOptions.Left, new Vector2(0.60f, 0.68f), new Vector2(250f, 26f));
            var charNameTxt = CreateTMP(charCard.transform, "CharName", fontAsset, "Nam Làng", 26, colorCream, TextAlignmentOptions.Left, new Vector2(0.60f, 0.32f), new Vector2(250f, 34f));
            charNameTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtHomeCharName = charNameTxt;

            // Right Card: Weapon Selector
            var weaponCard = CreatePanel(homePanel.transform, "WeaponCard", new Vector2(0.715f, 0.420f), new Vector2(0.715f, 0.420f), new Vector2(0.5f, 0.5f), new Vector2(430f, 100f), spritePanelCardDarkGold, Color.white);
            var weaponIconHome = CreateImage(weaponCard.transform, "Icon", spriteIconWeaponDep, new Vector2(0.16f, 0.5f), new Vector2(0.16f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(68f, 68f));
            weaponIconHome.GetComponent<Image>().preserveAspect = true;
            CreateTMP(weaponCard.transform, "Title", fontAsset, "VŨ KHÍ", 18, colorGoldSub, TextAlignmentOptions.Left, new Vector2(0.60f, 0.68f), new Vector2(250f, 26f));
            var weaponTxtHome = CreateTMP(weaponCard.transform, "Text", fontAsset, "Dép Tổ Ong", 26, colorCream, TextAlignmentOptions.Left, new Vector2(0.60f, 0.32f), new Vector2(250f, 34f));
            weaponTxtHome.fontStyle = FontStyles.Bold;"""

if old_arrows in code:
    code = code.replace(old_arrows, new_arrows)
    print("Updated Navigation arrows and Loadout cards.")
else:
    print("WARNING: old_arrows not found")

# 6. Update Action Buttons with 3D skins, icons and clean UTF-8 text
old_actions = """            // Action Buttons
            // 1. CH?I NHANH (Primary CTA V?ng - Y = 0.305f)
            var btnQuick = CreateButton(homePanel.transform, "BtnQuickMatch", spriteBtnYellow, new Vector2(0.5f, 0.305f), new Vector2(0.5f, 0.305f), new Vector2(0.5f, 0.5f), new Vector2(880f, 115f));
            var btnQuickTxt = CreateTMP(btnQuick.transform, "Label", fontAsset, UiStrings.BtnPlayNow, 40, new Color(0.24f, 0.12f, 0.02f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.60f), new Vector2(840f, 50f));
            btnQuickTxt.fontStyle = FontStyles.Bold;
            var btnQuickSub = CreateTMP(btnQuick.transform, "Sub", fontAsset, "Online ? T?i ?a 6 ng??i", 20, new Color(0.38f, 0.22f, 0.05f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.26f), new Vector2(840f, 30f));
            uiCtrl.btnQuickMatch = btnQuick.GetComponent<Button>();

            // 2. T?O PH?NG (Xanh d??ng) + V?O PH?NG (Xanh l?) - Row Y = 0.205f
            var btnCreate = CreateButton(homePanel.transform, "BtnCreateRoom", spriteBtnBlue, new Vector2(0.285f, 0.205f), new Vector2(0.285f, 0.205f), new Vector2(0.5f, 0.5f), new Vector2(430f, 95f));
            var btnCreateTxt = CreateTMP(btnCreate.transform, "Label", fontAsset, "??+ " + UiStrings.BtnCreateRoom, 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            btnCreateTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCreateRoom = btnCreate.GetComponent<Button>();

            var btnJoin = CreateButton(homePanel.transform, "BtnJoinRoom", spriteBtnGreen, new Vector2(0.715f, 0.205f), new Vector2(0.715f, 0.205f), new Vector2(0.5f, 0.5f), new Vector2(430f, 95f));
            var btnJoinTxt = CreateTMP(btnJoin.transform, "Label", fontAsset, UiStrings.BtnJoinRoom, 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            btnJoinTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnJoinRoom = btnJoin.GetComponent<Button>();

            // 3. LUY?N V?I BOT (Dark Blue/Wood - Y = 0.115f)
            var btnBot = CreateButton(homePanel.transform, "BtnBotTraining", spriteBannerWood, new Vector2(0.5f, 0.115f), new Vector2(0.5f, 0.115f), new Vector2(0.5f, 0.5f), new Vector2(880f, 75f));
            var btnBotTxt = CreateTMP(btnBot.transform, "Label", fontAsset, UiStrings.BtnPracticeBots, 30, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            btnBotTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnBotTraining = btnBot.GetComponent<Button>();"""

new_actions = """            // Action Buttons
            // 1. CHƠI NHANH (Primary CTA Vàng 3D - Y = 0.305f)
            var btnQuick = CreateButton(homePanel.transform, "BtnQuickMatch", spriteBtnPrimaryYellow, new Vector2(0.5f, 0.305f), new Vector2(0.5f, 0.305f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            if (spriteIconPlay != null)
            {
                var iconPlay = CreateImage(btnQuick.transform, "PlayIcon", spriteIconPlay, new Vector2(0.12f, 0.50f), new Vector2(0.12f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(52f, 52f));
                iconPlay.GetComponent<Image>().preserveAspect = true;
            }
            var btnQuickTxt = CreateTMP(btnQuick.transform, "Label", fontAsset, "CHƠI NHANH", 42, new Color(0.24f, 0.12f, 0.02f), TextAlignmentOptions.Center, new Vector2(0.54f, 0.62f), new Vector2(740f, 50f));
            btnQuickTxt.fontStyle = FontStyles.Bold;
            var btnQuickSub = CreateTMP(btnQuick.transform, "Sub", fontAsset, "Online • Tối đa 6 người", 20, new Color(0.38f, 0.22f, 0.05f), TextAlignmentOptions.Center, new Vector2(0.54f, 0.28f), new Vector2(740f, 32f));
            uiCtrl.btnQuickMatch = btnQuick.GetComponent<Button>();

            // 2. TẠO PHÒNG (3D Blue) + VÀO PHÒNG (3D Green) - Row Y = 0.205f
            var btnCreate = CreateButton(homePanel.transform, "BtnCreateRoom", spriteBtnBlue3D, new Vector2(0.285f, 0.205f), new Vector2(0.285f, 0.205f), new Vector2(0.5f, 0.5f), new Vector2(430f, 105f));
            if (spriteIconPersonAdd != null)
            {
                var iconCreate = CreateImage(btnCreate.transform, "Icon", spriteIconPersonAdd, new Vector2(0.16f, 0.50f), new Vector2(0.16f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f));
                iconCreate.GetComponent<Image>().preserveAspect = true;
            }
            var btnCreateTxt = CreateTMP(btnCreate.transform, "Label", fontAsset, "TẠO PHÒNG", 32, Color.white, TextAlignmentOptions.Center, new Vector2(0.58f, 0.50f), new Vector2(270f, 50f), wrap: false);
            btnCreateTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCreateRoom = btnCreate.GetComponent<Button>();

            var btnJoin = CreateButton(homePanel.transform, "BtnJoinRoom", spriteBtnGreen3D, new Vector2(0.715f, 0.205f), new Vector2(0.715f, 0.205f), new Vector2(0.5f, 0.5f), new Vector2(430f, 105f));
            if (spriteIconPeople != null)
            {
                var iconJoin = CreateImage(btnJoin.transform, "Icon", spriteIconPeople, new Vector2(0.16f, 0.50f), new Vector2(0.16f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f));
                iconJoin.GetComponent<Image>().preserveAspect = true;
            }
            var btnJoinTxt = CreateTMP(btnJoin.transform, "Label", fontAsset, "VÀO PHÒNG", 32, Color.white, TextAlignmentOptions.Center, new Vector2(0.58f, 0.50f), new Vector2(270f, 50f), wrap: false);
            btnJoinTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnJoinRoom = btnJoin.GetComponent<Button>();

            // 3. LUYỆN VỚI BOT (3D Navy Button - Y = 0.110f)
            var btnBot = CreateButton(homePanel.transform, "BtnBotTraining", spriteBtnNavy3D, new Vector2(0.5f, 0.110f), new Vector2(0.5f, 0.110f), new Vector2(0.5f, 0.5f), new Vector2(880f, 90f));
            if (spriteIconBot != null)
            {
                var iconBot = CreateImage(btnBot.transform, "Icon", spriteIconBot, new Vector2(0.14f, 0.50f), new Vector2(0.14f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f));
                iconBot.GetComponent<Image>().preserveAspect = true;
            }
            var btnBotTxt = CreateTMP(btnBot.transform, "Label", fontAsset, "LUYỆN VỚI BOT", 32, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.54f, 0.50f), new Vector2(740f, 45f), wrap: false);
            btnBotTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnBotTraining = btnBot.GetComponent<Button>();"""

if old_actions in code:
    code = code.replace(old_actions, new_actions)
    print("Updated Action buttons with 3D skins, icons and clean labels.")
else:
    print("WARNING: old_actions not found")

# 7. Update Bottom Dock Navigation (3 tabs: Trang chủ, Xếp hạng, Nhiệm vụ)
old_dock = """            // Bottom Nav bar buttons (Y = 0.035f, 3 tabs: Trang ch?, X?p h?ng, Nhi?m v?)
            var navBarBg = CreatePanel(homePanel.transform, "BottomNavBar", new Vector2(0.5f, 0.035f), new Vector2(0.5f, 0.035f), new Vector2(0.5f, 0.5f), new Vector2(1080f, 75f), null, new Color(0.06f, 0.08f, 0.12f, 0.95f));
            
            var btnHomeTab = CreateButton(navBarBg.transform, "BtnTabHome", null, new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(380f, 65f));
            var txtHomeTab = CreateTMP(btnHomeTab.transform, "Label", fontAsset, "TRANG CHỦ", 24, new Color(0.00f, 0.898f, 1f), TextAlignmentOptions.Center, wrap: false);
            txtHomeTab.fontStyle = FontStyles.Bold;
            uiCtrl.btnHomeLogin = btnHomeTab.GetComponent<Button>();"""

new_dock = """            // Bottom Dock Navigation (3D dock_bg with 3 tabs: Trang chủ, Xếp hạng, Nhiệm vụ)
            var navBarBg = CreatePanel(homePanel.transform, "BottomNavBar", new Vector2(0.5f, 0.038f), new Vector2(0.5f, 0.038f), new Vector2(0.5f, 0.5f), new Vector2(960f, 100f), spriteDockBg, Color.white);
            
            // Tab 1: Trang chủ (Active / Cyan)
            var btnHomeTab = CreateButton(navBarBg.transform, "BtnTabHome", null, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(280f, 70f));
            if (spriteIconHome != null)
            {
                var iconTabHome = CreateImage(btnHomeTab.transform, "Icon", spriteIconHome, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(34f, 34f));
                iconTabHome.GetComponent<Image>().preserveAspect = true;
            }
            var txtHomeTab = CreateTMP(btnHomeTab.transform, "Label", fontAsset, "Trang chủ", 24, new Color(0.00f, 0.898f, 1f), TextAlignmentOptions.Center, new Vector2(0.62f, 0.50f), new Vector2(170f, 40f), wrap: false);
            txtHomeTab.fontStyle = FontStyles.Bold;
            uiCtrl.btnHomeLogin = btnHomeTab.GetComponent<Button>();

            // Tab 2: Xếp hạng
            var btnRankTab = CreateButton(navBarBg.transform, "BtnTabRank", null, new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(280f, 70f));
            if (spriteIconTrophy != null)
            {
                var iconTabRank = CreateImage(btnRankTab.transform, "Icon", spriteIconTrophy, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(34f, 34f));
                iconTabRank.GetComponent<Image>().preserveAspect = true;
            }
            var txtRankTab = CreateTMP(btnRankTab.transform, "Label", fontAsset, "Xếp hạng", 22, colorCream, TextAlignmentOptions.Center, new Vector2(0.62f, 0.50f), new Vector2(170f, 40f), wrap: false);
            btnRankTab.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ShowToast("Tính năng Xếp hạng sắp ra mắt!");
            });

            // Tab 3: Nhiệm vụ
            var btnQuestTab = CreateButton(navBarBg.transform, "BtnTabQuest", null, new Vector2(0.80f, 0.50f), new Vector2(0.80f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(280f, 70f));
            if (spriteIconGift != null)
            {
                var iconTabQuest = CreateImage(btnQuestTab.transform, "Icon", spriteIconGift, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(34f, 34f));
                iconTabQuest.GetComponent<Image>().preserveAspect = true;
            }
            var txtQuestTab = CreateTMP(btnQuestTab.transform, "Label", fontAsset, "Nhiệm vụ", 22, colorCream, TextAlignmentOptions.Center, new Vector2(0.62f, 0.50f), new Vector2(170f, 40f), wrap: false);
            btnQuestTab.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ShowToast("Nhiệm vụ ngày sắp ra mắt!");
            });"""

if old_dock in code:
    code = code.replace(old_dock, new_dock)
    print("Updated Bottom Dock Navigation with 3 tabs.")
else:
    print("WARNING: old_dock not found")

# 8. Update PopulateConfigVisualAssets to NOT use cau_be_lang_aim.png
old_config_aim = """            configAsset.namLangPoseSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_aim.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_throw_release.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_hit.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_defeat.png")
            };"""

new_config_aim = """            configAsset.namLangPoseSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"), // Keep cute idle chibi in Aim pose
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"), // Keep cute idle chibi in Throw pose
                LoadSprite("Assets/Sprites/cau_be_lang_hit.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_defeat.png")
            };"""

if old_config_aim in code:
    code = code.replace(old_config_aim, new_config_aim)
    print("Updated PopulateConfigVisualAssets namLangPoseSprites.")
else:
    print("WARNING: old_config_aim not found")

path.write_text(code, encoding='utf-8')
print("Finished patching ProductionUISetup.cs!")
