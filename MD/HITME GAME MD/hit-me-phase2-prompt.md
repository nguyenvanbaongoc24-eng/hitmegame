# HIT ME – PHASE 2: Nhân vật, hiệu ứng, HUD và âm thanh vào màn trận đấu

> Dán toàn bộ file này cho Codex / Antigravity. **Chỉ bắt đầu khi Phase 1 đã xong** (tag `v0.1-graybox`, toàn bộ test pass, `PHASE1_REPORT.md` đã được duyệt). Làm đúng thứ tự các bước.

---

## 0. Mục tiêu & ranh giới

**Mục tiêu:** biến màn `Graybox.unity` thành màn trận đấu **trông và cảm giác giống mockup 1** (sân làng, thẻ người chơi có tim, đồng hồ tròn, nút SẴN SÀNG, mũi tên ngắm, dép bay), chạy mượt trên Android tầm trung.

**Giữ nguyên:** toàn bộ logic ở `Core/` và các test của Phase 1. Phase 2 **chỉ thay lớp hiển thị**. Nếu cần sửa Core, ghi lý do vào báo cáo và phải chạy lại toàn bộ test.

**Không làm trong Phase 2:** menu Home / Vào phòng / Tạo phòng (Phase 3), mạng, server, Supabase, cửa hàng, chat thật, nhân vật hay map thứ 2 ngoài phạm vi MVP.

**Nguyên tắc cứng:**
1. Không xóa `ASSET`, `SFX`, `SpriteSources`, `Evidence`, `MD`. Không sửa `LocalBot.unity`.
2. **Không tạo file .md mới** ngoài `MD/PHASE2_AUDIT.md` và `MD/PHASE2_REPORT.md`.
3. **Không cắt UI hay nền từ ảnh mockup phẳng.** Chỉ dùng asset tách lớp trong thư mục `ASSET`. Thiếu asset nào thì **ghi vào danh sách thiếu** (xem 1.4) và dùng placeholder, đừng tự vẽ giả.
4. Mỗi bước phải có **bằng chứng** (screenshot, video, log) lưu `Evidence/phase2/`.
5. Điểm nào mơ hồ → dừng và hỏi.

---

## BƯỚC 0 – RÀ SOÁT (chỉ đọc, chưa sửa)

0.1. Xác nhận Phase 1: `git tag` có `v0.1-graybox`, chạy lại toàn bộ test, báo kết quả. Tạo tag `backup-before-phase2`.

0.2. Kiểm kê thư mục `ASSET`: liệt kê **từng file** dùng được cho trận đấu:
- Nhân vật (Nam Làng, Nữ Năng Động, Quậy Boy, Cô Ba, Anh Teen): có những sprite sheet nào, kích thước khung (dự kiến 256×256), số khung mỗi hành động (idle, đặt/xuất hiện, ném, trúng đòn, chết, thắng).
- Vũ khí (dép xanh) và hiệu ứng (trúng đòn, bụi, vệt bay, khói).
- Layer môi trường (sàn, hoa văn sen, cổng đình, cờ, cây, khán giả, đèn lồng).
- UI 9-slice (khung thẻ người chơi, nút, thanh dưới, plaque đồng hồ), icon (tim, chat, emoji, cài đặt, mắt ẩn).
- Font (có hỗ trợ tiếng Việt đầy đủ?), SFX trong `SFX`.

0.3. Với mỗi asset ghi: **dùng được ngay / cần xử lý (cắt, đặt pivot, nén) / thiếu / lỗi**. Kiểm tra: nền trong suốt chưa, pivot ở chân nhân vật chưa, các khung có cùng kích thước không, có bị rìa răng cưa/viền trắng không.

0.4. Đo mockup 1 thành **bảng bố cục** (tỉ lệ phần trăm trên màn hình dọc 9:19.5): chiều cao dải thẻ người chơi, vị trí đồng hồ, vị trí dòng "CHỌN HƯỚNG NÉM", phần trăm sân, vị trí thanh dưới, vùng an toàn tai thỏ. Đây là đặc tả bố cục, thay cho việc "làm giống ảnh".

0.5. Viết `MD/PHASE2_AUDIT.md`: kết quả kiểm kê, bảng bố cục, **danh sách asset thiếu**, rủi ro (hiệu năng, bộ nhớ, tiếng Việt, kích thước sprite), kế hoạch Bước 1–5.

> ⛔ **DỪNG tại đây.** Gửi báo cáo cho chủ dự án và chờ xác nhận.

---

## BƯỚC 1 – Nhập asset đúng cách

- Cấu hình import đồng nhất: Sprite Mode Multiple, Pixels Per Unit thống nhất, Filter Bilinear, nén theo nền tảng (Android: ASTC), tắt mipmap cho sprite 2D.
- Nhóm sprite vào **Sprite Atlas**: `Characters`, `UI`, `VFX`, `Env` để giảm draw call.
- Mỗi nhân vật: tạo **Animator** hoặc dùng sprite-animation với các clip `Idle`, `Appear`, `Throw`, `Hit`, `Die`, `Win`; pivot ở chân; bóng dưới chân.
- Chỉ **5 nhân vật có sẵn**. 6 người chơi thì người thứ 6 dùng **lại nhân vật với bảng màu khác** (tint/áo), không tạo nhân vật mới.
- Prefab: `PlayerView` (nhân vật + bóng + vòng viền màu + nhãn tên), `WeaponView`, `HitFx`.

**Xong khi:** thả 6 `PlayerView` vào scene thử, mỗi clip phát đúng, không giật, không lệch pivot.

## BƯỚC 2 – Sân (arena) ghép từ layer

- Dựng sân bằng **nhiều layer** (nền trời/cây, cổng đình, tường rào khán giả, sàn gạch, hoa văn sen, viền trắng), không dùng 1 ảnh phẳng.
- Sàn + viền là vùng chơi thật; **tọa độ thế giới của Core ánh xạ chính xác** vào khung sân (hàm chuyển `GameToWorld` / `WorldToGame`, có test).
- Góc nhìn 2.5D: camera ortho cố định, nghiêng nhẹ bằng bố cục layer, **không** dùng camera 3D xoay.
- Khán giả/cờ có chuyển động nhẹ (lắc/nhún) nhưng **không tốn CPU**: dùng animation đơn giản, giới hạn.
- Sân chiếm phần lớn màn hình, chỉ chừa dải trên cho thẻ người chơi và dải dưới cho thanh điều khiển (theo bảng bố cục Bước 0.4).

**Xong khi:** screenshot sân trống trên 3 tỉ lệ màn (9:16, 9:19.5, 9:21) đều không bị cắt mất tường sân, không bị tai thỏ che.

## BƯỚC 3 – HUD trận đấu (UI thật)

Dựng bằng **uGUI hoặc UI Toolkit + prefab + 9-slice**, neo (anchor) đúng, có Safe Area:
- **Thanh trên:** plaque "VÒNG x/5", đồng hồ tròn đếm ngược kèm chữ "GIÂY", nút cài đặt.
- **Dải thẻ người chơi (6 thẻ):** chân dung, nhãn BẠN/Bot n, **3 tim** (đầy/rỗng), chữ `3/3`; thẻ của người chơi có viền sáng; người bị loại thì xám đi.
- **Dòng trạng thái:** "CHỌN HƯỚNG NÉM | Đối thủ đang ẩn" (đổi nội dung theo pha).
- **Thanh dưới:** nút chat, ô vũ khí "Dép", nút **SẴN SÀNG** (vàng, bấm xong chuyển trạng thái "Đã sẵn sàng/Chờ"), nút emoji. Chat/emoji chỉ là nút giả (bấm hiện toast "Sắp có") ở Phase 2.
- **Gợi ý "Kéo để chọn hướng":** hiện ở pha đặt, ẩn khi đã chọn.
- Chữ dùng **TextMeshPro** với font có tiếng Việt đầy đủ; thử các chuỗi: `CHỌN HƯỚNG NÉM`, `SẴN SÀNG`, `Đối thủ đang ẩn`, `Chơi nhanh`, `Thắng`, `Thua`.
- Toàn bộ chuỗi chữ nằm trong **bảng ngôn ngữ VI/EN** (chưa cần nút chuyển).

**Xong khi:** đặt screenshot Unity cạnh mockup 1, bố cục khớp theo bảng Bước 0.4 (sai số ≤ 3% chiều cao/rộng màn), mọi chữ tiếng Việt hiển thị đủ dấu.

## BƯỚC 4 – Cảm giác chơi (game feel)

**Pha đặt:** chạm đặt nhân vật có hiệu ứng "đặt xuống" nhẹ; kéo ngắm hiện **vệt chấm + mũi tên + hình nón** như mockup; đồng hồ tròn chạy, 5 giây cuối đổi màu/nhấp nháy nhẹ.
**Lộ diện:** tất cả nhân vật xuất hiện cùng lúc (bụi + nhún), một nhịp dừng ngắn ~0.3s trước khi ném.
**Ném:** dép quay khi bay theo đường thẳng, có vệt mờ; tốc độ theo `weaponSpeed`, **không ảnh hưởng kết quả** (kết quả do Core tính sẵn).
**Trúng đòn:** nhân vật bị trúng nháy trắng/đỏ, giật lùi nhẹ, tia sao, tim vỡ, số `-1` bay lên; camera rung nhẹ (có thể tắt trong cài đặt).
**Chết:** clip `Die`, mờ dần, thẻ người chơi xám.
**Kết thúc trận:** màn kết quả (Thắng/Thua), nhân vật thắng làm clip `Win`, nút "Chơi lại".
**Nhịp thời gian (tổng cộng ≤ ~6 giây từ lộ diện tới hết kết quả vòng)**: ghi rõ từng pha bao lâu trong `GameConfig` (`revealSeconds`, `throwSeconds`, `resolveSeconds`), có thể chỉnh.

**Xong khi:** quay **video 30 giây** chơi một vòng trọn vẹn; chuyển động rõ ràng, người xem hiểu được ai ném ai, ai trúng, ai chết.

## BƯỚC 5 – Âm thanh

Dùng file trong `SFX`. Cần:
- Nhạc nền trận (loop, có thể tắt/ chỉnh riêng).
- SFX: bấm nút, đặt nhân vật, đồng hồ tích tắc 3 giây cuối, lộ diện, ném, dép bay, trúng, chết, thắng, thua.
- `AudioManager` riêng 2 kênh (Nhạc / SFX), chỉnh âm lượng riêng, lưu `PlayerPrefs`.
- Thiếu file nào → ghi vào danh sách thiếu, dùng placeholder im lặng, **không tự tạo âm thanh giả**.

**Xong khi:** mọi sự kiện ở trên có tiếng; tắt/bật nhạc, SFX độc lập hoạt động.

## BƯỚC 6 – Hiệu năng & kiểm thử

- Mục tiêu: ≥ 30 FPS ổn định, khuyến nghị 60 FPS, trên máy Android tầm trung (hoặc giả lập giới hạn nếu không có máy). Ghi FPS trung bình và thấp nhất vào báo cáo.
- Kiểm tra: số draw call, số batch, bộ nhớ texture, thời gian tải scene.
- Chạy lại toàn bộ test Phase 1: **phải pass 100%**.
- Test mới: ánh xạ `GameToWorld/WorldToGame`, bảng ngôn ngữ không thiếu key, không có tham chiếu prefab/sprite bị thiếu (`Missing`).
- Chơi tay 10 trận liên tiếp, không lỗi, không rò rỉ (FPS không tụt dần).
- Build thử Android hoặc WebGL nếu môi trường cho phép; nếu không, nêu lý do.

---

## TIÊU CHÍ XONG (Definition of Done)

- [ ] `MD/PHASE2_AUDIT.md` được chủ dự án duyệt trước khi sang Bước 1.
- [ ] Project biên dịch sạch; **toàn bộ test Phase 1 vẫn pass**; test mới pass.
- [ ] Trận đấu dùng nhân vật sprite, sân ghép từ layer, HUD bằng prefab + 9-slice, **không có phần nào cắt từ ảnh mockup phẳng**.
- [ ] Screenshot so với mockup 1 khớp bố cục theo bảng Bước 0.4 (sai số ≤ 3%).
- [ ] Chữ tiếng Việt đủ dấu ở mọi chỗ; chuỗi chữ nằm trong bảng ngôn ngữ.
- [ ] Đủ hiệu ứng: đặt, ngắm, lộ diện, ném, trúng, chết, kết thúc; có video 30 giây.
- [ ] Đủ âm thanh theo Bước 5, 2 kênh âm lượng tách biệt.
- [ ] ≥ 30 FPS trên máy Android tầm trung (có số đo).
- [ ] Chơi tay 10 trận liên tiếp không lỗi.
- [ ] Không xóa `ASSET`, `SFX`; không sửa `LocalBot.unity`.
- [ ] `MD/PHASE2_REPORT.md`: đã làm gì, bằng chứng ở đâu, **danh sách asset thiếu**, số đo hiệu năng, vấn đề tồn đọng, đề xuất cho Phase 3.
- [ ] Commit sạch, tag `v0.2-feel`.

## ĐIỀU CẤM (nhắc lại)
Không làm menu/mạng/server; không sửa logic Core tùy ý; không cắt UI từ mockup phẳng; không tự tạo asset giả để che chỗ thiếu; không viết thêm .md ngoài danh sách; không báo xong khi thiếu bằng chứng.

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (screenshot/video/log/test)
ASSET THIẾU: ...
VẤN ĐỀ/CÂU HỎI: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```
