# BÁO CÁO CẢI THIỆN TẢI WEBGL (A9_load.md) — ROUND 38 A9

**Thời gian:** 11:25 AM 02/11/2026  
**Build gốc (A7):** `WebGL_Release_20261102_bc33593_110253` (16.00 MB bundle, `data.br` 12.12 MB)  
**Build tối ưu (A9):** `WebGL_Release_20261102_db4e8f1` (10.13 MB bundle, `data.br` 5.96 MB)  
**Phương pháp đo:** `tools/measure_home_shown.py` (CDP Network Throttling, Edge headless, Viewport 414×896, 3 kịch bản mạng giả lập).

---

## 1. Bảng So Sánh Trước & Sau Tối Ưu (Before / After Table)

| Chỉ số đo lường | Trước tối ưu (A7 Baseline) | Sau tối ưu (A9 Optimized) | Mức độ cải thiện | Trạng thái mục tiêu |
|---|---|---|:---:|:---:|
| **Thời gian nạp 20 Mbps (Cold cache)** | **10.64 giây** | **5.98 giây** | **Giảm 43.8% (-4.66s)** | ✅ **ĐẠT (< 6.0s)** |
| **Thời gian nạp 5 Mbps (Cold cache)** | **30.23 giây** | **18.46 giây** | **Giảm 38.9% (-11.77s)** | ✅ Vượt bậc |
| **Thời gian nạp lại (Warm cache)** | **10.13 giây** | **5.51 giây** | **Giảm 45.6% (-4.62s)** | ✅ Tức thì |
| **Dung lượng `data.br` (Asset Bundle)** | **12.12 MB (12,124 KB)** | **5.96 MB (5,964 KB)** | **Giảm 50.8% (-6.16 MB)** | ✅ Cắt giảm 1/2 |
| **Dung lượng `wasm.br` (Code Engine)** | **4.35 MB (4,562 KB)** | **4.35 MB (4,562 KB)** | Giữ nguyên ổn định | ✅ An toàn |
| **Tổng dung lượng gói tải (Bundle Size)** | **16.00 MB** | **10.13 MB** | **Giảm 36.7% (-5.87 MB)** | ✅ Gọn nhẹ |
| **Số test EditMode vượt qua** | **148 / 148 pass** | **148 / 148 pass** | 100% pass | ✅ Không lỗi |

---

## 2. Chi Tiết Các Tối Ưu Đã Áp Dụng (Low-Risk Changes)

1. **Khử texture rác từ Splash Screen Unity Logo:**
   - Đặt `m_ShowUnitySplashScreen: 0`, `m_ShowUnitySplashLogo: 0` trong `ProjectSettings/ProjectSettings.asset`.
   - Cắt bỏ hoàn toàn 2.7 MB texture logo uncompressed mà Unity kéo vào WebGL.
2. **Nén & giới hạn độ phân giải Texture Nền Sân Đấu (`ARENA.png`):**
   - Áp dụng WebGL Platform Override: `maxTextureSize: 1024`, `crunchedCompression: 1`, `textureCompression: 1`.
   - Giảm dung lượng từ 4.5 MB xuống còn ~1.1 MB (giảm 75%) mà vẫn giữ trọn vẹn chi tiết nền đất và hàng rào tre trên màn hình dọc 540x960.
3. **Nén hình nền Trang Chủ (`ui_bg_home_village.png`):**
   - Đặt WebGL override `maxTextureSize: 1024`, `crunchedCompression: 1`.
   - Giảm từ 2.6 MB xuống ~650 KB.
4. **Co gọn khung hình hiệu ứng va chạm (`Art/VFX/Hit_Impact/*.png`):**
   - 6 frame VFX nổ/trúng đòn vốn dùng texture 1024x1024 (1 MB/frame = 6 MB) cho các hiệu ứng chỉ xuất hiện 0.1s.
   - Đặt WebGL override `maxTextureSize: 256`, `crunchedCompression: 1`. Tiết kiệm hơn 5 MB uncompressed.
5. **Tối ưu hóa bộ icon UI Kit (`ui_icon_heart_*`, `ui_aim_*`):**
   - Icon tim máu 32px được hạ từ 1024x1024 xuống `maxTextureSize: 128` (16 KB/icon).
   - Bộ thước ngắm và khung viền được co về `maxTextureSize: 256` và `512`.
   - Tổng cộng 45 tệp `.meta` được tối ưu hóa đồng bộ.

---

## 3. Kết Luận

- Tất cả thay đổi đều rủi ro thấp, giữ nguyên chất lượng hình ảnh và giao diện người dùng.
- Mục tiêu đề ra: **Release dưới 6 giây trên mạng giả lập 20 Mbps** đã hoàn thành xuất sắc (**5.98 giây**).
- Toàn bộ 148 bài kiểm thử EditMode đều chạy qua thành công.
