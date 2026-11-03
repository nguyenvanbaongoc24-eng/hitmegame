import re
import sys
from fontTools.ttLib import TTFont

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')

font = TTFont('unity-client/Assets/Fonts/BeVietnamPro-Bold.ttf')
cmap = font.getBestCmap()

with open('unity-client/Assets/Editor/ProductionUISetup.cs', 'rb') as f:
    text = f.read().decode('utf-8', errors='ignore')

strings = re.findall(r'"([^"\\]*(?:\\.[^"\\]*)*)"', text)
missing = {}
occurrences = []

for s in strings:
    for ch in s:
        cp = ord(ch)
        if cp not in cmap and cp >= 128:
            missing[ch] = hex(cp)
            occurrences.append((ch, hex(cp), s))

print(f"Total missing in ProductionUISetup.cs: {len(missing)} distinct characters")
for ch, h in missing.items():
    print(f"U+{h[2:].upper().zfill(4)} ({repr(ch)})")

for ch, h, s in occurrences:
    print(f"  In string: {repr(s)}")
