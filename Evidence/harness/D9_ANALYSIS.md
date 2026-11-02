# BÁO CÁO PHÂN TÍCH DUNG LƯỢNG BUNDLE WEBGL (D9 ANALYSIS)

- **Bản build phân tích:** `WebGL_Release_20261102_188344e`
- **Mục tiêu:** Giảm tổng dung lượng truyền tải từ **9.87 MB** xuống **≤ 6.00 MB**
- **Phương pháp đo:** Trích xuất từ Unity Build Report (`unity-build.log`) và kích thước tệp tĩnh trong `tools/deploy_web/dist/Build`.

---

## 1. Phân tích kích thước các thành phần trong `dist/Build`

| Tệp | Định dạng nén | Kích thước sau nén (Brotli) | Kích thước gốc (Uncompressed) | Tỉ lệ nén |
| :--- | :---: | :---: | :---: | :---: |
| `WebGL_Release_20261102_188344e.data.br` | Brotli (.br) | **5,620,086 bytes** (5.36 MB) | 12,892,402 bytes (12.29 MB) | 43.6% |
| `WebGL_Release_20261102_188344e.wasm.br` | Brotli (.br) | **4,613,397 bytes** (4.40 MB) | 26,960,188 bytes (25.71 MB) | 17.1% |
| `WebGL_Release_20261102_188344e.framework.js.br` | Brotli (.br) | **65,972 bytes** (0.06 MB) | 329,396 bytes (0.31 MB) | 20.0% |
| `WebGL_Release_20261102_188344e.loader.js` | Không nén | **27,914 bytes** (0.03 MB) | 27,914 bytes (0.03 MB) | 100.0% |
| **Tổng cộng bundle truyền tải** | — | **10,327,369 bytes (9.85 MB)** | **40,209,900 bytes (38.35 MB)** | **25.7%** |

---

## 2. Phân loại tài nguyên người dùng (User Assets) theo danh mục

Từ Unity Build Report:
- **Textures / Sprites:** 9.4 MB (86.2% tổng tài nguyên asset)
- **Sounds / Audio:** 709.9 KB (6.4%)
- **Other Assets (Fonts / SDF / Resources):** 721.8 KB (6.5%)
- **Shaders:** 20.2 KB (0.2%)
- **File headers:** 63.9 KB (0.6%)
- **Tổng User Assets:** 10.9 MB (Uncompressed)

---

## 3. Top 20 Asset nặng nhất trong bản build

| STT | Đường dẫn Asset | Dung lượng gốc | % Dung lượng | Loại tài nguyên | Ghi chú & Đề xuất tối ưu |
| :---: | :--- | :---: | :---: | :---: | :--- |
| 1 | `Assets/Sprites/ARENA.png` | 2.0 MB | 20.3% | Texture | Sân thi đấu 2.5D; cần bật nén WebGL (ASTC/Crunch/DXT) hoặc giảm max size |
| 2 | `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset` | 1.0 MB | 10.4% | Font SDF Atlas | Atlas SDF dự phòng của TextMeshPro nằm trong thư mục Resources |
| 3 | `Assets/Fonts/BeVietnamPro-Bold SDF.asset` | 1.0 MB | 10.4% | Font SDF Atlas | Atlas SDF tiếng Việt chính; atlas 512x512 thay vì 1024x1024 |
| 4 | `Resources/unity_builtin_extra` | 519.8 KB | 5.2% | Engine Resources | Shader và vật liệu mặc định của Unity WebGL |
| 5 | `Assets/Sprites/cau_be_lang_hit.png` | 386.6 KB | 3.8% | Sprite | Sprite hành động nhân vật; cần nén WebGL & bỏ mipmap |
| 6 | `Assets/Sprites/cau_be_lang_throw_release.png` | 386.4 KB | 3.8% | Sprite | Sprite hành động nhân vật; cần nén WebGL & bỏ mipmap |
| 7 | `Assets/TextMesh Pro/Fonts/LiberationSans.ttf` | 344.5 KB | 3.4% | TrueType Font | Font TTF gốc thừa (game dùng SDF font); loại khỏi build |
| 8 | `Assets/Sprites/anh_non_la_throw_release.png` | 341.9 KB | 3.4% | Sprite | Sprite hành động nhân vật; cần nén WebGL |
| 9 | `Assets/Audio/bgm_menu_loop.wav` | 337.9 KB | 3.3% | Audio Clip | BGM menu; nén Vorbis chất lượng 0.25, mono |
| 10 | `Assets/Sprites/cau_be_lang_aim.png` | 308.3 KB | 3.1% | Sprite | Sprite hành động nhân vật; cần nén WebGL |
| 11 | `Assets/Sprites/anh_non_la_aim.png` | 294.7 KB | 2.9% | Sprite | Sprite hành động nhân vật; cần nén WebGL |
| 12 | `Assets/Sprites/cau_be_lang_idle.png` | 293.6 KB | 2.9% | Sprite | Sprite nhân vật chính; cần nén WebGL |
| 13 | `Assets/Sprites/dep_projectile.png` | 272.7 KB | 2.7% | Sprite | Vũ khí dép ném; cần nén WebGL |
| 14 | `Assets/Sprites/anh_non_la_idle.png` | 264.1 KB | 2.6% | Sprite | Sprite nhân vật chính; cần nén WebGL |
| 15 | `Assets/Audio/bgm_match_loop.wav` | 254.0 KB | 2.5% | Audio Clip | BGM trận đấu; nén Vorbis chất lượng 0.25, mono |
| 16 | `Assets/Sprites/anh_non_la_hit.png` | 240.2 KB | 2.4% | Sprite | Sprite hành động nhân vật; cần nén WebGL |
| 17 | `Assets/Scenes/Graybox.unity` | 238.2 KB | 2.4% | Scene Data | Scene dữ liệu trận đấu |
| 18 | `Assets/Sprites/anh_non_la_defeat.png` | 225.4 KB | 2.2% | Sprite | Sprite hành động nhân vật; cần nén WebGL |
| 19 | `Assets/Sprites/mai_hong_idle.png` | 209.1 KB | 2.1% | Sprite | Sprite nhân vật; cần nén WebGL |
| 20 | `Assets/Art/UI/Kit/ui_row_leaderboard.png` | 164.8 KB | 1.6% | UI Sprite | Sprite UI bảng xếp hạng; chuyển 9-slice nhỏ |

---

## 4. Kế hoạch tối ưu hóa để đạt mục tiêu ≤ 6.00 MB

1. **Nhóm A – Tối ưu Texture & Sprites (Tiềm năng giảm ~2.5 - 3.2 MB):**
   - Nén texture override cho WebGL (`textureCompression = TextureImporterCompression.Compressed`, định dạng DXT/ASTC/Crunch).
   - Tắt Mipmap cho toàn bộ Sprite 2D và UI (`generateMipMaps = false`).
   - Tắt `isReadable` (Read/Write Enabled) trên toàn bộ texture để giảm VRAM và Wasm memory heap.
   - Resize texture `ARENA.png` (2.0 MB) về kích thước chuẩn di động (1024 hoặc 2048 nén) với Crunch/ASTC.
2. **Nhóm B – Tối ưu Build Engine & Wasm Stripping (Tiềm năng giảm ~1.0 - 1.5 MB):**
   - Chuyển `PlayerSettings.SetManagedStrippingLevel(..., ManagedStrippingLevel.High)`.
   - Bật `PlayerSettings.stripEngineCode = true`.
   - Di chuyển hoặc loại bỏ font TTF thừa `LiberationSans.ttf` (344.5 KB) không nằm trong build.
3. **Nhóm C – Tối ưu Audio (Tiềm năng giảm ~250 - 400 KB):**
   - Đảm bảo toàn bộ BGM nén Vorbis 70 kbps (quality 0.25), load type `CompressedInMemory`.
   - Toàn bộ SFX nén Vorbis mono 50 kbps, load type `DecompressOnLoad`.
