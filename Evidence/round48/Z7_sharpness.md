# Z7 Sharpness & Character Distortion Audit Report

## 1. Canvas Buffer Resolution & devicePixelRatio
- **Target Screen:** iPhone 15 Pro (393 × 852 CSS pt, devicePixelRatio = 3.0).
- **Physical Native Resolution:** 1179 × 2556 px.
- **WebGL Canvas Sizing:** `matchWebGLToCanvasSize: true`, rendering 1:1 buffer at full DPR (1179 × 2556 px).
- **Camera Configuration:** Orthographic size = 7.2f, height = 14.4 world units, viewport width = 6.64 units (aspect 0.461). Zero black voids.

## 2. Character Sprite Texture Import Settings
All active gameplay character sprites are configured with uncompressed, high-fidelity settings:
- **Pixels Per Unit (PPU):** 150
- **Filter Mode:** Bilinear
- **Max Texture Size:** 2048
- **Texture Compression:** None (0) / Crunched: False
- **Sprite Dimensions:**
  - `cau_be_lang_idle.png`: 218 × 343 px (1.45u × 2.29u @ 150 PPU)
  - `mai_hong_idle.png`: 189 × 281 px (1.26u × 1.87u @ 150 PPU)
  - `anh_non_la_idle.png`: 225 × 299 px (1.50u × 1.99u @ 150 PPU)
  - `anh_nui_idle.png`: 149 × 257 px (0.99u × 1.71u @ 150 PPU)
  - `co_nang_the_thao_idle.png`: 91 × 181 px (Thấp độ phân giải -> Đã khóa "Sắp ra mắt")

## 3. Root Cause Analysis of Blurriness & Distortion During Aim
1. **Low-res Sprite Swap:** `PlayAnimation("Aim")` swapped `_body.sprite` to `cau_be_lang_aim.png` (233 × 337 px with different anatomy), producing visual artifacts and sudden loss of quality.
2. **Non-uniform Scale Distortion:** In `PlayerView.Update()`, Aim breathing applied non-uniform scaling:
   `new Vector3(_baseBodyScale.x * (1f + breath * 0.5f), _baseBodyScale.y * (1f + breath), 1f)`
   Scale Y grew at double the rate of Scale X, stretching the body and elongating legs dynamically during aiming!
   Idle breathing also applied `breath * 0.50f` on X vs `breath` on Y.

## 4. Fix Applied
1. **Preserve Idle Sprite on Aim:** `PlayAnimation("Aim")` keeps `_defaultSprites[idx]`.
2. **Uniform Scaling (X = Y):**
   `_body.transform.localScale = _baseBodyScale * (1f + breath);` in both Aim and Idle states.
   Aspect ratio distortion is strictly 0.0%.
3. **Sorting Order Hierarchy:**
   - Shadow: -1
   - Foot Ring (Back): 0
   - Aim Cone & Trajectory Dots: 1 (Under body)
   - Character Body: 2
   - Foot Ring (Front): 3
   - Aim Reticle & Weapon Slipper: 4
4. **Slipper Weapon Flight Scale:** Adjusted to `0.28f` (≈ 0.6 tile ≈ 88-92 px at 1179 width).
