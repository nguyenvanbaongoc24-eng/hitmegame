import math
from pathlib import Path
from PIL import Image, ImageDraw

KIT_DIR = Path("unity-client/Assets/Art/UI/Kit")
KIT_DIR.mkdir(parents=True, exist_ok=True)

SCALE = 4  # 4x supersampling for ultra smooth anti-aliased icons (256x256 -> 64x64)
SIZE = 64
CANVAS_SIZE = SIZE * SCALE
CENTER = CANVAS_SIZE / 2

def make_gear():
    im = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    
    # Outer teeth (8 teeth)
    teeth = 8
    outer_r = 105
    inner_r = 82
    center_hole_r = 38
    
    # Draw teeth as rotated polygons
    for i in range(teeth):
        angle = i * (2 * math.pi / teeth)
        cos_a = math.cos(angle)
        sin_a = math.sin(angle)
        # tooth rectangle centered at angle
        w = 26
        h = 112
        # Tooth points
        p1 = (CENTER + cos_a * outer_r - sin_a * (w/2), CENTER + sin_a * outer_r + cos_a * (w/2))
        p2 = (CENTER + cos_a * outer_r + sin_a * (w/2), CENTER + sin_a * outer_r - cos_a * (w/2))
        p3 = (CENTER + cos_a * (inner_r-5) + sin_a * (w/2 + 6), CENTER + sin_a * (inner_r-5) - cos_a * (w/2 + 6))
        p4 = (CENTER + cos_a * (inner_r-5) - sin_a * (w/2 + 6), CENTER + sin_a * (inner_r-5) + cos_a * (w/2 + 6))
        draw.polygon([p1, p2, p3, p4], fill=(255, 230, 140, 255))
        
    # Main ring body
    draw.ellipse([CENTER - inner_r, CENTER - inner_r, CENTER + inner_r, CENTER + inner_r], fill=(255, 215, 90, 255))
    # Gear border / highlight
    draw.ellipse([CENTER - inner_r + 8, CENTER - inner_r + 8, CENTER + inner_r - 8, CENTER + inner_r - 8], fill=(235, 175, 45, 255))
    # Inner hole (transparent)
    draw.ellipse([CENTER - center_hole_r, CENTER - center_hole_r, CENTER + center_hole_r, CENTER + center_hole_r], fill=(0, 0, 0, 0))

    return im.resize((SIZE, SIZE), Image.Resampling.LANCZOS)

def make_coin():
    im = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    r = 100
    # Outer gold rim
    draw.ellipse([CENTER - r, CENTER - r, CENTER + r, CENTER + r], fill=(255, 195, 30, 255))
    # Inner gold face
    r2 = 82
    draw.ellipse([CENTER - r2, CENTER - r2, CENTER + r2, CENTER + r2], fill=(255, 230, 100, 255))
    # Coin star or emblem in center
    draw.ellipse([CENTER - 36, CENTER - 36, CENTER + 36, CENTER + 36], fill=(240, 170, 20, 255))
    return im.resize((SIZE, SIZE), Image.Resampling.LANCZOS)

def make_gem():
    im = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    # Diamond gem polygon
    # Top vertices: flat top, facets
    top_w = 70
    mid_w = 110
    top_y = CENTER - 70
    mid_y = CENTER - 15
    bot_y = CENTER + 95
    
    pts_outer = [
        (CENTER - top_w, top_y),
        (CENTER + top_w, top_y),
        (CENTER + mid_w, mid_y),
        (CENTER, bot_y),
        (CENTER - mid_w, mid_y)
    ]
    draw.polygon(pts_outer, fill=(0, 220, 255, 255))
    # Facet highlights
    draw.polygon([(CENTER - top_w, top_y), (CENTER + top_w, top_y), (CENTER + 35, mid_y), (CENTER - 35, mid_y)], fill=(180, 245, 255, 255))
    draw.polygon([(CENTER, bot_y), (CENTER - 35, mid_y), (CENTER + 35, mid_y)], fill=(0, 180, 235, 255))
    return im.resize((SIZE, SIZE), Image.Resampling.LANCZOS)

def make_crown():
    im = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    # 3-point crown
    base_w = 90
    base_y1 = CENTER + 40
    base_y2 = CENTER + 75
    # Crown base bar
    draw.rectangle([CENTER - base_w, base_y1, CENTER + base_w, base_y2], fill=(255, 180, 0, 255))
    # Crown points
    pts = [
        (CENTER - base_w, base_y1),
        (CENTER - base_w, CENTER - 50),
        (CENTER - 45, CENTER + 10),
        (CENTER, CENTER - 75),
        (CENTER + 45, CENTER + 10),
        (CENTER + base_w, CENTER - 50),
        (CENTER + base_w, base_y1)
    ]
    draw.polygon(pts, fill=(255, 215, 0, 255))
    # Jewels on 3 peaks
    for peak in [(CENTER - base_w, CENTER - 50), (CENTER, CENTER - 75), (CENTER + base_w, CENTER - 50)]:
        draw.ellipse([peak[0]-14, peak[1]-14, peak[0]+14, peak[1]+14], fill=(255, 60, 60, 255))
    return im.resize((SIZE, SIZE), Image.Resampling.LANCZOS)

def make_check():
    im = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    # Circle green background
    r = 100
    draw.ellipse([CENTER - r, CENTER - r, CENTER + r, CENTER + r], fill=(35, 195, 95, 255))
    # White checkmark
    pts = [
        (CENTER - 55, CENTER + 5),
        (CENTER - 15, CENTER + 45),
        (CENTER + 55, CENTER - 45)
    ]
    draw.line(pts, fill=(255, 255, 255, 255), width=28, joint="curve")
    return im.resize((SIZE, SIZE), Image.Resampling.LANCZOS)

icons = {
    "ui_icon_gear.png": make_gear(),
    "ui_icon_coin.png": make_coin(),
    "ui_icon_gem.png": make_gem(),
    "ui_icon_crown.png": make_crown(),
    "ui_icon_check.png": make_check()
}

meta_template = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationMethod: 0
  spriteTessellationDetail: -1
"""

import uuid

for name, im in icons.items():
    png_path = KIT_DIR / name
    im.save(png_path)
    meta_path = KIT_DIR / (name + ".meta")
    if not meta_path.exists():
        guid_str = uuid.uuid5(uuid.NAMESPACE_DNS, name).hex
        meta_content = meta_template.format(guid=guid_str)
        meta_path.write_text(meta_content, encoding="utf-8")
    print(f"Generated {name} ({SIZE}x{SIZE})")

print("All icons successfully generated in Assets/Art/UI/Kit/")
