using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.UI
{
    public enum AppLanguage
    {
        Vietnamese,
        English
    }

    /// <summary>
    /// Localized strings (VI / EN) for HIT ME.
    /// Preserves all public const string fields for contract tests & backward compatibility.
    /// Supports dynamic English translation via UiStrings.Get() and UiStrings.CurrentLanguage.
    /// </summary>
    public static class UiStrings
    {
        public const string PrefKeyLanguage = "HitMe_Language";

        private static AppLanguage _currentLanguage = AppLanguage.Vietnamese;

        public static AppLanguage CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                try
                {
                    PlayerPrefs.SetString(PrefKeyLanguage, value == AppLanguage.English ? "EN" : "VI");
                    PlayerPrefs.Save();
                }
                catch { /* WebGL sandbox */ }
                OnLanguageChanged?.Invoke();
            }
        }

        public static event Action OnLanguageChanged;

        public static bool IsVietnamese => CurrentLanguage == AppLanguage.Vietnamese;

        public static void SetLanguage(AppLanguage lang)
        {
            CurrentLanguage = lang;
        }

        static UiStrings()
        {
            try
            {
                string saved = PlayerPrefs.GetString(PrefKeyLanguage, "VI");
                _currentLanguage = string.Equals(saved, "EN", StringComparison.OrdinalIgnoreCase)
                    ? AppLanguage.English
                    : AppLanguage.Vietnamese;
            }
            catch
            {
                _currentLanguage = AppLanguage.Vietnamese;
            }
        }

        // ──────────────────────────────────────────────────
        // Public Const Strings (Vietnamese Default & Canonical)
        // ──────────────────────────────────────────────────

        // Headers & Screen Titles
        public const string GameTitle = "HIT ME";
        public const string HomeSubtitle = "NÉM DÉP LÀNG QUÊ";
        public const string BotTrainingTitle = "LUYỆN VỚI BOT (CỤC BỘ)";
        public const string BotTrainingSubtitle = "Chơi một mình trên máy • Không cần mạng\nĐúng luật v1.2 (3 HP, 5s đặt, ném đồng thời)";
        public const string LobbyTitle = "PHÒNG CHIẾN HỮU • SÂN LÀNG";
        public const string MatchResultTitle = "KẾT THÚC TRẬN ĐẤU";
        public const string VictorySubtitle = "CHIẾN THẮNG TUYỆT ĐỐI!";
        public const string DefeatSubtitle = "CHÚC MAY MẮN LẦN SAU!";

        // Settings Menu (Lô 5)
        public const string SettingsTitle = "CÀI ĐẶT";
        public const string SettingsAudioHeader = "ÂM THANH";
        public const string SettingsMusic = "NHẠC NỀN";
        public const string SettingsSfx = "HIỆU ỨNG";
        public const string SettingsReducedMotion = "GIẢM CHUYỂN ĐỘNG";
        public const string SettingsShowFps = "HIỂN THỊ FPS";
        public const string SettingsLanguage = "NGÔN NGỮ";
        public const string SettingsBtnClose = "ĐÓNG";
        public const string SettingsBtnLeaveMatch = "THOÁT TRẬN";

        // Gameplay Phases
        public const string PhasePlacing = "ĐẶT VỊ TRÍ & HƯỚNG NÉM";
        public const string PhaseLocked = "ĐÃ KHÓA VỊ TRÍ";
        public const string PhaseReveal = "LỘ DIỆN ĐỐI THỦ!";
        public const string PhaseThrowing = "NÉM DÉP TỔ ONG!";
        public const string PhaseRoundResult = "KẾT QUẢ VÒNG";

        // Buttons
        public const string BtnReady = "SẴN SÀNG";
        public const string BtnLocked = "ĐÃ KHÓA";
        public const string BtnStart = "BẮT ĐẦU";
        public const string BtnPlayNow = "CHƠI NGAY / GHÉP NHANH";
        public const string BtnPracticeBots = "LUYỆN VỚI BOT";
        public const string BtnCreateRoom = "TẠO PHÒNG";
        public const string BtnJoinRoom = "VÀO PHÒNG";
        public const string BtnGoToBotTraining = "SANG LUYỆN VỚI BOT";
        public const string BtnReplay = "ĐẤU LẠI";
        public const string BtnBackToHome = "VỀ TRANG CHỦ";
        public const string BtnLeaveRoom = "RỜI PHÒNG";
        public const string BtnCopyRoomCode = "SAO CHÉP MÃ";
        public const string BtnCodeCopied = "ĐÃ SAO CHÉP!";
        public const string BtnSettings = "CÀI ĐẶT";
        public const string BtnAddFriend = "+ KẾT BẠN";
        public const string BtnFriendSent = "✓ ĐÃ GỬI";

        // Matchmaking & Lobby Notifications
        public const string MatchmakingSearching = "Đang tìm đối thủ...";
        public const string MatchmakingTimeoutSuggest = "Chưa tìm thấy đối thủ. Chơi với bot?";
        public const string LobbyOnePlayerWarning = "Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot.";
        public const string LobbyWaitingAllReady = "Đang chờ tất cả người chơi sẵn sàng...";
        public const string LobbyStartingCountdown = "Trận đấu sắp bắt đầu sau {0}s...";

        // Character Descriptions (7 nhân vật)
        public const string Char1Name = "CẬU BÉ LÀNG";
        public const string Char1Desc = "Nhanh nhẹn, tinh quái, quen thuộc sân đình";
        public const string Char2Name = "CÔ NÀNG THỂ THAO";
        public const string Char2Desc = "Năng động hoạt bát, phản xạ cực nhanh";
        public const string Char3Name = "ANH NÓN LÁ";
        public const string Char3Desc = "Vạm vỡ bền bỉ, sức ném mạnh mẽ";
        public const string Char4Name = "BÉ HOA";
        public const string Char4Desc = "Duyên dáng khéo léo, ném dép chuẩn xác";
        public const string Char5Name = "BÁC NÔNG DÂN";
        public const string Char5Desc = "Phong trần giản dị, dày dạn kinh nghiệm";
        public const string Char6Name = "CHÚ CHÓ VÀNG";
        public const string Char6Desc = "Trung thành hiếu động, chạy nhảy cực nhanh";
        public const string Char7Name = "MÈO MUN";
        public const string Char7Desc = "Bí ẩn ranh mãnh, né tránh tài tình";

        public static readonly string[] CharacterNames =
        {
            Char1Name, Char2Name, Char3Name, Char4Name, Char5Name, Char6Name, Char7Name
        };

        public static readonly string[] CharacterDescriptions =
        {
            Char1Desc, Char2Desc, Char3Desc, Char4Desc, Char5Desc, Char6Desc, Char7Desc
        };

        // Weapon
        public const string WeaponSandalName = "DÉP TỔ ONG";

        // Rankings & Status
        public const string Rank1 = "HẠNG 1";
        public const string Rank2 = "HẠNG 2";
        public const string Rank3 = "HẠNG 3";
        public const string RankOther = "HẠNG {0}";

        public const string RoundFormat = "VÒNG {0}/{1}";
        public const string RoomCodeFormat = "MÃ PHÒNG: {0} • SÂN LÀNG";
        public const string WaitingPlayerSlot = "Đang chờ...";
        public const string MinPlayersWarning = "Cần ít nhất 2 người để bắt đầu";
        public const string HpFormat = "{0} HP";
        public const string TotalPlayersFormat = "{0} người ({1} Bot)";

        // Leave Confirmation Dialog
        public const string LeaveDialogTitle = "XÁC NHẬN RỜI PHÒNG";
        public const string LeaveDialogMessage = "Bạn có chắc chắn muốn rời phòng không?\nMọi kết quả của trận đấu sẽ không được lưu.";
        public const string BtnConfirmLeave = "XÁC NHẬN";
        public const string BtnCancelLeave = "QUAY LẠI";

        // Tutorial 3-Step Gesture (Lô 6)
        public const string PrefKeyTutorialCompleted = "HitMe_TutorialCompleted";
        public const string TutorialTitle = "HƯỚNG DẪN CỬ CHỈ (v1.2)";
        public const string TutorialStep1Title = "BƯỚC 1/3: ĐẶT NHÂN VẬT";
        public const string TutorialStep1Body = "Chạm vào vùng sân để đặt vị trí nhân vật của bạn.";
        public const string TutorialStep2Title = "BƯỚC 2/3: KÉO ĐỂ NGẮM";
        public const string TutorialStep2Body = "Giữ rồi trượt để ngắm. Thước ngắm bám theo đầu ngón tay.";
        public const string TutorialStep3Title = "BƯỚC 3/3: ĐỔI VỊ TRÍ & HỦY";
        public const string TutorialStep3Body = "Chạm chỗ khác để làm lại. Kéo về dưới 4mm để hủy. Không ngắm = ném thẳng.";
        public const string BtnTutorialSkip = "BỎ QUA";
        public const string BtnTutorialNext = "TIẾP THEO";
        public const string BtnTutorialDone = "ĐÃ HIỂU";

        // Profile, Shop, Inventory, Leaderboard, Login (Lô 7 & 8)
        public const string ProfileTitle = "HỒ SƠ NGƯỜI CHƠI";
        public const string ShopTitle = "CỬA HÀNG VŨ KHÍ";
        public const string ShopSubtitle = "Trang trí thuần túy • Không đổi chỉ số chiến đấu";
        public const string InventoryTitle = "TÚI ĐỒ VŨ KHÍ";
        public const string LeaderboardTitle = "BẢNG XẾP HẠNG SÂN LÀNG";
        public const string LoginTitle = "ĐĂNG NHẬP";
        public const string LoginGuest = "CHƠI VỚI TƯ CÁCH KHÁCH";
        public const string LoginComingSoon = "SẮP CÓ";
        public const string LoginGoogleComingSoon = "Đăng nhập Google sắp ra mắt";
        public const string LoginFacebookComingSoon = "Đăng nhập Facebook sắp ra mắt";
        public const string LoginAppleComingSoon = "Đăng nhập Apple sắp ra mắt";
        public const string LoadingData = "Đang tải...";
        public const string NoData = "Chưa có dữ liệu";
        public const string BtnBuy = "MUA";
        public const string BtnEquip = "TRANG BỊ";
        public const string BtnEquipped = "ĐANG DÙNG";
        public const string BtnRename = "ĐỔI TÊN";
        public const string GoldFormat = "{0} VÀNG";
        public const string FreeRenameLabel = "LẦN ĐẦU MIỄN PHÍ";
        public const string CostRenameLabel = "100 VÀNG";

        // Error Toast
        public const string ErrorRoomNotFound = "Không tìm thấy mã phòng";

        // Vietnamese Glyph Verification String
        public const string GlyphCheckString = "ă â đ ê ô ơ ư á à ả ã ạ ắ ằ ẳ ẵ ặ ấ ầ ổ ỗ ộ ớ ờ ở ỡ ợ ứ ừ ử ữ ự";

        // ──────────────────────────────────────────────────
        // English Translation Mapping Table (Lô 5)
        // ──────────────────────────────────────────────────
        private static readonly Dictionary<string, string> EnglishMap = new Dictionary<string, string>
        {
            { HomeSubtitle, "VILLAGE SLIPPER TOSS" },
            { BotTrainingTitle, "BOT PRACTICE (OFFLINE)" },
            { BotTrainingSubtitle, "Solo on device • No internet needed\nRule v1.2 compliant (3 HP, 5s placing, simultaneous throw)" },
            { LobbyTitle, "FRIENDS LOBBY • VILLAGE ARENA" },
            { MatchResultTitle, "MATCH FINISHED" },
            { VictorySubtitle, "GLORIOUS VICTORY!" },
            { DefeatSubtitle, "BETTER LUCK NEXT TIME!" },

            { SettingsTitle, "SETTINGS" },
            { SettingsAudioHeader, "AUDIO" },
            { SettingsMusic, "MUSIC" },
            { SettingsSfx, "SOUND EFFECTS" },
            { SettingsReducedMotion, "REDUCED MOTION" },
            { SettingsShowFps, "SHOW FPS" },
            { SettingsLanguage, "LANGUAGE" },
            { SettingsBtnClose, "CLOSE" },
            { SettingsBtnLeaveMatch, "LEAVE MATCH" },

            { PhasePlacing, "SET POSITION & AIM" },
            { PhaseLocked, "POSITION LOCKED" },
            { PhaseReveal, "REVEAL OPPONENTS!" },
            { PhaseThrowing, "TOSS HONEYCOMB SLIPPERS!" },
            { PhaseRoundResult, "ROUND RESULT" },

            { BtnReady, "READY" },
            { BtnLocked, "LOCKED" },
            { BtnStart, "START" },
            { BtnPlayNow, "PLAY NOW / QUICK MATCH" },
            { BtnPracticeBots, "BOT PRACTICE" },
            { BtnCreateRoom, "CREATE ROOM" },
            { BtnJoinRoom, "JOIN ROOM" },
            { BtnGoToBotTraining, "GO TO BOT PRACTICE" },
            { BtnReplay, "REPLAY" },
            { BtnBackToHome, "BACK TO HOME" },
            { BtnLeaveRoom, "LEAVE ROOM" },
            { BtnCopyRoomCode, "COPY CODE" },
            { BtnCodeCopied, "COPIED!" },

            { MatchmakingSearching, "Searching for opponents..." },
            { MatchmakingTimeoutSuggest, "No opponent found. Play with bots?" },
            { LobbyOnePlayerWarning, "Needs at least 2 players. For solo, switch to Bot Practice menu." },
            { LobbyWaitingAllReady, "Waiting for all players to ready..." },

            { Char1Name, "VILLAGE BOY" },
            { Char1Desc, "Agile, witty, knows every village corner" },
            { Char2Name, "SPORTY GIRL" },
            { Char2Desc, "Energetic, dynamic, lightning reflexes" },
            { Char3Name, "CONICAL HAT GUY" },
            { Char3Desc, "Sturdy and resilient, powerful throws" },
            { Char4Name, "LITTLE HOA" },
            { Char4Desc, "Graceful and nimble, pinpoint slipper accuracy" },
            { Char5Name, "UNCLE FARMER" },
            { Char5Desc, "Weathered and wise, veteran experience" },
            { Char6Name, "GOLDEN DOG" },
            { Char6Desc, "Loyal and spirited, blazing sprint speed" },
            { Char7Name, "BLACK CAT" },
            { Char7Desc, "Mysterious and sly, elusive dodger" },

            { WeaponSandalName, "HONEYCOMB SLIPPER" },

            { Rank1, "RANK 1" },
            { Rank2, "RANK 2" },
            { Rank3, "RANK 3" },
            { WaitingPlayerSlot, "Waiting..." },
            { MinPlayersWarning, "Requires at least 2 players to start" },

            { LeaveDialogTitle, "LEAVE ROOM CONFIRMATION" },
            { LeaveDialogMessage, "Are you sure you want to leave the room?\nMatch progress will not be saved." },
            { BtnConfirmLeave, "CONFIRM" },
            { BtnCancelLeave, "CANCEL" },
            { ErrorRoomNotFound, "Room code not found" },

            { TutorialTitle, "GESTURE TUTORIAL (v1.2)" },
            { TutorialStep1Title, "STEP 1/3: PLACE CHARACTER" },
            { TutorialStep1Body, "Tap anywhere on the field to place your character." },
            { TutorialStep2Title, "STEP 2/3: DRAG TO AIM" },
            { TutorialStep2Body, "Hold and drag to aim. The aim line follows your finger." },
            { TutorialStep3Title, "STEP 3/3: RE-PLACE & CANCEL" },
            { TutorialStep3Body, "Tap elsewhere to re-place. Drag < 4mm to cancel. No aim = straight throw." },
            { BtnTutorialSkip, "SKIP" },
            { BtnTutorialNext, "NEXT" },
            { BtnTutorialDone, "GOT IT" },

            { ProfileTitle, "PLAYER PROFILE" },
            { ShopTitle, "WEAPON SHOP" },
            { ShopSubtitle, "Pure Cosmetics • No gameplay stat changes" },
            { InventoryTitle, "WEAPON INVENTORY" },
            { LeaderboardTitle, "VILLAGE LEADERBOARD" },
            { LoginTitle, "LOGIN" },
            { LoginGuest, "PLAY AS GUEST" },
            { LoginComingSoon, "COMING SOON" },
            { LoginGoogleComingSoon, "Google login coming soon" },
            { LoginFacebookComingSoon, "Facebook login coming soon" },
            { LoginAppleComingSoon, "Apple login coming soon" },
            { LoadingData, "Loading..." },
            { NoData, "No data" },
            { BtnBuy, "BUY" },
            { BtnEquip, "EQUIP" },
            { BtnEquipped, "EQUIPPED" },
            { BtnRename, "RENAME" },
            { FreeRenameLabel, "FIRST TIME FREE" },
            { CostRenameLabel, "100 GOLD" },
            { "BẢNG XẾP HẠNG", "LEADERBOARD" },
            { "NHIỆM VỤ", "QUESTS" },
            { "SẮP RA MẮT", "COMING SOON" },
            { "Hệ thống nhiệm vụ đang hoàn thiện!", "Quest system is under development!" },
            { "Bảng xếp hạng mùa giải sẽ mở trong bản cập nhật tới!", "Season leaderboard will open in the next update!" },
            { "👑 [CHỦ PHÒNG] Nam Làng (Bạn)", "👑 [HOST] Nam Lang (You)" },
            { "Đang chờ…", "Waiting…" },
            { "Bạn", "You" },
            { "Còn sống", "Alive" },
            { "Đã loại", "Eliminated" },
            { "Đồng thắng: ", "Co-winners: " },
            { "Người thắng: ", "Winner: " },
            { "Không có", "None" },
            { "Thời gian còn lại: ", "Time remaining: " },
            { "Đã hủy kết nối lại và về trang chủ.", "Reconnection canceled. Returned to Home." },
            { "Phiên vào lại đã hết hạn (30s). Đã trở về trang chủ.", "Reconnection expired (30s). Returned to Home." },
            { "Phòng Chiến Hữu", "Friends Lobby" },
            { "Sân Làng", "Village Arena" },
            { "XÁC NHẬN RỜI TRẬN", "LEAVE MATCH CONFIRMATION" },
            { "Bạn có chắc chắn muốn rời trận không?", "Are you sure you want to leave the match?" },
            { "ĐÃ SAO CHÉP MÃ PHÒNG: ", "ROOM CODE COPIED: " },
            { BtnAddFriend, "+ ADD FRIEND" },
            { BtnFriendSent, "✓ SENT" },
            { "Đã gửi lời mời kết bạn tới {0}!", "Friend request sent to {0}!" }
        };

        /// <summary>
        /// Returns localized text according to UiStrings.CurrentLanguage.
        /// When English, translates using EnglishMap; otherwise returns the original Vietnamese text.
        /// </summary>
        public static string Get(string viText)
        {
            if (string.IsNullOrEmpty(viText)) return string.Empty;
            if (_currentLanguage == AppLanguage.English && EnglishMap.TryGetValue(viText, out var enText))
            {
                return enText;
            }
            return viText;
        }

        public static string[] GetLocalizedCharacterNames()
        {
            var arr = new string[CharacterNames.Length];
            for (int i = 0; i < CharacterNames.Length; i++)
            {
                arr[i] = Get(CharacterNames[i]);
            }
            return arr;
        }

        private static readonly Dictionary<string, (string vi, string en)> NetErrorMap = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            { "not_enough_players", ("Cần ít nhất 2 người chơi để bắt đầu trận!", "At least 2 players required to start match!") },
            { "protocol_mismatch", ("Phiên bản trò chơi đã cũ, vui lòng tải lại trang!", "Game version outdated, please reload the page!") },
            { "auth_required_use_join_token", ("Cần đăng nhập để tham gia phòng!", "Authentication required to join room!") },
            { "auth_invalid", ("Mã đăng nhập không hợp lệ!", "Invalid authentication token!") },
            { "token_invalid", ("Mã đăng nhập không hợp lệ!", "Invalid token!") },
            { "reconnect_invalid", ("Không thể kết nối lại trận đấu!", "Cannot reconnect to match!") },
            { "room_full", ("Phòng đã đầy người chơi!", "Room is full!") },
            { "room_limit", ("Máy chủ đã đạt giới hạn phòng!", "Server room limit reached!") },
            { "room_code_exhausted", ("Hết mã phòng khả dụng, vui lòng thử lại sau!", "Room codes exhausted, please try again later!") },
            { "room_not_found", ("Không tìm thấy phòng chơi!", "Room not found!") },
            { "room_started", ("Trận đấu trong phòng đã bắt đầu!", "Match already started!") },
            { "banned", ("Tài khoản bị cấm do AFK. Vui lòng chờ hết thời gian phạt!", "Account suspended for AFK. Please wait out the penalty!") },
            { "rate_limited", ("Thao tác quá nhanh, vui lòng gửi chậm lại!", "Rate limit reached, please slow down!") },
            { "message_too_large", ("Dữ liệu gửi vượt quá kích thước cho phép!", "Message payload too large!") },
            { "legacy_ready_use_room_ready_or_lock_ready", ("Giao thức cũ, vui lòng cập nhật game!", "Legacy protocol, please update game!") },
            { "invalid_json", ("Dữ liệu không hợp lệ!", "Invalid payload format!") },
            { "invalid_envelope", ("Gói tin không hợp lệ!", "Invalid envelope format!") },
            { "unknown_type", ("Thao tác không xác định!", "Unknown action type!") },
            { "already_joined", ("Bạn đã ở trong phòng chơi rồi!", "Already joined in a room!") },
            { "not_joined", ("Bạn chưa tham gia phòng!", "Not joined in any room!") },
            { "not_in_room", ("Không tìm thấy phòng!", "Not in room!") },
            { "not_in_round", ("Trận đấu chưa bắt đầu vòng!", "Not in active round!") },
            { "wrong_phase", ("Thao tác không đúng thời điểm!", "Action invalid in current phase!") },
            { "wrong_round", ("Vòng đấu không khớp!", "Round mismatch!") },
            { "round_locked", ("Lượt đặt dép đã bị khóa!", "Round already locked!") },
            { "ready_required", ("Bạn cần sẵn sàng trước khi tiếp tục!", "Ready required before proceeding!") },
            { "match_not_started", ("Trận đấu chưa bắt đầu!", "Match has not started!") },
            { "invalid_commit", ("Tọa độ hoặc hướng ném không hợp lệ!", "Invalid placement or aim coordinates!") },
            { "invalid_emote", ("Biểu cảm không hợp lệ!", "Invalid emote identifier!") },
            { "emote_rate_limited", ("Bạn gửi biểu cảm quá nhanh!", "Emote rate limit reached!") },
            { "emote_phase_limit", ("Đã đạt tối đa 3 biểu cảm mỗi vòng!", "Maximum 3 emotes per round reached!") },
            { "host_only", ("Chỉ chủ phòng mới có quyền thực hiện!", "Only host can perform this action!") },
            { "duplicate_player", ("Người chơi đã tồn tại trong phòng!", "Duplicate player in room!") },
            { "invalid_player", ("Thông tin người chơi không hợp lệ!", "Invalid player data!") },
            { "no_response", ("Máy chủ không phản hồi, vui lòng thử lại!", "Server did not respond, please retry!") },
            { "server_shutdown", ("Máy chủ đang bảo trì hoặc đã đóng kết nối!", "Server shutting down or maintenance!") }
        };

        public static string GetNetErrorMessage(string code, int remainingSeconds = 0)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            if (NetErrorMap.TryGetValue(code, out var tuple))
            {
                string msg = _currentLanguage == AppLanguage.English ? tuple.en : tuple.vi;
                if (code.Equals("banned", StringComparison.OrdinalIgnoreCase) && remainingSeconds > 0)
                {
                    msg += _currentLanguage == AppLanguage.English ? $" ({remainingSeconds}s remaining)" : $" (còn {remainingSeconds}s)";
                }
                return msg;
            }
            return _currentLanguage == AppLanguage.English ? $"Network Error: {code}" : $"Lỗi mạng: {code}";
        }
    }
}
