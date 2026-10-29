using System.Collections.Generic;

namespace HitMe.Core
{
    /// <summary>
    /// Centralized Vietnamese Localization Table for Phase 2.
    /// Ensures 100% full Vietnamese diacritics and no missing keys.
    /// Pure C# implementation inside HitMe.Core assembly.
    /// </summary>
    public static class LocalizationTable
    {
        public const string KeyPhaseAim = "PHASE_AIM";
        public const string KeyOpponentHidden = "OPPONENT_HIDDEN";
        public const string KeyBtnReady = "BTN_READY";
        public const string KeyResultWin = "RESULT_WIN";
        public const string KeyResultLose = "RESULT_LOSE";
        public const string KeyResultDraw = "RESULT_DRAW";
        public const string KeyBtnRematch = "BTN_REMATCH";
        public const string KeyActionLocked = "ACTION_LOCKED";
        public const string KeyRoundPrefix = "ROUND_PREFIX";
        public const string KeyHpPlayer = "HP_PLAYER";
        public const string KeyHpBot = "HP_BOT";
        public const string KeyResultTitle = "RESULT_TITLE";
        public const string KeyHintTapPlace = "HINT_TAP_PLACE";
        public const string KeyHintHoldDrag = "HINT_HOLD_DRAG";
        public const string KeyHintRetapReset = "HINT_RETAP_RESET";

        private static readonly Dictionary<string, string> Strings = new Dictionary<string, string>
        {
            { KeyPhaseAim, "CHỌN HƯỚNG NÉM" },
            { KeyOpponentHidden, "Đối thủ đang ẩn" },
            { KeyBtnReady, "SẴN SÀNG" },
            { KeyResultWin, "Thắng" },
            { KeyResultLose, "Thua" },
            { KeyResultDraw, "Hòa nhau" },
            { KeyBtnRematch, "Chơi lại" },
            { KeyActionLocked, "ĐÃ KHÓA" },
            { KeyRoundPrefix, "VÒNG" },
            { KeyHpPlayer, "BẠN" },
            { KeyHpBot, "BOT" },
            { KeyResultTitle, "Kết quả" },
            { KeyHintTapPlace, "Chạm để đặt nhân vật" },
            { KeyHintHoldDrag, "Giữ và kéo để chọn hướng ném" },
            { KeyHintRetapReset, "Chạm chỗ khác để đặt lại · Giữ và kéo để đổi hướng" }
        };

        public static string Get(string key)
        {
            if (Strings.TryGetValue(key, out string value))
            {
                return value;
            }
            return key;
        }

        public static bool HasKey(string key) => Strings.ContainsKey(key);

        public static IReadOnlyDictionary<string, string> AllStrings => Strings;
    }
}
