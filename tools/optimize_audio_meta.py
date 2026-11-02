import os
import glob
import re

AUDIO_DIR = r"unity-client/Assets/Audio"

meta_files = glob.glob(os.path.join(AUDIO_DIR, "*.wav.meta"))
print(f"Found {len(meta_files)} audio meta files.")

for mf in meta_files:
    with open(mf, "r", encoding="utf-8") as f:
        content = f.read()

    is_bgm = "bgm_" in mf or "amb_" in mf
    target_quality = 0.20 if is_bgm else 0.15
    load_type = 1 if is_bgm else 0 # 1 = CompressedInMemory, 0 = DecompressOnLoad

    # Update quality
    content = re.sub(r'quality:\s*[\d\.]+', f'quality: {target_quality}', content)
    # Update loadType
    content = re.sub(r'loadType:\s*\d+', f'loadType: {load_type}', content)
    # Ensure compressionFormat is 1 (Vorbis)
    content = re.sub(r'compressionFormat:\s*\d+', 'compressionFormat: 1', content)
    # Ensure forceToMono is 1
    content = re.sub(r'forceToMono:\s*\d+', 'forceToMono: 1', content)

    # Set platformSettingOverrides for WebGL
    webgl_block = f"""  platformSettingOverrides:
    WebGL:
      serializedVersion: 2
      loadType: {load_type}
      sampleRateSetting: 0
      sampleRateOverride: 44100
      compressionFormat: 1
      quality: {target_quality}
      conversionMode: 0"""
    
    if "platformSettingOverrides: {}" in content:
        content = content.replace("  platformSettingOverrides: {}", webgl_block)

    with open(mf, "w", encoding="utf-8") as f:
        f.write(content)

print(f"Successfully updated {len(meta_files)} audio meta files.")
