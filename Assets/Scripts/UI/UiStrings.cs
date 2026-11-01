namespace HitMe.UI
{
    public static class UiStrings
    {
        // Headers & Screen Titles
        public const string GameTitle = "HIT ME";
        public const string HomeSubtitle = "NÉM DÉP LÀNG QUÊ";
        public const string BotTrainingTitle = "LUYỆN VỚI BOT (CỤC BỘ)";
        public const string BotTrainingSubtitle = "Chế độ một mình trên máy • Không cần mạng\nĐúng luật v1.2 (3 HP, 5s đặt, ném đồng thời)";
        public const string LobbyTitle = "PHÒNG CHIẾN HỮU • SÂN LÀNG";
        public const string MatchResultTitle = "KẾT THÚC TRẬN ĐẤU";
        public const string VictorySubtitle = "CHIẾN THẮNG TUYỆT ĐỐI!";
        public const string DefeatSubtitle = "CHÚC MAY MẮN LẦN SAU!";

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

        // Matchmaking & Lobby Notifications
        public const string MatchmakingSearching = "Đang tìm đối thủ…";
        public const string MatchmakingTimeoutSuggest = "Chưa tìm thấy đối thủ. Chơi với bot?";
        public const string LobbyOnePlayerWarning = "Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot.";
        public const string LobbyWaitingAllReady = "Đang chờ tất cả người chơi sẵn sàng...";
        public const string LobbyStartingCountdown = "Trận đấu sắp bắt đầu sau {0}s...";

        // Character Descriptions (7 nhân vật)
        public const string Char1Name = "CẬU BÉ LÀNG";
        public const string Char1Desc = "Nhanh nhẹn, tinh quái, quen thuộc sân đình";
        public const string Char2Name = "CÔ NÀNG THỂ THAO";
        public const string Char2Desc = "Năng động hoạt bát, phản xạ cực nhanh";
        public const string Char3Name = "ANH NÚI";
        public const string Char3Desc = "Lực lưỡng bền bỉ, sức ném mạnh mẽ";
        public const string Char4Name = "MAI HỒNG";
        public const string Char4Desc = "Duyên dáng khéo léo, ném dép chuẩn xác";
        public const string Char5Name = "ANH NÓN LÁ";
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
        public const string WaitingPlayerSlot = "Đang chờ…";
        public const string MinPlayersWarning = "Cần ít nhất 2 người để bắt đầu";
        public const string HpFormat = "{0} HP";
        public const string TotalPlayersFormat = "{0} người ({1} Bot)";

        // Vietnamese Glyph Verification String
        public const string GlyphCheckString = "Ế Ổ Ữ Ặ Đ Ằ Ẳ Ố Ậ Ứ Ớ Ệ Ồ Ẵ";
    }
}
