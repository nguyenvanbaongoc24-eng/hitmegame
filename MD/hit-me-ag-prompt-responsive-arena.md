# HIT ME – PROMPT CHO ANTIGRAVITY: chốt tỉ lệ, báo cáo, nền tảng responsive (iOS + Android) và sân đấu

> Dán toàn bộ file này cho AG. **Tất cả giá trị đã được chốt**, không còn ô trống. Làm đúng thứ tự, **commit sau mỗi bước nhỏ** (`WIP:` nếu dang dở). Không báo "xong" khi thiếu bằng chứng. Báo cáo ngắn để tiết kiệm token.

---

## 0. QUYẾT ĐỊNH ĐÃ CHỐT (thay cho các ô trống trong prompt trước)

| Hạng mục | Giá trị chốt |
|---|---|
| Ghi chú duyệt `PHASE2_AUDIT.md` | Đã duyệt, với các chỉnh sửa trong file này |
| Số vòng tối đa | `maxRounds = 5` |
| Hòa HP khi hết vòng | Xếp theo số lần trúng đối thủ; vẫn bằng thì hòa |
| Chưa ngắm | `NoAimPolicy = ThrowFacing` |
| Kích thước sân (không gian game) | **9,0 × 11,4** (rộng × cao, diện tích ≈ 102,6; tỉ lệ ≈ 0,79) – thay cho 8 × 12,8 |
| Lý do đổi tỉ lệ | Sân trong `ARENA.png` (1024×1536) hơi vuông hơn 8 × 12,8; art và gameplay phải cùng tỉ lệ |

Giữ nguyên toàn bộ luật cũ (pha đặt 5 giây, đặt bất kỳ đâu, giữ + lướt để kéo thước ngắm, hết giờ tự ném, dừng ở người đầu tiên trúng, chỉ thấy nhân vật của mình trong pha đặt).

### Nguyên tắc cứng (nhắc lại)
- Không xóa `ASSET`, `SFX`, `SpriteSources`, `Evidence`, `MD`. Không sửa `LocalBot.unity`.
- **Không sửa logic combat ở Core.** Core chỉ được đổi: tham số `GameConfig`, phần xếp hạng/hòa khi hết vòng (nếu chưa có), và thêm lớp chuyển đổi tọa độ.
- **Không tạo file .md mới** ngoài `MD/PHASE2_AUDIT.md` (cập nhật) và `MD/PHASE2_STEP2_REPORT.md`.
- Thiếu asset → dùng placeholder và ghi vào danh sách thiếu, **không tự vẽ asset giả**.
- Mọi kết luận kèm bằng chứng trong `Evidence/phase2/`.

---

## 1. TỈ LỆ & THÔNG SỐ ĐỂ CHƠI MƯỢT TRÊN iOS VÀ ANDROID

### 1.1 Màn hình cần hỗ trợ (dọc, portrait)
| Nhóm máy | Tỉ lệ | Độ phân giải kiểm thử |
|---|---|---|
| Android cũ/rẻ | 9:16 | 1080×1920 |
| iPhone đời mới (Dynamic Island) | ≈ 9:19.5 | 1179×2556 (**chuẩn thiết kế**: 1080×2340) |
| Android phổ biến | 9:20 | 1080×2400 |
| Android cao | 9:21 | 1080×2520 |
| Máy tính bảng (3:4, ví dụ iPad) | 3:4 | 1620×2160 → khóa khung chơi tối đa 9:16, hai bên để trống bằng nền trang trí |

Khóa hướng **Portrait**. Mọi layout phải đúng trên cả 5 độ phân giải trên.

### 1.2 Quy tắc layout (đơn vị: % chiều cao vùng an toàn, đã tính Safe Area)
Giữ bố cục do AG đo từ mockup 1:
- Thanh trên (vòng, đồng hồ, cài đặt): 5,2 – 11,5%
- Dải 6 thẻ người chơi: 11,5 – 20,5%
- Dòng trạng thái ("CHỌN HƯỚNG NÉM | Đối thủ đang ẩn"): 20,5 – 24,5%
- **Sân đấu: 24,5 – 85,5% (61%)**
- Thanh dưới (chat, vũ khí, SẴN SÀNG, emoji): 85,5 – 95,5%
- Lề dưới (thanh Home/cử chỉ hệ thống): 4,5%

**Cách co giãn (để không méo, không tràn trên mọi tỉ lệ):**
1. Dùng Canvas Scaler **Scale With Screen Size**, tham chiếu 1080×2340, `Match = 0,5` (cân giữa rộng và cao).
2. **Sân luôn giữ tỉ lệ nguồn, không bao giờ kéo giãn.** Hệ số thu phóng sân = `min(bề rộng khả dụng × 0,92 / bề rộng sân (px), chiều cao khả dụng / chiều cao sân (px))`.
3. Màn **cao hơn 9:19.5** (9:20, 9:21): phần chiều cao dư chia cho nền trang trí (cổng đình, cây, cờ kéo dài) và khoảng cách giữa các dải HUD, không phóng to sân quá mức.
4. Màn **thấp hơn 9:19.5** (9:16): sân thu nhỏ theo chiều cao, các dải HUD co theo neo (anchor); không để thẻ người chơi và nút SẴN SÀNG chồng nhau.
5. Máy tính bảng: khung chơi tối đa tỉ lệ 9:16 đặt giữa, hai bên là nền trang trí.
6. Mọi UI nằm trong **Safe Area** (tai thỏ/Dynamic Island, thanh cử chỉ iOS, punch-hole và thanh điều hướng Android). Đọc `Screen.safeArea` mỗi khi thay đổi độ phân giải/xoay.

### 1.3 Kích thước cảm ứng (theo kích thước vật lý, không theo pixel)
Tính bằng `Screen.dpi` (có giá trị dự phòng 160 nếu dpi = 0):
| Thành phần | Tối thiểu |
|---|---|
| Vòng cảm ứng quanh nhân vật khi ngắm | đường kính **≥ 9 mm** (≈ 48dp), lớn hơn thân nhân vật |
| Nút (SẴN SÀNG, chat, emoji, cài đặt) | **≥ 48 × 48 dp**; nút SẴN SÀNG cao **≥ 56 dp** |
| Ngưỡng kéo tối thiểu để tính là chọn hướng (`minAimDrag`) | **≥ 4 mm (≈ 20dp)**, đổi sang đơn vị game khi chạy |
| Khoảng cách giữa hai nút gần nhau | ≥ 8 dp |

Thước ngắm kéo dài ra ngoài vị trí ngón tay (ngón che nhân vật vẫn thấy hướng). Chuyển `minAimDrag` trong `GameConfig` từ "0,25 đơn vị thế giới" sang **dp/mm** và đổi tại thời điểm chạy.

### 1.4 Ngân sách hiệu năng (mục tiêu cho máy Android tầm thấp ~3 GB RAM và iPhone cũ)
| Mục | Mục tiêu |
|---|---|
| Khung hình | **60 FPS** (`Application.targetFrameRate = 60`, `vSyncCount = 0`); tự hạ xuống **30 FPS** nếu thời gian khung > 22 ms trong 3 giây liên tục |
| Độ phân giải hiển thị | Giới hạn cạnh ngắn ≤ 1080 px (đặt qua `Screen.SetResolution`/render scale); iPhone 1179×2556 → ≈ 0,92 |
| Draw call / SetPass | ≤ 100 / ≤ 40 mỗi khung |
| Bộ nhớ texture | ≤ 200 MB tổng RAM game ≤ 600 MB |
| Sprite Atlas | tối đa 2048×2048 (không dùng 4096); nhóm: Characters, UI, VFX, Env |
| Nén texture | **ASTC** (6×6 cho nhân vật/UI, 8×8 cho nền); Android có dự phòng ETC2; WebGL kiểm thử dùng ASTC cho trình duyệt di động |
| Hạt (particle) tối đa | ≤ 150 đang hoạt động; không có sinh vật thể/GC alloc mỗi khung trong gameplay (dùng object pool cho dép, hiệu ứng, số `-1`) |
| Font | TMP Static Atlas ≤ 1024×1024 (nạp sẵn toàn bộ ký tự tiếng Việt) |
| Dung lượng | Bản cài đầu ≤ **80 MB** (Android AAB), WebGL thử nghiệm ≤ **40 MB** nén |
| Build | iOS/Android: IL2CPP, ARM64, Managed Stripping mức Medium/High; không dùng post-processing nặng |

Mục tiêu "mượt": 0 GC alloc trong vòng chơi (đo bằng Profiler), thời gian khung trung bình ≤ 16,6 ms trên máy chuẩn, không tụt FPS khi 6 nhân vật + 6 vũ khí + hiệu ứng trúng đồng thời.

---

## BƯỚC 1 – Báo cáo hiện trạng (chỉ đọc, ngắn)

Gửi báo cáo theo mẫu cuối file với các mục:
1. Liệt kê **19 file đã đổi** (+1.289 −92), nhóm theo: Core / GameConfig / Test / Game(View) / Editor / Asset. Xác nhận Core chỉ đổi phần cho phép; nếu khác, nêu rõ.
2. Hiện `arenaWidth × arenaHeight`, `maxRounds`, `RoundLimitTiebreakPolicy`, `NoAimPolicy` đang là gì; số liệu `sim_v2` (số vòng trung bình/lớn nhất, % thắng rõ, % hòa HP).
3. Kết quả toàn bộ test + AutoPlay 10 trận.
4. Chữ tiếng Việt trên WebGL: ảnh scene kiểm tra đủ chuỗi (`CHỌN HƯỚNG NÉM`, `SẴN SÀNG`, `Đối thủ đang ẩn`, `Thắng`, `Thua`, `Chơi lại`, `Kết quả`): dấu có đủ không?
5. Danh sách asset thiếu và placeholder đang dùng.

Nếu có phần chưa đạt, ghi rõ, **không che**. Sau đó tiếp tục Bước 2.

## BƯỚC 2 – Áp tỉ lệ đã chốt vào GameConfig (không sửa logic combat)

- `arenaWidth = 9.0`, `arenaHeight = 11.4`, `maxRounds = 5`, hòa HP theo số lần trúng, `NoAimPolicy = ThrowFacing`.
- Đổi `minAimDrag` sang đơn vị dp/mm như mục 1.3 (có hàm quy đổi theo `Screen.dpi`).
- Cập nhật/thêm test: trận kết thúc đúng ở vòng 5; xếp hạng theo HP; hòa HP xếp theo số lần trúng; vẫn bằng thì hòa; test quy đổi dp → đơn vị game.
- Chạy lại 1.000 trận mô phỏng → `Evidence/phase2/sim_v3.txt` (số vòng trung bình/lớn nhất, % thắng rõ, % xếp theo HP, % hòa). Nếu **hơn 50% trận kết thúc bằng hòa HP** thì báo ngay cho chủ dự án (có thể phải tăng `maxRounds` hoặc đổi luật).
- Toàn bộ test + AutoPlay 10 trận. Commit; tag `v0.1.1-balance`; push.

## BƯỚC 3 – Phép biến đổi phối cảnh giữa sân game và màn hình

`ARENA.png` (1024×1536) là ảnh phẳng, sân chơi là **hình thang** (cạnh trên hẹp, cạnh dưới rộng). Tỉ lệ đơn giản sẽ làm hướng ngắm trên màn hình lệch hướng Core tính.

1. Đo 4 góc của sân chơi (trong viền trắng) bằng pixel trong `ARENA.png`; lưu vào ScriptableObject `ArenaQuadConfig` (không hard-code).
2. Cài **homography** hai chiều (thuần C#, có thể đặt trong Core hoặc Game): `GameToScreen`, `ScreenToGame` (dùng khi nhập liệu).
3. Nhân vật, bóng, dép, hiệu ứng **thu nhỏ theo độ xa** (hệ số theo y), sắp xếp thứ tự vẽ theo y.
4. Test: 4 góc ánh xạ đúng; khứ hồi (round trip) sai số < 0,01 đơn vị; một hướng ngắm trên màn hình tạo đúng hướng ném mà Core tính; điểm chạm ngoài sân bị kẹp vào biên.
5. Chạy lại toàn bộ test. Commit.

## BƯỚC 4 – Hạ tầng responsive (khung "Stage")

- Dựng `StageRoot` theo mục 1.2: Canvas Scaler, Safe Area, quy tắc thu phóng sân, nền kéo dài, khung 9:16 cho máy tính bảng.
- Một script `ResponsiveStage` đọc tỉ lệ màn hình và áp quy tắc; chạy lại khi đổi độ phân giải/xoay.
- Cài `PerformanceManager`: `targetFrameRate`, `vSyncCount`, giới hạn độ phân giải cạnh ngắn ≤ 1080, hạ FPS tự động theo mục 1.4. Ghi cấu hình hiện tại vào log khi khởi động.
- Cài `TouchMetrics`: đổi mm/dp sang pixel/đơn vị game bằng `Screen.dpi`.
- Thiết lập import: Pixels Per Unit thống nhất, Sprite Atlas theo nhóm, nén ASTC theo bảng 1.4.
- **Chụp ảnh scene rỗng (sân + nền) trên 5 độ phân giải ở mục 1.1** (Game view hoặc Device Simulator): `Evidence/phase2/res_1080x1920.png`, `res_1179x2556.png`, `res_1080x2400.png`, `res_1080x2520.png`, `res_1620x2160.png`. Mỗi ảnh phải thấy **đủ 4 góc sân**, không bị tai thỏ che, không méo.

## BƯỚC 5 – Sân đấu bằng layer (Phase 2 Bước 2)

Theo `hit-me-phase2-prompt.md` Bước 2, kết hợp tỉ lệ ở trên:
- Dựng sân từ **nhiều layer** (nền cây/trời, cổng đình, hàng rào khán giả, sàn gạch, hoa văn sen, viền trắng), không dùng một ảnh phẳng. Nếu `ARENA.png` là ảnh phẳng duy nhất, dùng **tạm** làm nền cho scene kiểm thử và ghi vào danh sách asset thiếu (layer tách).
- Khán giả/cờ chuyển động nhẹ bằng animation đơn giản (không tốn CPU).
- Đặt 6 `PlayerView` placeholder ở các vị trí khác nhau, kiểm tra bóng và thu nhỏ theo độ xa.
- Kiểm tra bằng **thước ngắm** trên 2 độ phân giải khác tỉ lệ: kéo từ nhân vật ra góc sân trên-trái, hướng ném phải khớp hướng thước.
- Chạy toàn bộ test + AutoPlay; build WebGL; **chụp ảnh sân trên iPhone** nếu có thể (chủ dự án sẽ gửi), nếu chưa thì chụp trên Chrome với giả lập.

**Xong khi:** 5 ảnh độ phân giải đạt (đủ 4 góc sân, không méo); round trip homography đạt; thước ngắm khớp hướng ném; test pass 100%.

> ⛔ **DỪNG sau Bước 5.** Viết `MD/PHASE2_STEP2_REPORT.md` (đã làm gì, bằng chứng, asset thiếu, số đo FPS/draw call nếu đo được, vấn đề tồn đọng) và chờ chủ dự án duyệt trước khi làm HUD (Bước 3 của Phase 2).

---

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn ảnh/log/test)
ASSET THIẾU: ...
CHỜ CHỦ DỰ ÁN: ...
VẤN ĐỀ/CÂU HỎI: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```

## Điều cấm (nhắc lại)
Không sửa logic combat ở Core; không xóa `ASSET`/`SFX`; không tạo .md ngoài danh sách; không tự tạo asset giả; không báo xong khi thiếu bằng chứng; không làm HUD, menu, mạng ở giai đoạn này.
