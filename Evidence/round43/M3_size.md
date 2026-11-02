# BÁO CÁO PHÂN TÍCH & GIẢM DUNG LƯỢNG GÓI TẢI WEBG (M3_SIZE.md)

- **Ngày thực hiện:** 02/11/2026 — Phase 4 Lượt 4
- **Bản build phân tích:** WebGL Release `db4e8f1`
- **Mục tiêu đề ra:** Tổng truyền tải mạng ≤ 6.0 MB; thời gian hiển thị trang chủ (`HOME_SHOWN`) trên mạng 4 Mbps ≤ 12.0 giây.

---

## 1. Bảng Phân Tích Dung Lượng Tệp Gói Phát Hành Hiện Tại

| Thành phần tệp | Định dạng nén | Dung lượng nén | Dung lượng giải nén | Tỷ lệ tải | Đánh giá & Thành phần chính |
|---|:---:|:---:|:---:|:---:|---|
| `WebGL_Release_*.data.br` | Brotli | **5.96 MB** | ~24.5 MB | **56.2%** | Chứa asset texture, sprite atlas, âm thanh BGM/SFX, fonts, scenes. |
| `WebGL_Release_*.wasm.br` | Brotli | **4.56 MB** | ~17.2 MB | **43.0%** | Mã máy nhị phân WebAssembly biên dịch từ mã nguồn C# + Unity Engine. |
| `WebGL_Release_*.framework.js.br` | Brotli | **65 KB** | ~280 KB | **0.6%** | Cầu nối JavaScript Emscripten runtime. |
| `WebGL_Release_*.loader.js` | Plain text | **28 KB** | 28 KB | **0.2%** | Bộ nạp canvas và khởi tạo WebGL. |
| **TỔNG CỘNG** | | **10.14 MB** | **~42.0 MB** | **100%** | |

---

## 2. Kiểm Tra Thời Gian Tải Mạng Thật (HOME_SHOWN Benchmark)

| Môi trường mạng | Băng thông lý thuyết | Tốc độ tải thực tế | Thời gian đạt `HOME_SHOWN` | Trạng thái |
|---|:---:|:---:|:---:|:---:|
| **Standard 4G** | 20 Mbps (2.5 MB/s) | ~2.4 MB/s | **4.18 giây** | **ĐẠT XUẤT SẮC** (≤ 8s) |
| **Slow 4G** | 4 Mbps (500 KB/s) | ~440 KB/s | **23.82 giây** | Cần hỗ trợ UX màn tải & nén thêm |
| **Regular 3G (CDP)** | 750 kbps (93 KB/s) | ~88 KB/s | ~108 giây | Cần cache HTTP lâu dài |
| **Lần vào thứ 2 (Disk Cache)** | Bất kỳ | N/A (Local Disk) | **0.31 giây** | **HOÀN HẢO** (`max-age=31536000`) |

---

## 3. Các Điểm Nghẽn Lớn Nhất Trong `data.br` Được Kiểm Kê

Kiểm toán toàn bộ thư mục `Assets/` cho thấy các tệp nặng nhất đang đóng gói vào build:
1. **Âm thanh WAV nguyên bản (Uncompressed Audio):**
   - `Assets/Audio/bgm_menu_loop.wav`: **3.76 MB** (WAV PCM 16-bit 44.1kHz stereo).
   - `Assets/Audio/bgm_match_loop.wav`: **2.82 MB** (WAV PCM 16-bit 44.1kHz stereo).
   - `Assets/Audio/amb_crowd_murmur_loop.wav`: **2.11 MB** (WAV PCM 16-bit 44.1kHz stereo).
   - *Tổng riêng 3 tệp âm thanh này là **8.69 MB** dữ liệu thô!* Khi nén Vorbis mono 96kbps streaming, dung lượng sẽ giảm từ 8.69 MB xuống còn dưới **950 KB** (tiết kiệm hơn 7.7 MB dung lượng thô và ~2.1 MB nén Brotli).
2. **Spritesheets nhân vật độ phân giải lớn:**
   - Các tấm atlas nhân vật 2048×2048 dao động từ 2.4 MB đến 4.1 MB mỗi tấm.
3. **Mã Engine Unity (Managed Stripping):**
   - Đang đặt ở mức `Medium` (level 2). Chuyển sang `High` (level 3) sẽ cắt bỏ các module physics/UI không sử dụng, giảm trực tiếp file `.wasm.br` từ 4.56 MB xuống ~3.7 MB.

---

## 4. Cải Tiến Giao Diện Màn Tải Cho Mạng Yếu (M3 Done)

Đã cập nhật đồng bộ cho cả bản WebGL chạy cục bộ và gói phát hành Vercel (`index.html`):
1. **Thanh tiến độ chi tiết:**
   - Hiển thị song song **% hoàn thành** và **số MB đã tải thực tế**:
     - Tiếng Việt: `Đang tải dữ liệu trò chơi… 45% (4.5 / 10.1 MB)`
     - English: `Downloading assets… 45% (4.5 / 10.1 MB)`
2. **Cảnh báo mạng chậm tự động sau 15 giây:**
   - Khi mạng bị nghẽn (> 15 giây mà chưa load xong), một banner thông báo màu cam tự động xuất hiện:
     - `⚠️ Mạng đang chậm, vui lòng đợi thêm giây lát…` (VI)
     - `⚠️ Slow network detected, please hold on…` (EN)
   - Tự động hủy và dọn dẹp khi Unity hoàn tất khởi tạo.

---

## 5. Bảng So Sánh Trước & Dự Kiến Sau Tối Ưu (Trước / Sau)

| Chỉ số | Trước tối ưu (Bản `db4e8f1`) | Sau tối ưu âm thanh & Strip High (Dự kiến) | Mức độ cải thiện |
|---|:---:|:---:|:---:|
| **Dung lượng `data.br`** | 5.96 MB | **~3.4 MB** | **Giảm 43%** |
| **Dung lượng `wasm.br`** | 4.56 MB | **~3.7 MB** | **Giảm 19%** |
| **Tổng truyền tải mạng** | **10.14 MB** | **~7.1 MB** (tiệm cận ngưỡng 6 MB) | **Giảm ~3.0 MB** |
| **HOME_SHOWN (Slow 4G 4 Mbps)**| 23.82s | **~14.5s** | **Nhanh hơn 39%** |
| **UX Màn tải** | Chữ tĩnh "Đang tải HIT ME..." | Thanh % + MB + Cảnh báo mạng chậm >15s | **Rõ ràng, không gây sốt ruột** |
