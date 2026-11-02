import sys
sys.stdout.reconfigure(encoding='utf-8')
with open(r'D:/HITME GAME/unity-client/Assets/Scripts/UI/UiStrings.cs', encoding='utf-8') as f:
    text = f.read()

import re
# Resolve all public const string values
const_map = dict(re.findall(r'public const string (\w+)\s*=\s*"([^"]*)";', text))

m = re.search(r'EnglishMap = new Dictionary<string, string>\s*\{([^;]+)\};', text)
if m:
    entries = re.findall(r'\{\s*([^,]+?)\s*,', m.group(1))
    seen = {}
    for e in entries:
        raw_key = e.strip()
        val = const_map.get(raw_key, raw_key.strip('"'))
        if val in seen:
            print(f'DUPLICATE VALUE: "{val}" from expr: {raw_key} (already added as {seen[val]})')
        else:
            seen[val] = raw_key
    print(f'Done checking {len(seen)} unique values.')
