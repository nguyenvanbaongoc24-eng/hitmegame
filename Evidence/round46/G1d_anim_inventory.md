# BÁO CÁO KIỂM KÊ HOẠT ẢNH & ĐỘ PHÂN GIẢI G1d

## 1. Ma trận hoạt ảnh có thật trong dự án

| Nhân vật | Idle | Aim | Ném Dép | Ném Chảo | Ném Vợt | Hit | KO | Win | Nguồn & Bản chất |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|---|
| **Nam Làng** | 20 | — | 15 | 12 | 8 | — | — | — | `ASSET HITME FINAL/NAM LÀNG` (Chuỗi frame rời 100% alpha) |
| **Nữ Năng Động** | 16 | — | 20 | 21 | 20 | — | — | — | `ASSET HITME FINAL/MAI HỒNG` (Chuỗi frame rời 100% alpha) |
| **Cô Ba** | 18 | — | 22 | 22 | 23 | — | — | — | `ASSET HITME FINAL/ANH NÓN LÁ` (Chuỗi frame rời 100% alpha) |
| **Anh Teen** | 22 | — | 20 | 21 | 23 | — | — | — | `ASSET HITME FINAL/ANH NÚI` (Chuỗi frame rời 100% alpha) |
| **Quậy Boy** | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | **THIẾU TOÀN BỘ** (Chỉ có sheet concept `NGÔ_BÁ_KHÍ_spritesheet.png`) |
| **Chó Vàng** | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | **THIẾU TOÀN BỘ** (Chỉ có sheet concept `CHÚ_CHÓ_VÀNG_spritesheet.png`) |
| **Mèo Mun** | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | **THIẾU TOÀN BỘ** (Chỉ có sheet concept `MÈO_MUN_spritesheet.png`) |

## 2. Kết luận phần hoạt ảnh còn thiếu

- **4 nhân vật người (Nam Làng, Nữ Năng Động, Cô Ba, Anh Teen):** Đã có đầy đủ Idle thở nhẹ và toàn bộ hoạt ảnh Ném (Dép, Chảo, Vợt) từ 15-23 frame cực mượt. **Thiếu:** Aim (tư thế giương vũ khí), Hit (bị trúng đòn), KO (hạ gục), Win (chiến thắng). Hướng nhìn hiện tại là 3/4 nhìn xuống.
- **Quậy Boy:** Hoàn toàn không có chuỗi frame hoạt ảnh nào (0 frame). Các slice 60 ô trong Unity thực chất là cắt máy móc từ tranh concept art tĩnh.
- **Chó Vàng & Mèo Mun:** Hoàn toàn không có chuỗi frame hoạt ảnh nào (0 frame). Nguồn từ turnaround chỉ có 1 góc nhìn tĩnh.

## 3. Chính sách độ phân giải (chuẩn màn Retina 3x: cần ≥ 218 px)

| Nhân vật | Chiều cao nguồn | Khung gameplay đích | Đánh giá độ nét | Hành động đề xuất |
|---|:---:|:---:|:---:|---|
| Nam Làng | 289 px | 256×256 | Sắc nét (132% chuẩn) | Dùng nguồn hiện có, chỉ vẽ thêm Hit/KO/Win |
| Nữ Năng Động | 281 px | 256×256 | Sắc nét (128% chuẩn) | Dùng nguồn hiện có, chỉ vẽ thêm Hit/KO/Win |
| Cô Ba | 278 px | 256×256 | Sắc nét (127% chuẩn) | Dùng nguồn hiện có, chỉ vẽ thêm Hit/KO/Win |
| Anh Teen | 257 px | 256×256 | Sắc nét (118% chuẩn) | Dùng nguồn hiện có, chỉ vẽ thêm Hit/KO/Win |
| **Quậy Boy** | 189 px | 256×256 | **CẦN BẢN NÉT HƠN** (86% chuẩn, mềm mờ) | **Tạo mới toàn bộ Chibi bằng ChatGPT** |
| **Chó Vàng** | 131 px | 256×256 | **CẦN BẢN NÉT HƠN** (60% chuẩn, vỡ hạt) | **Tạo mới toàn bộ Chibi bằng ChatGPT** |
| **Mèo Mun** | 131 px | 256×256 | **CẦN BẢN NÉT HƠN** (60% chuẩn, vỡ hạt) | **Tạo mới toàn bộ Chibi bằng ChatGPT** |

## 4. Bảng mức chìm chuẩn hóa (sinkRatio)

| Nhân vật | Tỷ lệ đầu thô | Mức chìm áp dụng (`sinkRatio`) | Tỷ lệ đầu nhìn thấy | Nhận xét |
|---|:---:|:---:|:---:|---|
| **Nam Làng** | 46,7% | **18% (0.18)** | 56,7% | Khớp hoàn hảo với tham chiếu (56,4%) |
| **Nữ Năng Động** | 46,3% | **18% (0.18)** | 56,1% | Khớp chuẩn phong cách Chibi tham chiếu |
| **Cô Ba** | 52,2% | **10% (0.10)** | 58,0% | Giữ thân cân đối, không bị cụt người |
| **Anh Teen** | 52,5% | **10% (0.10)** | 58,3% | Giữ thân cân đối, không bị cụt người |
| **Chó Vàng** | 52,6% | **8% (0.08)** | 57,2% | Đẹp với thú 4 chân, thấy rõ bàn chân |
| **Mèo Mun** | 53,4% | **8% (0.08)** | 58,0% | Đẹp với thú 4 chân, thấy rõ bàn chân |
| **Quậy Boy** | 28,0% (sọ thật) | *0% (Tạm dừng)* | — | Tỷ lệ người lớn mảnh khảnh, chờ vẽ lại Chibi |
