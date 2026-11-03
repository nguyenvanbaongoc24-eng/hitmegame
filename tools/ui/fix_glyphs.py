import os
import re
import sys

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')

def replace_in_file(path, replacements):
    with open(path, 'rb') as f:
        data = f.read().decode('utf-8')
    count = 0
    for old, new in replacements:
        if old in data:
            data = data.replace(old, new)
            print(f'Replaced {repr(old)} -> {repr(new)} in {path}')
            count += 1
        else:
            print(f'WARNING: {repr(old)} not found in {path}')
    with open(path, 'wb') as f:
        f.write(data.encode('utf-8'))
    return count

print("=== Fixing glyphs in scripts and data ===")

# 1. UiStrings.cs
replace_in_file('unity-client/Assets/Scripts/UI/UiStrings.cs', [
    ('✓ ĐÃ GỬI', 'ĐÃ GỬI'),
    ('✓ SENT', 'SENT'),
    ('👑 [CHỦ PHÒNG] Nam Làng (Bạn)', '[CHỦ PHÒNG] Nam Làng (Bạn)'),
    ('👑 [HOST] Nam Lang (You)', '[HOST] Nam Lang (You)')
])

# 2. ProductionUIController.cs
replace_in_file('unity-client/Assets/Scripts/UI/ProductionUIController.cs', [
    ('"❮"', '"<"'),
    ('"❯"', '">"'),
    ('string tierBadge = tier != null ? tier.badge : "🥉";', 'string tierBadge = tier != null ? tier.badge : "";'),
    ('{tierBadge} ', ''),
    ('⏰ Đặt lại lúc 00:00 (Giờ Việt Nam)', 'Đặt lại lúc 00:00 (Giờ Việt Nam)'),
    ('⚠️ [LỖI] ', '[LỖI] '),
    ('message.Contains("⚠️")', 'message.Contains("[LỖI]")'),
    ('✅ Đã tìm thấy:', 'Đã tìm thấy:'),
    ('👑 [CHỦ PHÒNG] Nam Làng (Bạn)', '[CHỦ PHÒNG] Nam Làng (Bạn)'),
    ('(isHost ? "👑 " : "")', '(isHost ? "[CHỦ PHÒNG] " : "")'),
    ('$"🏟️ {prefix} {mapName} ⇄"', '$"[BẢN ĐỒ] {prefix} {mapName}"'),
    ('"⌫ XÓA"', '"XÓA"'),
    ('★ ĐỒNG HẠNG 1', 'ĐỒNG HẠNG 1'),
    ('★ THẮNG', 'CHIẾN THẮNG'),
    ('✖ Đã loại', 'Đã loại'),
    ('txt.text = "💬";', 'txt.text = "CHAT";'),
    ('string[] emotes = { "😎", "🔥", "🎯", "😅", "💀" };', 'string[] emotes = { "Haha", "Hay", "Né đi", "Ôi", "Cay" };'),
    ('ShowToast($"💬 Nam Làng: {content}", 2.0f);', 'ShowToast($"Nam Làng: {content}", 2.0f);')
])

# 3. UIManager.cs
replace_in_file('unity-client/Assets/Scripts/UI/UIManager.cs', [
    ('"♥ " : "♡ "', '"[HP] " : "[-] "')
])

# 4. JuiceFeedbackController.cs
replace_in_file('unity-client/Assets/Scripts/UI/JuiceFeedbackController.cs', [
    ('text.text = $"{damage} HP 💔";', 'text.text = $"{damage} HP";'),
    ('text.text = $"-{damage} HP 💔";', 'text.text = $"-{damage} HP";')
])

# 5. RankConfig.json
replace_in_file('unity-client/Assets/Data/RankConfig.json', [
    ('"badge": "🥉"', '"badge": "I"'),
    ('"badge": "🥈"', '"badge": "II"'),
    ('"badge": "🥇"', '"badge": "III"'),
    ('"badge": "💎"', '"badge": "IV"'),
    ('"badge": "👑"', '"badge": "V"')
])

# 6. RankAndQuestService.cs
replace_in_file('unity-client/Assets/Scripts/Economy/RankAndQuestService.cs', [
    ('badge = "🥉"', 'badge = "I"'),
    ('badge = "🥈"', 'badge = "II"'),
    ('badge = "🥇"', 'badge = "III"'),
    ('badge = "💎"', 'badge = "IV"'),
    ('badge = "👑"', 'badge = "V"')
])

# Also check ProductionUISetup.cs:
replace_in_file('unity-client/Assets/Editor/ProductionUISetup.cs', [
    ('"❮"', '"<"'),
    ('"❯"', '">"'),
    ('"⚙"', '""'),
    ('"🪙 1.250 +"', '"1.250 +"'),
    ('"💎 50"', '"50"')
])

print("Finished applying glyph fixes.")
