"""
A9 Texture and Asset Optimization Script for WebGL Build.
Enables WebGL platform overrides with crunch compression and appropriate maxTextureSize.
"""
import os
import glob
import re

UNITY_CLIENT_DIR = r"D:\HITME GAME\unity-client"

def update_webgl_meta(meta_path, max_size, crunched=1):
    if not os.path.exists(meta_path):
        return False
    with open(meta_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    pattern = r'(- serializedVersion: \d+\s+buildTarget: WebGL\s+maxTextureSize: )\d+(\s+resizeAlgorithm: \d+\s+textureFormat: [-\d]+\s+textureCompression: )\d+(\s+compressionQuality: \d+\s+crunchedCompression: )\d+(\s+allowsAlphaSplitting: \d+\s+overridden: )\d+'
    
    def repl(m):
        return f'{m.group(1)}{max_size}{m.group(2)}1{m.group(3)}{crunched}{m.group(4)}1'
    
    new_content, count = re.subn(pattern, repl, content)
    if count > 0:
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        return True
    return False

def main():
    modified = []

    # 1. Arena background (1024)
    arena_meta = os.path.join(UNITY_CLIENT_DIR, "Assets", "Sprites", "ARENA.png.meta")
    if update_webgl_meta(arena_meta, 1024, 1):
        modified.append(arena_meta)

    # 2. Home village background (1024)
    home_bg = os.path.join(UNITY_CLIENT_DIR, "Assets", "Art", "UI", "ui_bg_home_village.png.meta")
    if update_webgl_meta(home_bg, 1024, 1):
        modified.append(home_bg)

    # 3. VFX Hit impact frames (256)
    vfx_files = glob.glob(os.path.join(UNITY_CLIENT_DIR, "Assets", "Art", "VFX", "Hit_Impact", "*.png.meta"))
    for f in vfx_files:
        if update_webgl_meta(f, 256, 1):
            modified.append(f)

    # 4. Heart icons (128)
    heart_files = glob.glob(os.path.join(UNITY_CLIENT_DIR, "Assets", "Art", "UI", "Kit", "ui_icon_heart_*.png.meta"))
    for f in heart_files:
        if update_webgl_meta(f, 128, 1):
            modified.append(f)

    # 5. Aim kit (256)
    aim_files = glob.glob(os.path.join(UNITY_CLIENT_DIR, "Assets", "Art", "UI", "Kit", "ui_aim_*.png.meta"))
    for f in aim_files:
        if update_webgl_meta(f, 256, 1):
            modified.append(f)

    # 6. Panel and other UI Kit elements (512)
    ui_kit_files = glob.glob(os.path.join(UNITY_CLIENT_DIR, "Assets", "Art", "UI", "Kit", "*.png.meta"))
    for f in ui_kit_files:
        if f not in modified:
            if update_webgl_meta(f, 512, 1):
                modified.append(f)

    # 7. Disable Unity Splash Logo in ProjectSettings.asset
    proj_settings = os.path.join(UNITY_CLIENT_DIR, "ProjectSettings", "ProjectSettings.asset")
    with open(proj_settings, 'r', encoding='utf-8') as f:
        ps_content = f.read()
    
    ps_content = re.sub(r'm_ShowUnitySplashScreen: 1', 'm_ShowUnitySplashScreen: 0', ps_content)
    ps_content = re.sub(r'm_ShowUnitySplashLogo: 1', 'm_ShowUnitySplashLogo: 0', ps_content)
    with open(proj_settings, 'w', encoding='utf-8') as f:
        f.write(ps_content)

    print(f"Optimized {len(modified)} texture meta files.")
    print("Disabled Unity Splash Screen Logo in ProjectSettings.asset.")

if __name__ == "__main__":
    main()
