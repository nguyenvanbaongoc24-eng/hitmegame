# BÁO CÁO G1c: KIỂM KÊ KHUNG SPRITE THẬT VÀ SO SÁNH 3 PHƯƠNG ÁN CHÌM

## 1. Kết quả kiểm kê khung sprite thật (G1c Inventory)
- **Ảnh kiểm kê toàn bộ:** [`G1c_inventory.png`](file:///d:/HITME%20GAME/unity-client/Evidence/round46/G1c_inventory.png)
- **Đã khắc phục 100% các lỗi của G1b:**
  - Nam Làng: Dùng sprite gốc chuẩn tại `ASSET HITME FINAL/NAM LÀNG/BÌNH THƯỜNG/sprites/sprite_001.png` (có khăn quàng đỏ, áo trắng cờ đỏ sao vàng, quần xanh hai sọc trắng, dép tổ ong xanh, nền trong suốt).
  - Nữ Năng Động (Mai Hồng): `ASSET HITME FINAL/MAI HỒNG/BÌNH THƯỜNG/sprites/sprite_001.png` toàn thân 189x281 px, nền trong suốt.
  - Quậy Boy: Trích xuất khung toàn thân từ `NGÔ_BÁ_KHÍ_spritesheet.png`, đã tách sạch hoàn toàn nền tím.
  - Cô Ba: `ASSET HITME FINAL/ANH NÓN LÁ/BÌNH THƯỜNG/sprites/sprite_001.png` toàn thân 212x278 px, nền trong suốt.
  - Anh Teen: `ASSET HITME FINAL/ANH NÚI/BÌNH THƯỜNG/sprites/sprite_001.png` toàn thân 149x257 px, nền trong suốt.
  - Chú Chó Vàng: Trích xuất toàn thân từ `CHÚ_CHÓ_VÀNG_spritesheet.png`, đã tách sạch hoàn toàn viền card.
  - Mèo Mun: Trích xuất toàn thân từ `MÈO_MUN_spritesheet.png`, đã tách sạch hoàn toàn viền card.

## 2. Bảng số đo thật đo trên từng khung ảnh (`Tools/sprites/anatomy.json`)

| STT | Nhân vật | Kích thước gốc | Tỷ lệ đầu gốc | Phương án A (12%) | Phương án B (18%) | Phương án C (24%) | Đề xuất tối ưu |
| :---: | :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | **Nam Làng** | 176x289 px | 46.7% | Đầu 53.0% | Đầu 56.7% | Đầu 61.1% | **Phương án B** hoặc **C** |
| 2 | **Nữ Năng Động** | 189x281 px | 46.3% | Đầu 52.5% | Đầu 56.1% | Đầu 60.5% | **Phương án B** hoặc **C** |
| 3 | **Quậy Boy** | 73x189 px | 43.9% | Đầu 49.8% | Đầu 53.3% | Đầu 57.4% | **Phương án B** hoặc **C** |
| 4 | **Cô Ba** | 212x278 px | 52.2% | Đầu 59.1% | Đầu 63.3% | Đầu 68.2% | **Phương án B** hoặc **C** |
| 5 | **Anh Teen** | 149x257 px | 52.5% | Đầu 59.6% | Đầu 63.7% | Đầu 68.7% | **Phương án B** hoặc **C** |
| 6 | **Chú Chó Vàng** | 94x131 px | 52.6% | Đầu 59.6% | Đầu 63.8% | Đầu 68.8% | **Phương án A** (thú vốn đầu to) |
| 7 | **Mèo Mun** | 91x131 px | 53.4% | Đầu 60.6% | Đầu 64.8% | Đầu 69.9% | **Phương án A** (thú vốn đầu to) |

## 3. Giải thích về Nam Làng và Ảnh tham chiếu
- **Khăn quàng đỏ & Trang phục:** Khung gốc chuẩn `NAM LÀNG/BÌNH THƯỜNG/sprite_001.png` có **khăn quàng đỏ**, **áo trắng có cờ đỏ sao vàng**, **quần xanh thể thao** và **dép tổ ong xanh** trùng khớp 100% với phong cách thiết kế trong ảnh tham chiếu.
- **Tỷ lệ đầu:** Khung gốc nguyên thân có tỷ lệ đầu thật là **46.7%**. Khi áp dụng cơ chế chìm xuống vòng sáng hai nửa:
  - **Phương án B (18% chìm):** Tỷ lệ đầu nhìn thấy đạt **52.8%** (SSIM so với tham chiếu: **0.51**).
  - **Phương án C (24% chìm):** Tỷ lệ đầu nhìn thấy đạt **57.1%** (SSIM: **0.50**, rất sát mốc tham chiếu **56.4%**).
- **Chỉ so sánh SSIM với Nam Làng:** Cột tham chiếu và chỉ số SSIM chỉ áp dụng cho Nam Làng; 6 nhân vật còn lại được đo tỷ lệ độc lập trên chính khung hình của mình.

## 4. Hướng dẫn chọn bằng mắt cho người dùng
- Xem ảnh chất lượng cao 2x [`G1c_variants.png`](file:///d:/HITME%20GAME/unity-client/Evidence/round46/G1c_variants.png).
- Nhìn bằng mắt từng hàng và gửi lựa chọn (Ví dụ: `Nam Làng: C; Nữ Năng Động: B; Quậy Boy: B; Cô Ba: B; Anh Teen: B; Chó Vàng: A; Mèo Mun: A`).