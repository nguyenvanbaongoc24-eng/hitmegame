import os
import glob
import re

AUDIO_DIR = r"unity-client/Assets/Audio"

meta_files = glob.glob(os.path.join(AUDIO_DIR, "*.wav.meta"))

for mf in meta_files:
    with open(mf, "r", encoding="utf-8") as f:
        content = f.read()

    is_bgm = "bgm_" in mf or "amb_" in mf
    target_quality = 0.15 if is_bgm else 0.10
    load_type = 1 if is_bgm else 0 # 1 = CompressedInMemory, 0 = DecompressOnLoad
    sample_rate_setting = 2 if is_bgm else 0
    sample_rate_override = 22050 if is_bgm else 44100

    content = re.sub(r'quality:\s*[\d\.]+', f'quality: {target_quality}', content)
    content = re.sub(r'loadType:\s*\d+', f'loadType: {load_type}', content)
    content = re.sub(r'sampleRateSetting:\s*\d+', f'sampleRateSetting: {sample_rate_setting}', content)
    content = re.sub(r'sampleRateOverride:\s*\d+', f'sampleRateOverride: {sample_rate_override}', content)
    content = re.sub(r'compressionFormat:\s*\d+', 'compressionFormat: 1', content)
    content = re.sub(r'forceToMono:\s*\d+', 'forceToMono: 1', content)

    # Set platformSettingOverrides for WebGL
    webgl_block = f"""  platformSettingOverrides:
    WebGL:
      serializedVersion: 2
      loadType: {load_type}
      sampleRateSetting: {sample_rate_setting}
      sampleRateOverride: {sample_rate_override}
      compressionFormat: 1
      quality: {target_quality}
      conversionMode: 0"""
    
    pattern = r'  platformSettingOverrides:(\s+WebGL:[\s\S]+?conversionMode: \d+)?'
    content = re.sub(pattern, webgl_block, content)

    with open(mf, "w", encoding="utf-8") as f:
        f.write(content)

print(f"Successfully tuned {len(meta_files)} audio files with 22kHz BGM and lightweight SFX.")
