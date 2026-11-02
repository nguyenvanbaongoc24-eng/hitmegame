import os
import glob
import re
from pathlib import Path

UNITY_CLIENT = Path("unity-client")

def optimize_texture_meta(meta_path, max_size=256, crunched=1):
    with open(meta_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Disable mipmaps and readable
    content = re.sub(r'enableMipMap:\s*\d+', 'enableMipMap: 0', content)
    content = re.sub(r'isReadable:\s*\d+', 'isReadable: 0', content)

    # Match the WebGL block in platformSettings
    pattern = r'(- serializedVersion: \d+\s+buildTarget: WebGL\s+maxTextureSize: )\d+(\s+resizeAlgorithm: \d+\s+textureFormat: [-\d]+\s+textureCompression: )\d+(\s+compressionQuality: \d+\s+crunchedCompression: )\d+(\s+allowsAlphaSplitting: \d+\s+overridden: )\d+'
    
    def repl(m):
        return f'{m.group(1)}{max_size}{m.group(2)}1{m.group(3)}{crunched}{m.group(4)}1'
    
    new_content, count = re.subn(pattern, repl, content)
    if count > 0:
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(new_content)
        return True
    return False

def main():
    modified = 0

    # 1. Assets/Sprites/*.png.meta
    for p in (UNITY_CLIENT / "Assets" / "Sprites").glob("*.png.meta"):
        name = p.stem.lower()
        if "arena" in name:
            max_s = 512
        elif any(k in name for k in ["dot", "glow", "marker", "ring", "shadow", "avatar"]):
            max_s = 128
        else:
            max_s = 256
        if optimize_texture_meta(p, max_size=max_s, crunched=1):
            modified += 1

    # 2. Assets/Art/UI/**/*.png.meta
    for p in (UNITY_CLIENT / "Assets" / "Art" / "UI").rglob("*.png.meta"):
        name = p.stem.lower()
        if "bg" in name or "village" in name:
            max_s = 512
        elif any(k in name for k in ["icon", "heart", "gem", "coin", "arrow", "dot", "knob", "track"]):
            max_s = 128
        else:
            max_s = 256
        if optimize_texture_meta(p, max_size=max_s, crunched=1):
            modified += 1

    # 3. Assets/Art/VFX/**/*.png.meta
    for p in (UNITY_CLIENT / "Assets" / "Art" / "VFX").rglob("*.png.meta"):
        if optimize_texture_meta(p, max_size=128, crunched=1):
            modified += 1

    # 4. Assets/Art/Weapons/**/*.png.meta
    for p in (UNITY_CLIENT / "Assets" / "Art" / "Weapons").rglob("*.png.meta"):
        if optimize_texture_meta(p, max_size=256, crunched=1):
            modified += 1

    print(f"Successfully optimized {modified} texture meta files with mobile-tuned resolutions (256/512)!")

if __name__ == "__main__":
    main()
