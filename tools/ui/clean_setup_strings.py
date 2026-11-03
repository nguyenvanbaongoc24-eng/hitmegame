import sys
from pathlib import Path

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')

SETUP_PATH = Path('unity-client/Assets/Editor/ProductionUISetup.cs')
with open(SETUP_PATH, 'rb') as f:
    code = f.read().decode('utf-8')

# Replacements for clean typography
replacements = [
    ('Nam Làng\\n<size=16><color=#aaccff>Hồ sơ ▸</color></size>', 'Nam Làng\\n<size=16><color=#aaccff>Hồ sơ</color></size>'),
    ('"Nam Làng ▸"', '"Nam Làng"'),
    ('"Dép ▸"', '"Dép Tổ Ong"'),
    ('"► " + UiStrings.BtnPlayNow', 'UiStrings.BtnPlayNow'),
    ('"⚔ " + UiStrings.BtnJoinRoom', 'UiStrings.BtnJoinRoom'),
    ('"🤖 " + UiStrings.BtnPracticeBots + " ▸"', 'UiStrings.BtnPracticeBots'),
    ('"🏠 Trang chủ"', '"TRANG CHỦ"'),
    ('"🏆 Xếp hạng"', '"XẾP HẠNG"'),
    ('"📜 Nhiệm vụ"', '"NHIỆM VỤ"'),
    ('✅ Đã tìm thấy:', 'Đã tìm thấy:'),
    ('★ THẮNG', 'CHIẾN THẮNG'),
    ('✖ Đã loại', 'Đã loại'),
]

for old, new in replacements:
    if old in code:
        code = code.replace(old, new)
        print(f"Replaced: {repr(old)} -> {repr(new)}")
    else:
        print(f"WARNING: Not found: {repr(old)}")

with open(SETUP_PATH, 'wb') as f:
    f.write(code.encode('utf-8'))

print("Completed cleaning strings in ProductionUISetup.cs")
