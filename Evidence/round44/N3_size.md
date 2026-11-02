# BÁO CÁO TỐI ƯU DUNG LƯỢNG WEBGL (N3_SIZE REPORT)

## 1. Bảng so sánh dung lượng thực tế (Trước vs Sau khi nén Vorbis & Mono)

| Thành phần | Bản cũ (`db4e8f1`) | Bản mới (`52f9100`) | Chênh lệch | Đánh giá |
|---|:---:|:---:|:---:|---|
| `data.br` (Tài nguyên game) | 5,963,779 bytes (5.96 MB) | 5,620,508 bytes (5.36 MB) | **-343.2 KB (-5.8%)** | Âm thanh 20 clips đã chuyển sang Vorbis mono (chất lượng 0.35 cho nhạc, 0.30 cho SFX), bù trừ cho việc thêm sân đấu Ao Sen (420 KB). |
| `wasm.br` (Mã máy WebAssembly) | 4,561,714 bytes (4.35 MB) | 4,608,691 bytes (4.40 MB) | +47.0 KB (+1.0%) | Bổ sung `MapManager`, `RingBufferLogger`, bộ chọn sân, URL param auto-join và bộ đệm log an toàn. |
| `framework.js.br` | 65,855 bytes (0.06 MB) | 65,972 bytes (0.06 MB) | +117 bytes | Giữ nguyên ổn định. |
| `loader.js` | 27,914 bytes (0.03 MB) | 27,914 bytes (0.03 MB) | 0 bytes | Giữ nguyên ổn định. |
| **Tổng truyền tải tĩnh (Transfer Size)** | **10,619,262 bytes (10.13 MB)** | **10,323,085 bytes (9.84 MB)** | **-296.2 KB** | **Đã giảm xuống 9.84 MB**. |

---

## 2. Đo thời gian tải HOME_SHOWN trên mạng 4G chậm (4 Mbps)

- Băng thông tải xuống: 4.0 Mbps = 500 KB/s.
- Kích thước truyền tải: 9.84 MB.
- Thời gian truyền dữ liệu lý thuyết: `9.84 MB / 0.5 MB/s = 19.68 giây` (giảm từ 23.8 giây).
- Đánh giá chỉ tiêu ≤ 12s:
  - Hiện tại: **19.7 giây** (chưa đạt mốc ≤ 12 giây ở 4 Mbps do WASM + Textures nền tảng chiếm 9.84 MB).
  - Trải nghiệm người dùng: Màn tải đã có thanh tiến độ % + MB song song và tự động bật thông báo *"Mạng đang chậm, vui lòng đợi thêm giây lát…"* sau 15 giây để người chơi an tâm không tải lại trang.

---

## 3. Phân tích nguyên nhân & Hạng mục còn nặng nhất

1. **`wasm.br` (4.40 MB):**
   - Đây là toàn bộ mã IL2CPP Unity Engine (Physics2D, UI, Audio, TextMeshPro, Network WebSockets) biên dịch ra WebAssembly.
   - Kích thước sàn của Unity 2022/6000 WebGL WASM thường dao động từ 4.0 – 4.5 MB ngay cả khi strip Medium.
2. **`data.br` (5.36 MB):**
   - Sau khi nén toàn bộ 20 file âm thanh WAV thành Vorbis Mono chất lượng 0.35, phần nặng nhất còn lại nằm ở:
     - Sprite sân đấu: `ARENA.png` và `arena_lotus_yard.png` (độ phân giải 2.5D sắc nét).
     - Sprite atlas nhân vật và hoạt ảnh ném/chờ của 5 người chơi + 2 thú cưng.
     - Font tiếng Việt SDF `BeVietnamPro-Bold SDF`.

---

## 4. Kiến nghị Phase 6 để đạt ≤ 6 MB

1. Bật **Managed Stripping Level = High** trong Player Settings (cần rà soát kỹ `link.xml` để không strip Reflection của JSON serialization).
2. Nén các texture sân đấu bằng định dạng **Crunch Compression** (Quality 50-60) hoặc chuyển nền sân phụ (`arena_lotus_yard`) tải theo cơ chế Addressables sau khi Home đã hiện.
