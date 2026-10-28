#!/usr/bin/env python3
"""
tools/generate_ui_kit_zip.py
HIT ME VN - Professional 2x Retina UI Kit Generator
Zero-bleeding, Pixel-perfect, RGBA Transparent UI Elements
Compatible with TextMeshPro and Unity 9-Slicing Sprite Mode.
"""

import os
import math
import zipfile
import shutil
from PIL import Image, ImageDraw, ImageFilter

SCALE = 4  # 4x supersampling for ultra-crisp anti-aliasing

def hex_to_rgba(hex_str, alpha=255):
    hex_str = hex_str.lstrip('#')
    if len(hex_str) == 6:
        r = int(hex_str[0:2], 16)
        g = int(hex_str[2:4], 16)
        b = int(hex_str[4:6], 16)
        return (r, g, b, alpha)
    elif len(hex_str) == 8:
        r = int(hex_str[0:2], 16)
        g = int(hex_str[2:4], 16)
        b = int(hex_str[4:6], 16)
        a = int(hex_str[6:8], 16)
        return (r, g, b, a)
    raise ValueError(f"Invalid hex color: {hex_str}")

def create_gradient_surface(w, h, top_color, bottom_color):
    """Generates an RGBA image filled with a vertical linear gradient."""
    grad_col = Image.new('RGBA', (1, h))
    r1, g1, b1, a1 = top_color
    r2, g2, b2, a2 = bottom_color
    pixels = []
    for y in range(h):
        t = y / max(1, h - 1)
        r = int(r1 + (r2 - r1) * t)
        g = int(g1 + (g2 - g1) * t)
        b = int(b1 + (b2 - b1) * t)
        a = int(a1 + (a2 - a1) * t)
        pixels.append((r, g, b, a))
    grad_col.putdata(pixels)
    return grad_col.resize((w, h), Image.Resampling.NEAREST)

def render_downsample(hi_res_img, target_w, target_h):
    """Downsamples from 4x supersampled resolution to target resolution with Lanczos."""
    return hi_res_img.resize((target_w, target_h), Image.Resampling.LANCZOS)

# -------------------------------------------------------------
# 1. ui_banner_wood.png (512 x 140)
# -------------------------------------------------------------
def make_banner_wood():
    W, H = 512, 140
    sw, sh = W * SCALE, H * SCALE
    pad = 8 * SCALE
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    
    # Outer bounds of wooden board
    bx0, by0, bx1, by1 = pad + 14 * SCALE, pad + 10 * SCALE, sw - pad - 14 * SCALE, sh - pad - 10 * SCALE
    radius = 20 * SCALE
    
    # 1. Drop shadow beneath wood banner
    shadow_mask = Image.new('L', (sw, sh), 0)
    sdraw = ImageDraw.Draw(shadow_mask)
    sdraw.rounded_rectangle([bx0, by0 + 8 * SCALE, bx1, by1 + 10 * SCALE], radius=radius, fill=160)
    shadow_mask = shadow_mask.filter(ImageFilter.GaussianBlur(10 * SCALE))
    shadow_layer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    shadow_layer.paste((15, 8, 4, 180), (0, 0), shadow_mask)
    img = Image.alpha_composite(img, shadow_layer)
    
    # 2. Outer dark border (#381A0B)
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([bx0, by0, bx1, by1], radius=radius, fill=hex_to_rgba('#381A0B', 255))
    
    # 3. Main wood body with vertical gradient (#8A441D -> #59270D)
    inset = 4 * SCALE
    wood_box = [bx0 + inset, by0 + inset, bx1 - inset, by1 - inset]
    wood_w = int(wood_box[2] - wood_box[0])
    wood_h = int(wood_box[3] - wood_box[1])
    
    wood_grad = create_gradient_surface(wood_w, wood_h, hex_to_rgba('#8A441D'), hex_to_rgba('#59270D'))
    wood_mask = Image.new('L', (wood_w, wood_h), 0)
    wdraw = ImageDraw.Draw(wood_mask)
    wdraw.rounded_rectangle([0, 0, wood_w, wood_h], radius=radius - inset, fill=255)
    
    # Natural organic wood grain lines (soft dark streaks, no white stripes)
    grain_layer = Image.new('RGBA', (wood_w, wood_h), (0, 0, 0, 0))
    gdraw = ImageDraw.Draw(grain_layer)
    grain_y_offsets = [0.20, 0.42, 0.65, 0.82]
    for gy in grain_y_offsets:
        y_pos = int(wood_h * gy)
        gdraw.line([(8 * SCALE, y_pos), (wood_w - 8 * SCALE, y_pos)], fill=(35, 14, 5, 40), width=4 * SCALE)
        # Subtle wave
        gdraw.arc([100 * SCALE, y_pos - 6 * SCALE, 280 * SCALE, y_pos + 6 * SCALE], start=0, end=180, fill=(35, 14, 5, 30), width=3 * SCALE)
        gdraw.arc([300 * SCALE, y_pos - 6 * SCALE, 450 * SCALE, y_pos + 6 * SCALE], start=180, end=360, fill=(35, 14, 5, 30), width=3 * SCALE)
    grain_layer = grain_layer.filter(ImageFilter.GaussianBlur(1.5 * SCALE))
    wood_grad = Image.alpha_composite(wood_grad, grain_layer)
    
    # Top inner rim bevel highlight
    w_top_h = ImageDraw.Draw(wood_grad)
    w_top_h.line([(25 * SCALE, 3 * SCALE), (wood_w - 25 * SCALE, 3 * SCALE)], fill=(255, 215, 170, 70), width=3 * SCALE)
    # Bottom inner shadow
    w_top_h.line([(25 * SCALE, wood_h - 4 * SCALE), (wood_w - 25 * SCALE, wood_h - 4 * SCALE)], fill=(20, 8, 3, 110), width=4 * SCALE)
    
    img.paste(wood_grad, (int(wood_box[0]), int(wood_box[1])), wood_mask)
    
    # 4. Golden Rivets (Corner Studs)
    draw = ImageDraw.Draw(img)
    rivet_offset_x = 36 * SCALE
    rivet_offset_y = 26 * SCALE
    rivets = [
        (bx0 + rivet_offset_x, by0 + rivet_offset_y),
        (bx1 - rivet_offset_x, by0 + rivet_offset_y),
        (bx0 + rivet_offset_x, by1 - rivet_offset_y),
        (bx1 - rivet_offset_x, by1 - rivet_offset_y)
    ]
    r_radius = 7 * SCALE
    for rx, ry in rivets:
        # Shadow
        draw.ellipse([rx - r_radius, ry - r_radius + 2 * SCALE, rx + r_radius, ry + r_radius + 2 * SCALE], fill=(20, 10, 4, 180))
        # Gold body
        draw.ellipse([rx - r_radius, ry - r_radius, rx + r_radius, ry + r_radius], fill=hex_to_rgba('#FBBF24'), outline=hex_to_rgba('#78350F'), width=2 * SCALE)
        # Specular glint
        draw.ellipse([rx - 3 * SCALE, ry - 3 * SCALE, rx - 1 * SCALE, ry - 1 * SCALE], fill=(255, 255, 255, 230))
    
    # 5. Stylized Golden Blossom & Green Leaves at ends
    def draw_rosette(cx, cy):
        petal_r = 10 * SCALE
        for i in range(5):
            angle = i * (2 * math.pi / 5) - math.pi / 2
            px = cx + math.cos(angle) * 9 * SCALE
            py = cy + math.sin(angle) * 9 * SCALE
            draw.ellipse([px - petal_r, py - petal_r, px + petal_r, py + petal_r], fill=hex_to_rgba('#F59E0B'), outline=hex_to_rgba('#78350F'), width=2 * SCALE)
        draw.ellipse([cx - 7 * SCALE, cy - 7 * SCALE, cx + 7 * SCALE, cy + 7 * SCALE], fill=hex_to_rgba('#FEF08A'), outline=hex_to_rgba('#B45309'), width=2 * SCALE)
        draw.ellipse([cx - 2 * SCALE, cy - 2 * SCALE, cx + 2 * SCALE, cy + 2 * SCALE], fill=hex_to_rgba('#D97706'))

    def draw_leaf(lx, ly, angle_deg, size=16 * SCALE):
        rad = math.radians(angle_deg)
        cos_a = math.cos(rad)
        sin_a = math.sin(rad)
        pts = [
            (lx + cos_a * size, ly + sin_a * size),
            (lx - sin_a * (size * 0.45), ly + cos_a * (size * 0.45)),
            (lx - cos_a * (size * 0.7), ly - sin_a * (size * 0.7)),
            (lx + sin_a * (size * 0.45), ly - cos_a * (size * 0.45))
        ]
        draw.polygon(pts, fill=hex_to_rgba('#22C55E'), outline=hex_to_rgba('#14532D'))
        draw.line([(lx - cos_a * (size * 0.5), ly - sin_a * (size * 0.5)), (lx + cos_a * (size * 0.8), ly + sin_a * (size * 0.8))], fill=hex_to_rgba('#166534'), width=2 * SCALE)

    draw_leaf(bx0 + 2 * SCALE, by0 + 8 * SCALE, -135, size=18 * SCALE)
    draw_leaf(bx0 + 10 * SCALE, by0 - 4 * SCALE, -100, size=16 * SCALE)
    draw_rosette(bx0 + 4 * SCALE, (by0 + by1) / 2)
    
    draw_leaf(bx1 - 2 * SCALE, by0 + 8 * SCALE, -45, size=18 * SCALE)
    draw_leaf(bx1 - 10 * SCALE, by0 - 4 * SCALE, -80, size=16 * SCALE)
    draw_rosette(bx1 - 4 * SCALE, (by0 + by1) / 2)
    
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# 2. Chunky 3D Capsule CTA Buttons (Yellow, Red, Blue, Green)
# -------------------------------------------------------------
def make_btn_cta(width, height, face_top, face_bottom, bevel_color, border_color, bevel_h=12):
    sw, sh = width * SCALE, height * SCALE
    pad = 8 * SCALE
    b_h = bevel_h * SCALE
    
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    radius = (sh - 2 * pad) // 2
    
    x0, y0 = pad, pad
    x1, y1 = sw - pad, sh - pad
    
    # 1. Ambient drop shadow
    shadow_mask = Image.new('L', (sw, sh), 0)
    sdraw = ImageDraw.Draw(shadow_mask)
    sdraw.rounded_rectangle([x0, y0 + 6 * SCALE, x1, y1 + 6 * SCALE], radius=radius, fill=180)
    shadow_mask = shadow_mask.filter(ImageFilter.GaussianBlur(8 * SCALE))
    shadow_layer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    shadow_layer.paste((0, 0, 0, 160), (0, 0), shadow_mask)
    img = Image.alpha_composite(img, shadow_layer)
    
    # 2. Bottom 3D Bevel Lip
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([x0, y0 + b_h, x1, y1], radius=radius, fill=bevel_color, outline=border_color, width=4 * SCALE)
    
    # 3. Main Face with continuous smooth vertical gradient
    face_y1 = y1 - b_h
    face_w = x1 - x0
    face_h = face_y1 - y0
    
    face_grad = create_gradient_surface(face_w, face_h, face_top, face_bottom)
    face_mask = Image.new('L', (face_w, face_h), 0)
    fdraw = ImageDraw.Draw(face_mask)
    fdraw.rounded_rectangle([0, 0, face_w, face_h], radius=radius, fill=255)
    
    # 4. Smooth glossy top highlight: an alpha gradient fading downward without sharp line
    gloss_h = int(face_h * 0.45)
    gloss_grad = create_gradient_surface(face_w, gloss_h, (255, 255, 255, 90), (255, 255, 255, 0))
    face_grad.paste(gloss_grad, (0, 0), gloss_grad)
    
    # Inner rim highlight (subtle top arc)
    fdraw_grad = ImageDraw.Draw(face_grad)
    fdraw_grad.line([(radius, 2 * SCALE), (face_w - radius, 2 * SCALE)], fill=(255, 255, 255, 120), width=2 * SCALE)
    
    # Paste face onto main image
    img.paste(face_grad, (x0, y0), face_mask)
    
    # 5. Crisp Face Outline
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([x0, y0, x1, face_y1], radius=radius, outline=border_color, width=4 * SCALE)
    
    return render_downsample(img, width, height)

# -------------------------------------------------------------
# 3. ui_panel_dark_gold.png (512 x 512 - 9-Slice Uniform)
# -------------------------------------------------------------
def make_panel_dark_gold():
    W, H = 512, 512
    sw, sh = W * SCALE, H * SCALE
    pad = 8 * SCALE
    radius = 28 * SCALE
    
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, pad, sw - pad, sh - pad
    
    # Drop shadow
    smask = Image.new('L', (sw, sh), 0)
    sdraw = ImageDraw.Draw(smask)
    sdraw.rounded_rectangle([x0, y0 + 6 * SCALE, x1, y1 + 8 * SCALE], radius=radius, fill=160)
    smask = smask.filter(ImageFilter.GaussianBlur(8 * SCALE))
    slayer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    slayer.paste((0, 0, 0, 180), (0, 0), smask)
    img = Image.alpha_composite(img, slayer)
    
    draw = ImageDraw.Draw(img)
    # Dark outer border (#451A03)
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=hex_to_rgba('#0F172A', 230), outline=hex_to_rgba('#451A03'), width=5 * SCALE)
    
    # Polished Gold Inner Border (#FBBF24)
    draw.rounded_rectangle([x0 + 2 * SCALE, y0 + 2 * SCALE, x1 - 2 * SCALE, y1 - 2 * SCALE], radius=radius - 2 * SCALE, outline=hex_to_rgba('#FBBF24', 255), width=3 * SCALE)
    
    # Subtle inner dark gold glow line (#92400E)
    draw.rounded_rectangle([x0 + 5 * SCALE, y0 + 5 * SCALE, x1 - 5 * SCALE, y1 - 5 * SCALE], radius=radius - 5 * SCALE, outline=hex_to_rgba('#92400E', 120), width=2 * SCALE)
    
    # Corner rivets at 4 corners
    corner_offset = 20 * SCALE
    rivet_r = 4 * SCALE
    corners = [
        (x0 + corner_offset, y0 + corner_offset),
        (x1 - corner_offset, y0 + corner_offset),
        (x0 + corner_offset, y1 - corner_offset),
        (x1 - corner_offset, y1 - corner_offset)
    ]
    for cx, cy in corners:
        draw.ellipse([cx - rivet_r, cy - rivet_r, cx + rivet_r, cy + rivet_r], fill=hex_to_rgba('#FDE047'), outline=hex_to_rgba('#78350F'), width=1 * SCALE)
    
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# 4. Character / Weapon Card (Normal & Selected) (256 x 320)
# -------------------------------------------------------------
def make_card_char(is_selected=False):
    W, H = 256, 320
    sw, sh = W * SCALE, H * SCALE
    pad = 8 * SCALE
    radius = 24 * SCALE
    
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, pad, sw - pad, sh - pad
    
    if is_selected:
        glow_mask = Image.new('L', (sw, sh), 0)
        gdraw = ImageDraw.Draw(glow_mask)
        gdraw.rounded_rectangle([x0 - 2 * SCALE, y0 - 2 * SCALE, x1 + 2 * SCALE, y1 + 2 * SCALE], radius=radius + 2 * SCALE, fill=220)
        glow_mask = glow_mask.filter(ImageFilter.GaussianBlur(10 * SCALE))
        glow_layer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
        glow_layer.paste(hex_to_rgba('#06B6D4', 160), (0, 0), glow_mask)
        img = Image.alpha_composite(img, glow_layer)
    else:
        smask = Image.new('L', (sw, sh), 0)
        sdraw = ImageDraw.Draw(smask)
        sdraw.rounded_rectangle([x0, y0 + 6 * SCALE, x1, y1 + 6 * SCALE], radius=radius, fill=160)
        smask = smask.filter(ImageFilter.GaussianBlur(6 * SCALE))
        slayer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
        slayer.paste((0, 0, 0, 160), (0, 0), smask)
        img = Image.alpha_composite(img, slayer)
        
    draw = ImageDraw.Draw(img)
    bg_color = hex_to_rgba('#081D33', 250) if is_selected else hex_to_rgba('#0F172A', 245)
    border_color = hex_to_rgba('#22D3EE') if is_selected else hex_to_rgba('#F59E0B')
    border_w = 5 * SCALE if is_selected else 3 * SCALE
    
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=bg_color, outline=border_color, width=border_w)
    
    inner_c = hex_to_rgba('#A5F3FC', 90) if is_selected else hex_to_rgba('#FEF08A', 60)
    draw.rounded_rectangle([x0 + 4 * SCALE, y0 + 4 * SCALE, x1 - 4 * SCALE, y1 - 4 * SCALE], radius=radius - 4 * SCALE, outline=inner_c, width=2 * SCALE)
    
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# 5. ui_input_box_cyan.png (480 x 100)
# -------------------------------------------------------------
def make_input_box_cyan():
    W, H = 480, 100
    sw, sh = W * SCALE, H * SCALE
    pad = 8 * SCALE
    radius = 18 * SCALE
    
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, pad, sw - pad, sh - pad
    
    # Outer cyan aura
    glow_mask = Image.new('L', (sw, sh), 0)
    gdraw = ImageDraw.Draw(glow_mask)
    gdraw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=180)
    glow_mask = glow_mask.filter(ImageFilter.GaussianBlur(6 * SCALE))
    glow_layer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    glow_layer.paste(hex_to_rgba('#06B6D4', 100), (0, 0), glow_mask)
    img = Image.alpha_composite(img, glow_layer)
    
    # Base Box with Masked Inner Top Shadow
    box_w = x1 - x0
    box_h = y1 - y0
    box_surface = Image.new('RGBA', (box_w, box_h), hex_to_rgba('#09131C', 255))
    
    # Top inner shadow inside the box surface
    shadow_grad = create_gradient_surface(box_w, 14 * SCALE, (0, 0, 0, 120), (0, 0, 0, 0))
    box_surface.paste(shadow_grad, (0, 0), shadow_grad)
    
    # Mask box to rounded rectangle
    box_mask = Image.new('L', (box_w, box_h), 0)
    bmdraw = ImageDraw.Draw(box_mask)
    bmdraw.rounded_rectangle([0, 0, box_w, box_h], radius=radius, fill=255)
    img.paste(box_surface, (x0, y0), box_mask)
    
    # Crisp cyan outline (#22D3EE)
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, outline=hex_to_rgba('#22D3EE', 255), width=3 * SCALE)
    
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# 6. Leaderboard Rows (Normal & Highlighted) (600 x 96)
# -------------------------------------------------------------
def make_row_leaderboard(is_me=False):
    W, H = 600, 96
    sw, sh = W * SCALE, H * SCALE
    pad = 8 * SCALE
    radius = 18 * SCALE
    
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, pad, sw - pad, sh - pad
    
    if is_me:
        glow_mask = Image.new('L', (sw, sh), 0)
        gdraw = ImageDraw.Draw(glow_mask)
        gdraw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=200)
        glow_mask = glow_mask.filter(ImageFilter.GaussianBlur(8 * SCALE))
        glow_layer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
        glow_layer.paste(hex_to_rgba('#06B6D4', 130), (0, 0), glow_mask)
        img = Image.alpha_composite(img, glow_layer)
        
    draw = ImageDraw.Draw(img)
    bg_color = hex_to_rgba('#08243C', 245) if is_me else hex_to_rgba('#0F1A2C', 225)
    border_color = hex_to_rgba('#22D3EE') if is_me else hex_to_rgba('#334155')
    border_w = 3 * SCALE if is_me else 2 * SCALE
    
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=bg_color, outline=border_color, width=border_w)
    
    if is_me:
        draw.rounded_rectangle([x0 + 3 * SCALE, y0 + 3 * SCALE, x1 - 3 * SCALE, y1 - 3 * SCALE], radius=radius - 3 * SCALE, outline=hex_to_rgba('#67E8F9', 80), width=1 * SCALE)
        
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# 7. ui_toggle_on.png & ui_toggle_off.png (160 x 80)
# -------------------------------------------------------------
def make_toggle(is_on=True):
    W, H = 160, 80
    sw, sh = W * SCALE, H * SCALE
    pad = 8 * SCALE
    
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, pad, sw - pad, sh - pad
    radius = (y1 - y0) // 2
    
    if is_on:
        track_grad = create_gradient_surface(x1 - x0, y1 - y0, hex_to_rgba('#22D3EE'), hex_to_rgba('#0891B2'))
        border_c = hex_to_rgba('#0E3D52')
    else:
        track_grad = create_gradient_surface(x1 - x0, y1 - y0, hex_to_rgba('#334155'), hex_to_rgba('#1E293B'))
        border_c = hex_to_rgba('#0F172A')
        
    track_mask = Image.new('L', (x1 - x0, y1 - y0), 0)
    tmdraw = ImageDraw.Draw(track_mask)
    tmdraw.rounded_rectangle([0, 0, x1 - x0, y1 - y0], radius=radius, fill=255)
    img.paste(track_grad, (x0, y0), track_mask)
    
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, outline=border_c, width=3 * SCALE)
    
    # Circular 3D Knob (Diameter ~48px)
    knob_d = 48 * SCALE
    knob_r = knob_d // 2
    knob_cx = (x1 - knob_r - 4 * SCALE) if is_on else (x0 + knob_r + 4 * SCALE)
    knob_cy = (y0 + y1) // 2
    
    kshadow_mask = Image.new('L', (sw, sh), 0)
    ksdraw = ImageDraw.Draw(kshadow_mask)
    ksdraw.ellipse([knob_cx - knob_r, knob_cy - knob_r + 3 * SCALE, knob_cx + knob_r, knob_cy + knob_r + 3 * SCALE], fill=160)
    kshadow_mask = kshadow_mask.filter(ImageFilter.GaussianBlur(4 * SCALE))
    kslayer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    kslayer.paste((0, 0, 0, 160), (0, 0), kshadow_mask)
    img = Image.alpha_composite(img, kslayer)
    
    draw = ImageDraw.Draw(img)
    draw.ellipse([knob_cx - knob_r, knob_cy - knob_r, knob_cx + knob_r, knob_cy + knob_r], fill=hex_to_rgba('#CBD5E1'), outline=hex_to_rgba('#94A3B8'), width=2 * SCALE)
    draw.ellipse([knob_cx - knob_r + 2 * SCALE, knob_cy - knob_r + 1 * SCALE, knob_cx + knob_r - 2 * SCALE, knob_cy + knob_r - 3 * SCALE], fill=hex_to_rgba('#FFFFFF'))
    
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# 8. Slider Track & Modular Parts (480 x 40)
# -------------------------------------------------------------
def make_slider_track():
    W, H = 480, 40
    sw, sh = W * SCALE, H * SCALE
    pad = 6 * SCALE
    
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, (sh - 22 * SCALE) // 2, sw - pad, (sh + 22 * SCALE) // 2
    radius = (y1 - y0) // 2
    
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=hex_to_rgba('#0B1320', 255), outline=hex_to_rgba('#1E293B', 255), width=2 * SCALE)
    
    split_x = int(x0 + (x1 - x0) * 0.55)
    cyan_w = split_x - x0
    cyan_h = y1 - y0
    c_grad = create_gradient_surface(cyan_w, cyan_h, hex_to_rgba('#22D3EE'), hex_to_rgba('#0891B2'))
    
    c_mask = Image.new('L', (cyan_w, cyan_h), 0)
    cm_draw = ImageDraw.Draw(c_mask)
    cm_draw.rounded_rectangle([0, 0, cyan_w, cyan_h], radius=radius, fill=255)
    img.paste(c_grad, (x0, y0), c_mask)
    
    k_r = 13 * SCALE
    ky = (y0 + y1) // 2
    draw.ellipse([split_x - k_r, ky - k_r + 2 * SCALE, split_x + k_r, ky + k_r + 2 * SCALE], fill=(0, 0, 0, 140))
    draw.ellipse([split_x - k_r, ky - k_r, split_x + k_r, ky + k_r], fill=hex_to_rgba('#FFFFFF'), outline=hex_to_rgba('#94A3B8'), width=2 * SCALE)
    
    return render_downsample(img, W, H)

def make_slider_track_empty():
    W, H = 480, 32
    sw, sh = W * SCALE, H * SCALE
    pad = 4 * SCALE
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, pad, sw - pad, sh - pad
    radius = (y1 - y0) // 2
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=hex_to_rgba('#0B1320', 255), outline=hex_to_rgba('#1E293B', 255), width=2 * SCALE)
    return render_downsample(img, W, H)

def make_slider_track_fill():
    W, H = 480, 32
    sw, sh = W * SCALE, H * SCALE
    pad = 4 * SCALE
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    x0, y0, x1, y1 = pad, pad, sw - pad, sh - pad
    radius = (y1 - y0) // 2
    grad = create_gradient_surface(x1 - x0, y1 - y0, hex_to_rgba('#22D3EE'), hex_to_rgba('#0891B2'))
    mask = Image.new('L', (x1 - x0, y1 - y0), 0)
    mdraw = ImageDraw.Draw(mask)
    mdraw.rounded_rectangle([0, 0, x1 - x0, y1 - y0], radius=radius, fill=255)
    img.paste(grad, (x0, y0), mask)
    return render_downsample(img, W, H)

def make_slider_knob():
    W, H = 56, 56
    sw, sh = W * SCALE, H * SCALE
    pad = 4 * SCALE
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    cx, cy = sw // 2, sh // 2
    r = (sw - 2 * pad) // 2
    
    smask = Image.new('L', (sw, sh), 0)
    sdraw = ImageDraw.Draw(smask)
    sdraw.ellipse([cx - r, cy - r + 3 * SCALE, cx + r, cy + r + 3 * SCALE], fill=160)
    smask = smask.filter(ImageFilter.GaussianBlur(3 * SCALE))
    slayer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    slayer.paste((0, 0, 0, 150), (0, 0), smask)
    img = Image.alpha_composite(img, slayer)
    
    draw = ImageDraw.Draw(img)
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=hex_to_rgba('#FFFFFF'), outline=hex_to_rgba('#94A3B8'), width=2 * SCALE)
    draw.ellipse([cx - 4 * SCALE, cy - 4 * SCALE, cx + 4 * SCALE, cy + 4 * SCALE], fill=hex_to_rgba('#06B6D4'))
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# 9. ui_btn_circle_back.png (128 x 128)
# -------------------------------------------------------------
def make_btn_circle_back():
    W, H = 128, 128
    sw, sh = W * SCALE, H * SCALE
    pad = 8 * SCALE
    img = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    cx, cy = sw // 2, sh // 2
    r = (sw - 2 * pad) // 2
    
    smask = Image.new('L', (sw, sh), 0)
    sdraw = ImageDraw.Draw(smask)
    sdraw.ellipse([cx - r, cy - r + 4 * SCALE, cx + r, cy + r + 4 * SCALE], fill=160)
    smask = smask.filter(ImageFilter.GaussianBlur(6 * SCALE))
    slayer = Image.new('RGBA', (sw, sh), (0, 0, 0, 0))
    slayer.paste((0, 0, 0, 180), (0, 0), smask)
    img = Image.alpha_composite(img, slayer)
    
    draw = ImageDraw.Draw(img)
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=hex_to_rgba('#0F172A', 255), outline=hex_to_rgba('#451A03'), width=5 * SCALE)
    draw.ellipse([cx - r + 2 * SCALE, cy - r + 2 * SCALE, cx + r - 2 * SCALE, cy + r - 2 * SCALE], outline=hex_to_rgba('#FBBF24', 255), width=4 * SCALE)
    
    arrow_w = 6 * SCALE
    p_tip = (cx - 10 * SCALE, cy)
    p_top = (cx + 10 * SCALE, cy - 18 * SCALE)
    p_bot = (cx + 10 * SCALE, cy + 18 * SCALE)
    
    draw.line([p_top, p_tip], fill=(0, 0, 0, 140), width=arrow_w + 2 * SCALE)
    draw.line([p_tip, p_bot], fill=(0, 0, 0, 140), width=arrow_w + 2 * SCALE)
    draw.line([p_top, p_tip], fill=hex_to_rgba('#FFFFFF'), width=arrow_w)
    draw.line([p_tip, p_bot], fill=hex_to_rgba('#FFFFFF'), width=arrow_w)
    draw.ellipse([p_tip[0] - arrow_w//2, p_tip[1] - arrow_w//2, p_tip[0] + arrow_w//2, p_tip[1] + arrow_w//2], fill=hex_to_rgba('#FFFFFF'))
    draw.ellipse([p_top[0] - arrow_w//2, p_top[1] - arrow_w//2, p_top[0] + arrow_w//2, p_top[1] + arrow_w//2], fill=hex_to_rgba('#FFFFFF'))
    draw.ellipse([p_bot[0] - arrow_w//2, p_bot[1] - arrow_w//2, p_bot[0] + arrow_w//2, p_bot[1] + arrow_w//2], fill=hex_to_rgba('#FFFFFF'))
    
    return render_downsample(img, W, H)

# -------------------------------------------------------------
# MAIN GENERATION & PACKAGING PIPELINE
# -------------------------------------------------------------
def main():
    out_dir = r"D:\HITME GAME\ASSET HITME FINAL\UI_KIT_EXTRACTED"
    zip_path = r"D:\HITME GAME\ASSET HITME FINAL\HITME_UI_KIT_CLEAN.zip"
    os.makedirs(out_dir, exist_ok=True)
    
    print(f"[UI Kit Generator] Starting generation at 4x supersampling...")
    
    elements = {}
    
    # 1. Banner
    print(" - Rendering ui_banner_wood.png (512x140)...")
    elements['ui_banner_wood.png'] = make_banner_wood()
    
    # 2. CTA Buttons
    print(" - Rendering ui_btn_cta_yellow.png (512x128)...")
    elements['ui_btn_cta_yellow.png'] = make_btn_cta(
        512, 128,
        face_top=hex_to_rgba('#FFE642'),
        face_bottom=hex_to_rgba('#F59E0B'),
        bevel_color=hex_to_rgba('#B45309'),
        border_color=hex_to_rgba('#3B2308'),
        bevel_h=12
    )
    
    print(" - Rendering ui_btn_cta_red.png (512x128)...")
    elements['ui_btn_cta_red.png'] = make_btn_cta(
        512, 128,
        face_top=hex_to_rgba('#EF4444'),
        face_bottom=hex_to_rgba('#B91C1C'),
        bevel_color=hex_to_rgba('#7F1D1D'),
        border_color=hex_to_rgba('#450A0A'),
        bevel_h=12
    )
    
    print(" - Rendering ui_btn_cta_blue.png (360x110)...")
    elements['ui_btn_cta_blue.png'] = make_btn_cta(
        360, 110,
        face_top=hex_to_rgba('#3B82F6'),
        face_bottom=hex_to_rgba('#1D4ED8'),
        bevel_color=hex_to_rgba('#1E40AF'),
        border_color=hex_to_rgba('#0F172A'),
        bevel_h=10
    )
    
    print(" - Rendering ui_btn_cta_green.png (360x110)...")
    elements['ui_btn_cta_green.png'] = make_btn_cta(
        360, 110,
        face_top=hex_to_rgba('#10B981'),
        face_bottom=hex_to_rgba('#059669'),
        bevel_color=hex_to_rgba('#047857'),
        border_color=hex_to_rgba('#022C22'),
        bevel_h=10
    )
    
    # 3. Panel
    print(" - Rendering ui_panel_dark_gold.png (512x512)...")
    elements['ui_panel_dark_gold.png'] = make_panel_dark_gold()
    
    # 4. Character Cards
    print(" - Rendering ui_card_char_normal.png (256x320)...")
    elements['ui_card_char_normal.png'] = make_card_char(is_selected=False)
    
    print(" - Rendering ui_card_char_selected.png (256x320)...")
    elements['ui_card_char_selected.png'] = make_card_char(is_selected=True)
    
    # 5. Input Box
    print(" - Rendering ui_input_box_cyan.png (480x100)...")
    elements['ui_input_box_cyan.png'] = make_input_box_cyan()
    
    # 6. Leaderboard Rows
    print(" - Rendering ui_row_leaderboard.png (600x96)...")
    elements['ui_row_leaderboard.png'] = make_row_leaderboard(is_me=False)
    
    print(" - Rendering ui_row_leaderboard_me.png (600x96)...")
    elements['ui_row_leaderboard_me.png'] = make_row_leaderboard(is_me=True)
    
    # 7. Toggles
    print(" - Rendering ui_toggle_on.png (160x80)...")
    elements['ui_toggle_on.png'] = make_toggle(is_on=True)
    
    print(" - Rendering ui_toggle_off.png (160x80)...")
    elements['ui_toggle_off.png'] = make_toggle(is_on=False)
    
    # 8. Sliders
    print(" - Rendering ui_slider_track.png (480x40)...")
    elements['ui_slider_track.png'] = make_slider_track()
    elements['ui_slider_track_empty.png'] = make_slider_track_empty()
    elements['ui_slider_track_fill.png'] = make_slider_track_fill()
    elements['ui_slider_knob.png'] = make_slider_knob()
    
    # 9. Circle Back Button
    print(" - Rendering ui_btn_circle_back.png (128x128)...")
    elements['ui_btn_circle_back.png'] = make_btn_circle_back()
    
    # Save all PNGs
    print(f"\n[UI Kit Generator] Saving {len(elements)} PNG files to {out_dir}...")
    saved_paths = []
    for filename, img in elements.items():
        file_path = os.path.join(out_dir, filename)
        img.save(file_path, 'PNG', optimize=True)
        saved_paths.append(file_path)
        print(f"   [OK] {filename} ({img.width}x{img.height})")
        
    # Compress into ZIP
    print(f"\n[UI Kit Generator] Packaging into {zip_path}...")
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for file_path in saved_paths:
            arcname = os.path.basename(file_path)
            zipf.write(file_path, arcname=arcname)
    print(f"   [OK] Successfully created ZIP: {zip_path} ({os.path.getsize(zip_path)} bytes)")

    # Copy to Unity Project Assets in worktrees
    unity_kit_dirs = [
        r"D:\HITME GAME\worktrees\ag-visual-ui\Assets\HITME\Art\UI\Kit",
        r"D:\HITME GAME\Assets\HITME\Art\UI\Kit"
    ]
    for ukd in unity_kit_dirs:
        try:
            os.makedirs(ukd, exist_ok=True)
            for file_path in saved_paths:
                dest = os.path.join(ukd, os.path.basename(file_path))
                shutil.copy2(file_path, dest)
            print(f"   [OK] Copied {len(saved_paths)} sprites to Unity folder: {ukd}")
        except Exception as e:
            print(f"   [Notice] Skipped copying to {ukd}: {e}")

    print("\n[UI Kit Generator] ALL TASKS COMPLETED SUCCESSFULLY!")

if __name__ == '__main__':
    main()
