import re
import sys

sys.stdout.reconfigure(encoding='utf-8')
with open(r'D:/HITME GAME/unity-client/Assets/Scripts/UI/UiStrings.cs', encoding='utf-8') as f:
    s = f.read()

consts = re.findall(r'public const string (\w+)\s*=\s*"([^"]*)"', s)
for k, v in consts:
    print(f"{k} = {v}")

