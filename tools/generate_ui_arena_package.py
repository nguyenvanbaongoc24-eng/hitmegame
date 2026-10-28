import os
import zipfile
import json
import shutil
from PIL import Image, ImageDraw, ImageFilter

base_dir = r"d:\HITME GAME"
out_dir = os.path.join(base_dir, "HITME_UI_ARENA_PACKAGE")
ui_dir = os.path.join(out_dir, "UI_Elements_PNG")
arena_dir = os.path.join(out_dir, "Arena_Layers_Cropped")
unity_dir = os.path.join(out_dir, "Unity_Editor_Scripts")

os.makedirs(ui_dir, exist_ok=True)
os.makedirs(arena_dir, exist_ok=True)
os.makedirs(unity_dir, exist_ok=True)

def create_rounded_rect_button(filename, size, top_color, bottom_color, border_color, border_width=6, radius=48, bevel_color=None, glow_color=None):
    w, h = size
    pad = 16
    img = Image.new("RGBA", (w + pad*2, h + pad*2), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    if glow_color:
        glow_layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
        gdraw = ImageDraw.Draw(glow_layer)
        gdraw.rounded_rectangle([pad-4, pad-4, pad+w+4, pad+h+4], radius=radius+4, fill=glow_color)
        glow_layer = glow_layer.filter(ImageFilter.GaussianBlur(8))
        img = Image.alpha_composite(img, glow_layer)
        draw = ImageDraw.Draw(img)

    # Outer border
    draw.rounded_rectangle([pad, pad, pad+w, pad+h], radius=radius, fill=border_color)

    # Bevel bottom
    inner_top = pad + border_width
    inner_left = pad + border_width
    inner_right = pad + w - border_width
    inner_bottom = pad + h - border_width
    
    if bevel_color:
        draw.rounded_rectangle([inner_left, inner_top, inner_right, inner_bottom], radius=max(8, radius-border_width), fill=bevel_color)
        body_bottom = inner_bottom - 10
    else:
        body_bottom = inner_bottom

    # Gradient body
    grad = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    g_draw = ImageDraw.Draw(grad)
    body_h = max(1, (body_bottom - pad) - border_width)
    for y in range(border_width, body_bottom - pad):
        t = (y - border_width) / float(body_h)
        r = int(top_color[0] * (1 - t) + bottom_color[0] * t)
        g = int(top_color[1] * (1 - t) + bottom_color[1] * t)
        b = int(top_color[2] * (1 - t) + bottom_color[2] * t)
        a = int(top_color[3] * (1 - t) + bottom_color[3] * t)
        g_draw.line([(0, y), (w, y)], fill=(r, g, b, a))

    mask = Image.new("L", (w, h), 0)
    m_draw = ImageDraw.Draw(mask)
    m_draw.rounded_rectangle([border_width, border_width, w - border_width, body_bottom - pad], radius=max(8, radius-border_width), fill=255)
    
    body_layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
    body_layer.paste(grad, (pad, pad), mask)
    img = Image.alpha_composite(img, body_layer)
    draw = ImageDraw.Draw(img)

    # Top gloss highlight
    if bevel_color:
        gloss = Image.new("RGBA", img.size, (0, 0, 0, 0))
        gl_draw = ImageDraw.Draw(gloss)
        gl_draw.rounded_rectangle([inner_left + 12, inner_top + 6, inner_right - 12, inner_top + int(body_h * 0.42)], radius=max(4, (radius-border_width)//2), fill=(255, 255, 255, 70))
        img = Image.alpha_composite(img, gloss)

    img.save(os.path.join(ui_dir, filename))
    print(f"Generated UI: {filename}")

# 1. Generate clean 9-sliceable UI PNGs
create_rounded_rect_button("ui_btn_cta_yellow.png", (520, 128), (255, 232, 68, 255), (245, 158, 11, 255), (42, 26, 8, 255), border_width=6, radius=64, bevel_color=(180, 83, 9, 255))
create_rounded_rect_button("ui_btn_cta_blue.png", (420, 116), (59, 130, 246, 255), (29, 78, 216, 255), (15, 23, 42, 255), border_width=5, radius=58, bevel_color=(30, 58, 138, 255))
create_rounded_rect_button("ui_btn_cta_green.png", (420, 116), (16, 185, 129, 255), (4, 120, 87, 255), (15, 23, 42, 255), border_width=5, radius=58, bevel_color=(6, 78, 59, 255))
create_rounded_rect_button("ui_btn_cta_red.png", (520, 116), (239, 68, 68, 255), (185, 28, 28, 255), (45, 10, 10, 255), border_width=5, radius=58, bevel_color=(127, 29, 29, 255))
create_rounded_rect_button("ui_panel_dark_gold.png", (512, 512), (17, 26, 38, 242), (11, 18, 28, 248), (245, 185, 45, 255), border_width=4, radius=36)
create_rounded_rect_button("ui_card_slot_normal.png", (256, 280), (24, 34, 48, 235), (15, 23, 34, 245), (148, 163, 184, 120), border_width=3, radius=28)
create_rounded_rect_button("ui_card_slot_selected_cyan.png", (256, 280), (20, 48, 68, 245), (12, 32, 48, 250), (34, 211, 238, 255), border_width=5, radius=28, glow_color=(6, 182, 212, 160))
create_rounded_rect_button("ui_hud_pill_dark.png", (420, 76), (13, 21, 32, 230), (9, 15, 24, 240), (245, 185, 45, 180), border_width=3, radius=38)
create_rounded_rect_button("ui_chat_row_normal.png", (380, 88), (22, 32, 46, 235), (16, 24, 36, 245), (71, 85, 105, 160), border_width=3, radius=22)
create_rounded_rect_button("ui_chat_row_selected.png", (380, 88), (18, 45, 64, 245), (12, 32, 48, 250), (34, 211, 238, 255), border_width=4, radius=22, glow_color=(6, 182, 212, 150))
create_rounded_rect_button("ui_btn_circle_dark.png", (128, 128), (22, 32, 46, 240), (13, 20, 30, 250), (148, 163, 184, 180), border_width=4, radius=64)
create_rounded_rect_button("ui_btn_circle_cyan_active.png", (128, 128), (20, 48, 68, 245), (13, 28, 42, 250), (34, 211, 238, 255), border_width=5, radius=64, glow_color=(6, 182, 212, 160))
create_rounded_rect_button("ui_timer_circle_gold.png", (148, 148), (255, 224, 51, 255), (234, 152, 12, 255), (28, 19, 8, 255), border_width=6, radius=74)

# Speech Bubble ("Ném hay lắm!", Emote Like trên đầu nhân vật)
speech = Image.new("RGBA", (280, 120), (0, 0, 0, 0))
sdraw = ImageDraw.Draw(speech)
sdraw.rounded_rectangle([8, 8, 272, 92], radius=24, fill=(255, 255, 255, 255), outline=(20, 30, 48, 255), width=4)
sdraw.polygon([(125, 89), (155, 89), (140, 114)], fill=(255, 255, 255, 255), outline=(20, 30, 48, 255))
sdraw.line([(127, 89), (153, 89)], fill=(255, 255, 255, 255), width=5)
speech.save(os.path.join(ui_dir, "ui_speech_bubble.png"))
print("Generated UI: ui_speech_bubble.png")

# Player Foot Ring Cyan
ring = Image.new("RGBA", (160, 160), (0, 0, 0, 0))
rdraw = ImageDraw.Draw(ring)
rdraw.ellipse([12, 36, 148, 124], outline=(34, 211, 238, 255), width=8)
ring.save(os.path.join(ui_dir, "ui_player_foot_ring_cyan.png"))
print("Generated UI: ui_player_foot_ring_cyan.png")

# 2. Extract Arena Slices from ARENA.png and ARENA ASSET.png
arena_path = os.path.join(base_dir, "ARENA.png")
if not os.path.exists(arena_path):
    # Fallback to unity client sprites
    alt_arena = os.path.join(base_dir, "unity-client", "Assets", "Sprites", "arena_background.png")
    if os.path.exists(alt_arena):
        arena_path = alt_arena

if os.path.exists(arena_path):
    arena_full = Image.open(arena_path).convert("RGBA")
    aw, ah = arena_full.size
    arena_full.save(os.path.join(arena_dir, "arena_full_clean_reference.png"))
    arena_full.crop((0, 0, aw, int(ah * 0.19))).save(os.path.join(arena_dir, "layer_02_top_gate_header.png"))
    arena_full.crop((int(aw * 0.05), int(ah * 0.175), int(aw * 0.95), int(ah * 0.875))).save(os.path.join(arena_dir, "layer_04_playable_sand_court.png"))
    arena_full.crop((0, int(ah * 0.855), aw, ah)).save(os.path.join(arena_dir, "layer_06_bottom_steps_foreground.png"))
    print("Extracted Arena layers from ARENA.png")

arena_asset_path = os.path.join(base_dir, "ARENA ASSET.png")
if os.path.exists(arena_asset_path):
    sheet = Image.open(arena_asset_path).convert("RGBA")
    sw, sh = sheet.size
    sheet.crop((int(sw * 0.342), int(sh * 0.43), int(sw * 0.495), int(sh * 0.56))).save(os.path.join(arena_dir, "tile_sand_seamless.png"))
    sheet.crop((int(sw * 0.50), int(sh * 0.43), int(sw * 0.652), int(sh * 0.56))).save(os.path.join(arena_dir, "tile_arena_border_corner.png"))
    sheet.crop((int(sw * 0.656), int(sh * 0.43), int(sw * 0.825), int(sh * 0.56))).save(os.path.join(arena_dir, "decal_central_lotus_emblem.png"))
    sheet.crop((int(sw * 0.336), int(sh * 0.61), int(sw * 0.99), int(sh * 0.825))).save(os.path.join(arena_dir, "props_sheet_flags_lanterns_plants.png"))
    print("Extracted Arena slices from ARENA ASSET.png")
else:
    # Check pre-extracted assets in ASSET BỐI CẢNH/BỐI CẢNH
    boi_canh = os.path.join(base_dir, "ASSET HITME FINAL", "ASSET BỐI CẢNH", "BỐI CẢNH")
    if os.path.exists(boi_canh):
        tile_src = os.path.join(boi_canh, "HITME_A07_Stone_Tile_Floor.png")
        border_src = os.path.join(boi_canh, "HITME_A08_Arena_Border.png")
        lotus_src = os.path.join(boi_canh, "HITME_A09_Lotus_Emblem.png")
        if os.path.exists(tile_src):
            shutil.copy2(tile_src, os.path.join(arena_dir, "tile_sand_seamless.png"))
        if os.path.exists(border_src):
            shutil.copy2(border_src, os.path.join(arena_dir, "tile_arena_border_corner.png"))
        if os.path.exists(lotus_src):
            shutil.copy2(lotus_src, os.path.join(arena_dir, "decal_central_lotus_emblem.png"))
        print("Supplied arena tiles & decals from pre-extracted high quality assets.")

# 3. Write Unity Editor Auto-9-Slice & Prefab Spec
unity_script = """using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class HitMeUIPackageImporter : AssetPostprocessor
{
    private static readonly Dictionary<string, Vector4> SliceBorders = new Dictionary<string, Vector4>
    {
        // Vector4(left, bottom, right, top)
        { "ui_btn_cta_yellow", new Vector4(68, 28, 68, 28) },
        { "ui_btn_cta_blue", new Vector4(60, 24, 60, 24) },
        { "ui_btn_cta_green", new Vector4(60, 24, 60, 24) },
        { "ui_btn_cta_red", new Vector4(60, 24, 60, 24) },
        { "ui_panel_dark_gold", new Vector4(44, 44, 44, 44) },
        { "ui_card_slot_normal", new Vector4(36, 36, 36, 36) },
        { "ui_card_slot_selected_cyan", new Vector4(36, 36, 36, 36) },
        { "ui_hud_pill_dark", new Vector4(42, 20, 42, 20) },
        { "ui_chat_row_normal", new Vector4(28, 20, 28, 20) },
        { "ui_chat_row_selected", new Vector4(28, 20, 28, 20) },
        { "ui_speech_bubble", new Vector4(32, 36, 32, 28) }
    };

    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("HITME") && !assetPath.Contains("UI_Elements_PNG")) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100f;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;

        foreach (var kv in SliceBorders)
        {
            if (assetPath.Contains(kv.Key))
            {
                settings.spriteBorder = kv.Value;
                break;
            }
        }
        importer.SetTextureSettings(settings);
    }
}
"""
with open(os.path.join(unity_dir, "HitMeUIPackageImporter.cs"), "w", encoding="utf-8") as f:
    f.write(unity_script)
print("Written HitMeUIPackageImporter.cs")

manifest_json = {
    "canvasReferenceResolution": {"width": 390, "height": 844, "matchWidthOrHeight": 0.5},
    "battleHUDLayout": {
        "topBar": {
            "roundPill": {"anchor": "TopLeft", "size": [110, 36], "sprite": "ui_hud_pill_dark.png"},
            "timerCircle": {"anchor": "TopCenter", "size": [68, 68], "sprite": "ui_timer_circle_gold.png"},
            "settingsBtn": {"anchor": "TopRight", "size": [44, 44], "sprite": "ui_btn_circle_dark.png"}
        },
        "playerRosterRow": {
            "columns": 6,
            "slotSize": [56, 78],
            "spacing": 5,
            "localPlayerSprite": "ui_card_slot_selected_cyan.png",
            "botPlayerSprite": "ui_card_slot_normal.png"
        },
        "statusBanner": {
            "text": "CHỌN HƯỚNG NÉM | Đối thủ đang ẩn",
            "size": [270, 30],
            "sprite": "ui_hud_pill_dark.png"
        },
        "arenaViewportRatio": 0.72,
        "bottomBar": {
            "height": 84,
            "chatBtn": {"size": [48, 48], "sprite": "ui_btn_circle_dark.png", "activeSprite": "ui_btn_circle_cyan_active.png"},
            "weaponSlot": {"size": [58, 64], "sprite": "ui_card_slot_normal.png", "activeSprite": "ui_card_slot_selected_cyan.png"},
            "readyCTA": {"size": [168, 56], "sprite": "ui_btn_cta_yellow.png", "text": "SẴN SÀNG"},
            "emoteBtn": {"size": [48, 48], "sprite": "ui_btn_circle_dark.png", "activeSprite": "ui_btn_circle_cyan_active.png"}
        },
        "bottomDrawers": {
            "containerSprite": "ui_panel_dark_gold.png",
            "weaponPicker": {"columns": 3, "cardSize": [104, 128], "items": ["Dép", "Chảo", "Vợt pickleball"]},
            "emotePicker": {"columns": 5, "rows": 2, "cardSize": [64, 72], "items": ["Cười", "Khóc", "Giận", "Bất ngờ", "Nháy mắt", "Like", "Chào", "Vỗ tay", "Yêu thích", "Chiến thắng"]},
            "quickChatPicker": {"columns": 2, "rows": 3, "rowSize": [168, 42], "items": ["Xin chào!", "Sẵn sàng!", "Ném hay lắm!", "Suýt trúng!", "Chơi tiếp nhé!", "Cảm ơn!"]}
        },
        "inMatchSettingsModal": {
            "containerSprite": "ui_panel_dark_gold.png",
            "size": [346, 460],
            "sliders": ["Nhạc nền (50%)", "Âm thanh (80%)"],
            "toggles": ["Rung", "Hiệu ứng biểu cảm"],
            "continueCTA": {"sprite": "ui_btn_cta_yellow.png", "text": "TIẾP TỤC"},
            "leaveCTA": {"sprite": "ui_btn_cta_red.png", "text": "RỜI TRẬN"}
        }
    }
}

with open(os.path.join(out_dir, "AG_UI_ARENA_LAYOUT_SPEC.json"), "w", encoding="utf-8") as f:
    json.dump(manifest_json, f, ensure_ascii=False, indent=2)
print("Written AG_UI_ARENA_LAYOUT_SPEC.json")

zip_filename = os.path.join(base_dir, "HITME_AG_UI_ARENA_BUNDLE.zip")
with zipfile.ZipFile(zip_filename, "w", zipfile.ZIP_DEFLATED) as zf:
    for root, _, files in os.walk(out_dir):
        for file in files:
            full_path = os.path.join(root, file)
            rel_path = os.path.relpath(full_path, out_dir)
            zf.write(full_path, rel_path)

print("Created ZIP archive:", zip_filename)
