# DANH SÁCH TÀI NGUYÊN CẦN BỔ SUNG CHO CỬA HÀNG VŨ KHÍ VIỆT NAM (MISSING_ASSETS.md)

**Ngày lập:** 02/11/2026 — Phase 4  
**Tham chiếu dữ liệu:** `Assets/Data/Items.json`

Theo nguyên tắc của dự án: **Không dùng ảnh vẽ tạm / placeholder nhòe**, các vật phẩm chưa có sprite chính thức trong kho `asset/` được ghi rõ danh sách dưới đây để artist thiết kế bổ sung.

---

## 1. Bảng Kiểm Tra Sprite Vũ Khí

| Mã ID | Tên vật phẩm | Độ hiếm | Icon Sprite | Projectile Sprite | Trạng thái hiện tại |
|---|---|---|---|---|---|
| `dep_to_ong` | Dép tổ ong huyền thoại | Common | `dep_icon.png` | `dep_projectile.png` | **ĐÃ CÓ ĐẦY ĐỦ** (kho asset chính) |
| `chao_ran` | Chảo rán chống dính | Common | `chao_icon.png` | `chao_projectile.png` | **ĐÃ CÓ ĐẦY ĐỦ** (kho asset chính) |
| `vot_pickleball` | Vợt Pickleball thời thượng | Rare | `pickleball_icon.png` | `pickleball_projectile.png` | **ĐÃ CÓ ĐẦY ĐỦ** (kho asset chính) |
| `coc_bia_hoi` | Cốc bia hơi thủy tinh xanh | Rare | `coc_bia_icon.png` | `coc_bia_projectile.png` | **THIẾU** (Cần sprite icon 128×128 & projectile 64×64) |
| `xe_dap_phuong_hoang` | Xe đạp Phượng Hoàng cổ | Epic | `xe_dap_icon.png` | `xe_dap_projectile.png` | **THIẾU** (Cần sprite icon 128×128 & projectile 96×64) |
| `xe_may_honda_cub` | Xe máy Honda Cub 50cc | Legendary | `xe_may_icon.png` | `xe_may_projectile.png` | **THIẾU** (Cần sprite icon 128×128 & projectile 128×80) |
| `chu_cho_vang` | Xương vàng của Cún may mắn | Mythic | `cho_vang_icon.png` | `cho_vang_projectile.png` | **THIẾU** (Có sprite pet nhân vật `chu_cho_vang_idle.png`, thiếu sprite vũ khí ném) |

---

## 2. Danh Sách Hiệu Ứng Âm Thanh & VFX Cần Bổ Sung

1. `vfx_beer_splash` + `sfx_hit_glass_clink` (Hiệu ứng bọt bia sủi tung tóe & tiếng cốc bia cụng)
2. `vfx_bell_ring` + `sfx_hit_bike_crash` (Hiệu ứng sóng âm chuông kính coong xe đạp)
3. `vfx_exhaust_smoke_burst` + `sfx_hit_engine_roarm` (Hiệu ứng khói pô xả mù mịt & tiếng nổ máy Cub 50)
4. `vfx_golden_paw_sparkle` + `sfx_hit_dog_bark` (Hiệu ứng vệt chân cún vàng lấp lánh & tiếng sủa gâu gâu)
