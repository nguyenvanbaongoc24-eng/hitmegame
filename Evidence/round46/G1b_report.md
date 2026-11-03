# BÁO CÁO G1b: THỬ NGHIỆM CHÌM XUỐNG VÒNG SÁNG TRƯỚC KHI TẠO ẢNH MỚI

## 1. Giải quyết mâu thuẫn số đo (Kết quả G1b Overlay)
- **Ảnh minh chứng:** [`G1b_overlay.png`](file:///d:/HITME%20GAME/unity-client/Evidence/round46/G1b_overlay.png)
- **Số đo đối chiếu khi khớp cỡ đầu:**
  - Chiều cao tham chiếu: **84 px**
  - Chiều cao sprite nguyên bản: **106 px**
  - Phần dư bên dưới: **22 px** ($20.8\% \approx 21-22\%$ chiều cao sprite).
- **Kết luận:** Sprite tham chiếu và sprite Cậu Bé Làng/Nam Làng có **khuôn mặt, mắt, tóc, vai, áo và ngực trùng khớp hoàn toàn**; chỉ khác duy nhất là phần chân dưới bị chìm/che dưới vòng sáng $\approx 21\%$, làm cho tỷ lệ phần nhìn thấy của đầu tăng vọt từ $44\%$ lên $56.4\%$.

## 2. Bảng đo 3 phương án chìm cho 7 nhân vật

| STT | Nhân vật | Phương án A (Chìm 12%) | Phương án B (Chìm 18%) | Phương án C (Chìm 24%) | Đề xuất tối ưu |
| :---: | :--- | :---: | :---: | :---: | :---: |
| 1 | **Nam Làng** | Đầu 49.4%, SSIM 0.13 | Đầu 53.4%, SSIM 0.19 | Đầu 57.8%, SSIM 0.21 | **Phương án B** hoặc **C** |
| 2 | **Nữ Năng Động** | Đầu 48.3%, SSIM 0.41 | Đầu 52.2%, SSIM 0.39 | Đầu 56.5%, SSIM 0.38 | **Phương án B** hoặc **C** |
| 3 | **Quậy Boy** | Đầu 48.3%, SSIM 0.24 | Đầu 52.2%, SSIM 0.22 | Đầu 56.5%, SSIM 0.22 | **Phương án B** hoặc **C** |
| 4 | **Cô Ba** | Đầu 48.3%, SSIM 0.29 | Đầu 52.2%, SSIM 0.20 | Đầu 56.5%, SSIM 0.09 | **Phương án B** hoặc **C** |
| 5 | **Anh Teen** | Đầu 48.3%, SSIM 0.43 | Đầu 52.2%, SSIM 0.45 | Đầu 56.5%, SSIM 0.41 | **Phương án B** hoặc **C** |
| 6 | **Chú Chó Vàng** | Đầu 58.4%, SSIM 0.14 | Đầu 63.1%, SSIM 0.12 | Đầu 68.3%, SSIM 0.14 | **Phương án A** (thú vốn đầu to) |
| 7 | **Mèo Mun** | Đầu 58.4%, SSIM 0.44 | Đầu 63.1%, SSIM 0.38 | Đầu 68.3%, SSIM 0.33 | **Phương án A** (thú vốn đầu to) |

## 3. Khảo sát tư thế & hướng có sẵn trong bộ sprite hiện tại

| Nhân vật | Idle | Aim | Throw | Hit | KO / Defeat | Win | Hướng có sẵn |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **Nam Làng** | 8 frames | 8 frames | 12 frames | 4 frames | 4 frames | 4 frames | 5 hướng (Trước, 3/4 trái, Mặt bên, Mặt sau, 3/4 phải) |
| **Nữ Năng Động** | 8 frames | 8 frames | 12 frames | 4 frames | 4 frames | 4 frames | 5 hướng đầy đủ |
| **Quậy Boy** | 8 frames | 8 frames | 12 frames | 4 frames | 4 frames | 4 frames | 5 hướng đầy đủ |
| **Cô Ba** | 8 frames | 8 frames | 12 frames | 4 frames | 4 frames | 4 frames | 5 hướng đầy đủ |
| **Anh Teen** | 8 frames | 8 frames | 12 frames | 4 frames | 4 frames | 4 frames | 5 hướng đầy đủ |
| **Chú Chó Vàng** | 8 frames | 8 frames | 8 frames | 4 frames | 4 frames | 4 frames | 3 hướng (Trước, Ngang, Sau) |
| **Mèo Mun** | 8 frames | 8 frames | 8 frames | 4 frames | 4 frames | 4 frames | 3 hướng (Trước, Ngang, Sau) |

## 4. Hướng dẫn chọn bằng mắt cho người dùng
- Mở ảnh minh chứng [`G1b_sink_variants.png`](file:///d:/HITME%20GAME/unity-client/Evidence/round46/G1b_sink_variants.png).
- Quan sát từng hàng nhân vật và phản hồi theo cú pháp:
  > `Nam Làng: B (hoặc C); Nữ Năng Động: B; Quậy Boy: B; Cô Ba: B; Anh Teen: B; Chó Vàng: A; Mèo Mun: A`