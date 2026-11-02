import re
import sys

sys.stdout.reconfigure(encoding='utf-8')
with open(r'D:/HITME GAME/unity-client/Assets/Scripts/UI/ProductionUIController.cs', encoding='utf-8') as f:
    text = f.read()

# find string literals
lines = text.split('\n')
for i, line in enumerate(lines):
    # look for hardcoded strings that might be user-facing
    matches = re.findall(r'"([^"\\]*(?:\\.[^"\\]*)*)"', line)
    for m in matches:
        # check if it contains Vietnamese characters or user-facing english
        if any(c in m for c in 'ăâđêôơưáàảãạắằẳẵặấầẩẫậéèẻẽẹếềểễệíìỉĩịóòỏõọốồổỗộớờởỡợúùủũụứừửữựýỳỷỹỵ') or any(w in m for w in ['Room', 'Lobby', 'Ready', 'Start', 'Cancel', 'Leave', 'Connecting', 'Reconnecting', 'Chủ phòng', 'Bạn']):
            print(f"Line {i+1}: {m}")
