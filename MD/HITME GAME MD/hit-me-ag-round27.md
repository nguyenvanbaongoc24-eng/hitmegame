# HIT ME – LỆNH CHO AG, ĐỢT 27: SỬA HÌNH ẢNH TRONG TRẬN THEO ẢNH CHỦ DỰ ÁN CHỤP TRÊN ĐIỆN THOẠI THẬT

> Dán cho AG, kèm `hit-me-ag-round26.md` (G1–G6 chưa thấy báo), `hit-me-gameplay-rules-v1.2.md`. **Đây là ưu tiên số 1, trước mọi việc về tài liệu/audit.** Chủ dự án chơi bản WebGL trên điện thoại (iPhone, Safari) và nhận xét: *"vẫn rất tệ, nhân vật thì nhỏ"*. 3 ảnh chụp màn hình nằm trong cuộc hội thoại của chủ dự án; mô tả lại bên dưới. Phase 2 **không** đóng chừng nào chủ dự án chưa duyệt hình ảnh.

## Đánh giá
Báo cáo round26 xử lý giấy tờ (F1–F9) nhưng **không nhắc đến lỗi hình ảnh nào**, trong khi các lỗi này nhìn thấy ngay trên điện thoại. Bằng chứng của AG (Editor batchmode, 2 màn lobby/menu) không bao giờ chụp **cảnh đang chơi**, nên audit 12/12 "ĐẠT" không nói gì về chất lượng trận. Từ nay: **không báo "đạt" cho hình ảnh khi chưa có ảnh chụp từ bản WebGL ở từng pha và chủ dự án chưa duyệt.**

## Lỗi thấy trong ảnh điện thoại (mỗi lỗi: nguyên nhân, sửa, bằng chứng)

**V1. BẢNG SPRITE THAM CHIẾU (có chú thích chữ) hiện ra giữa sân, đè lên vị trí của Bot.** Trong ảnh 1 và ảnh 3, ở góc phải trên (đúng chỗ vòng đỏ của Bot) hiện một **tấm ảnh nhiều ô có chữ**: "Quần / Dép / Lá quạt (vũ khí) / Dây nón / Phụ kiện khác", "ANIMATION – ELIMINATION (10 frames)", "ANIMATION – HIT (6 frames)", "VFX – LÁ QUẠT (Vệt bay… Hiệu ứng va chạm… Dư quang)", "VŨ KHÍ – LÁ QUẠT (Weapon)". Đó là **ảnh bảng tổng hợp để tham khảo**, không phải khung hình nhân vật. Hiện tại **nhân vật của Bot được vẽ bằng nguyên cả tấm bảng đó**.
- Tìm nguồn: dán đường dẫn asset nào đang gán cho sprite Bot (nhân vật `Anh Nón Lá`/những nhân vật khác lấy từ thư mục `ANH NÓN LÁ`… trong `ASSET HITME FINAL`), và vì sao `Sprite.Create`/slice lấy cả texture.
- Sửa: chỉ dùng **khung đã cắt sạch từng khung** (không chữ, không nền bảng) cho cả 7 nhân vật; Bot dùng cùng bộ khung như người chơi.
- Chặn tái phát: viết kiểm thử EditMode: **mọi sprite dùng khi chạy** (nhân vật, vũ khí, FX) phải: (a) không nằm trong thư mục/tệp có chữ `sheet`, `contact`, `reference`, `atlas_labeled`; (b) kích thước nằm trong ngưỡng cấu hình (ví dụ ≤ 512×512); (c) không có khung nào có chữ cái viết sẵn (liệt kê danh sách tên). Đưa danh sách "sprite đã duyệt" vào một tệp cấu hình; sprite ngoài danh sách → test đỏ.

**V2. Nhân vật bị cắt dở/đen (ảnh 3, giữa sân).** Khung nhân vật chỉ còn nửa thân, một phần đen. Nguyên nhân khả dĩ: cắt sai ô, pivot sai, hoặc lấy khung `ELIMINATION`/`HIT` lệch. Sửa: tạo **ảnh ghép kiểm định** (`Evidence/asset_qa/contact/characters_<ngày>.png`) cho 7 nhân vật × (đứng, ném, bị trúng, bị loại): mỗi khung kèm hộp bao alpha, đánh dấu đỏ nếu hộp chạm mép khung. Khung nào chạm mép → thay khung hoặc cắt lại. Dán ảnh ghép.

**V3. Chữ "CHẢO" in ngay trên sprite chảo (ảnh 3), chữ "DÉP" nhỏ trong ô vũ khí.** Chú thích chữ của bảng gốc bị lẫn vào sprite. Sửa: dùng khung sạch không chữ; chữ trên ô vũ khí do UI vẽ bằng font game, cỡ đủ đọc. Xác nhận trong báo cáo: **vũ khí của từng nhân vật/Bot do thiết kế hay do lỗi?** (Luật chốt: vũ khí là Dép Tổ Ong xanh; nếu chảo dành cho nhân vật khác, chỉ rõ nguồn trong tài liệu của chủ dự án.)

**V4. NHÂN VẬT QUÁ NHỎ (lời chủ dự án).** Trên màn hình rộng ~923 px, nhân vật cao khoảng 70 px (≈ 6 % chiều cao sân). Trò chơi ném-né cần nhìn rõ nhân vật và hướng. Làm:
1. Đưa `characterScale` (và `weaponScale`, `hitRingScale`) vào `GameConfig`; **bán kính trúng theo luật không đổi** (chỉ đổi kích thước hình), vòng chân thể hiện đúng bán kính trúng.
2. Cho phép chọn qua URL (`?charScale=1.6`, `2.0`, `2.4`) và chụp **3 phương án** ở viewport điện thoại **1170×2532** (WebGL Playwright, pha Đặt và pha Ném, có Bot) để chủ dự án chọn. Mặc định đề xuất ≈ 12–14 % chiều cao sân.
3. Phóng to tương ứng dép/vệt ngắm/thẻ HP để cân đối; ngón tay không che mất nhân vật (kiểm tra ảnh: vệt ngắm, vòng đặt vị trí).

**V5. HUD khó đọc.**
- Chữ "VÒNG 3/5" nằm đè lên cờ, gần như không đọc được (ảnh 2, 3): đưa vào một nhãn nền tối ở giữa trên, cỡ chữ lớn.
- Nút **"SẴN SÀNG!" có chữ lặp/chồng** (ảnh 1, 3 thấy hai lớp chữ): sửa để có **một** lớp chữ, hiện đúng trạng thái (sáng/mờ khi đã khóa).
- Thẻ HP "BẠN ♥♥♥ / BOT ♥♥♥" chữ nhỏ: tăng cỡ; chữ tối thiểu ≈ 2,2 % chiều cao màn hình.
- Viền đen trên và dưới màn hình (khoảng 12 %): dùng cho HUD/ô vũ khí hoặc phủ nền, không để trống.

**V6. Cảnh trong ảnh 1 và 3 vẫn thấy Bot chưa vào sân đúng**: kiểm tra vị trí Bot, tên "BOT" thay vì tên nhân vật; nhân vật của Bot phải cùng tỉ lệ, đúng sprite (xem V1).

## Quy tắc bằng chứng mới (bắt buộc)
1. **Ảnh chụp WebGL (không phải Editor), viewport 1170×2532 và 1080×2340**, cho từng pha: Đặt (đang kéo ngắm, có vệt), Khóa, Lộ diện, Ném (đang bay), Kết quả vòng, Kết thúc trận (bảng hạng) — ở **N=2 và N=4**, có Bot. Mỗi ảnh có sidecar `.json` do công cụ sinh.
2. Thêm **kiểm tra tự động "sprite sạch"**: bản build ghi (chế độ debug, tắt ở Release) danh sách tên sprite đang vẽ mỗi khung; Playwright thu danh sách sau 3 ván và đối chiếu với danh sách đã duyệt. Dán kết quả (0 sprite ngoài danh sách).
3. Ghi tiêu đề từng ảnh vào `docs/PHASE2_REPORT.md` với dòng "Chủ dự án duyệt: CHƯA / ĐÃ".
4. **Không** đẩy lên cổng cũ. Dùng cổng mới (8093); dán URL LAN chính xác (IPv4 từ `ipconfig`, port) để chủ dự án thử lại.

## Còn nợ từ round26 (làm sau V1–V6)
- **G1–G6 chưa thấy báo cáo**: `.gitattributes`, thư mục build/`LEGACY`, `MatchRanking` biến thể ngoài luật (mặc định tắt hoặc xóa), một XML chuẩn + lưu trữ, sidecar mọi ảnh, không tự sửa `tools/audit`.
- **F9:** hai test bạn nêu (xác định theo seed, AutoPlay) **không** chứng minh bot không thấy vị trí/hướng người chơi trước khóa. Viết test đúng: đổi vị trí/hướng của người chơi giữa hai lần chạy cùng seed thì quyết định của bot trong vòng đó **không đổi**. Tên test + kết quả.
- **Mã HEAD và tên báo cáo audit không khớp** (`bc1dfc6`, `28c1637`, build `f14c587`): sau khi mọi thứ xong, báo đúng **một** HEAD và dấu vân tay build bằng đúng commit mã (docs/manifest đến sau thì ghi rõ).
- 10 trận trên build mới: `AvgFPS=56.7` trên PC. Ghi con số này như đo được; không viết "mượt 60".

## Không làm
Không gắn `v0.2-rc1`; không viết mã mạng; không đổi luật/bán kính trúng; không sửa `tools/audit`; không tuyên bố "đạt" khi chưa có ảnh WebGL và chưa có duyệt của chủ dự án.

## Mẫu báo cáo
```
VIỆC: V1…V6 | bằng chứng | nợ round26
ĐÃ LÀM: ...
BẰNG CHỨNG: (đường dẫn ảnh WebGL + sidecar, tên test + kết quả, ảnh ghép kiểm định)
CHƯA LÀM / THỪA NHẬN SAI: ...
CHỜ CHỦ DỰ ÁN: chọn tỉ lệ nhân vật (3 ảnh), duyệt hình ảnh
HEAD + build + URL LAN: ...
```
