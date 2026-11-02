# HIT ME – PHASE 3: Menu & các màn hình ngoài trận (UI bằng prefab, dữ liệu giả)

> Dán toàn bộ file này cho Codex / Antigravity. **Chỉ bắt đầu khi Phase 2 đã xong** (tag `v0.2-feel`, test pass, `PHASE2_REPORT.md` đã được duyệt). Làm đúng thứ tự các bước.

---

## 0. Mục tiêu & ranh giới

**Mục tiêu:** dựng toàn bộ luồng màn hình ngoài trận đấu, **khớp mockup Home và Vào phòng**, bấm được hết, chuyển màn mượt, và **vào được trận** (Luyện với bot → màn trận Phase 2). Dữ liệu còn là **dữ liệu giả** (chưa mạng, chưa Supabase).

**Giữ nguyên:** `Core/`, màn trận đấu Phase 2, toàn bộ test cũ. Phase 3 chỉ thêm lớp UI/điều hướng và các "dịch vụ giả" có giao diện (interface) để Phase 4–5 thay bằng bản thật.

**Không làm trong Phase 3:** mạng, server, matchmaking thật, Supabase, đăng nhập thật (Google/Facebook/Apple), mua bán thật, chat thật, bản đồ/nhân vật mới ngoài MVP.

**Nguyên tắc cứng:**
1. Không xóa `ASSET`, `SFX`, `SpriteSources`, `Evidence`, `MD`. Không sửa `LocalBot.unity`, không phá `Graybox`/màn trận.
2. **Không tạo file .md mới** ngoài `MD/PHASE3_AUDIT.md` và `MD/PHASE3_REPORT.md`.
3. **Mỗi màn hình = 1 prefab.** UI bằng prefab + 9-slice + TextMeshPro, **không cắt từ ảnh mockup phẳng**, **không tạo UI hoàn toàn bằng code**.
4. Mọi chuỗi chữ nằm trong **bảng ngôn ngữ VI/EN**; mọi màn có Safe Area.
5. Mỗi bước có **bằng chứng** (screenshot, video, log) lưu `Evidence/phase3/`.
6. Màn nào **chưa có mockup**: không tự bịa lung tung (xem Bước 0.4).

---

## BƯỚC 0 – RÀ SOÁT (chỉ đọc, chưa sửa)

0.1. Xác nhận Phase 2: tag `v0.2-feel` có mặt, chạy lại toàn bộ test, báo kết quả. Tạo tag `backup-before-phase3`.

0.2. Kiểm kê asset UI cho menu trong `ASSET` (nút vàng/xanh dương/xanh lá, khung gỗ có tiêu đề, khung tối viền vàng, tab, ô nhập, icon: cài đặt, bút sửa, dấu +, đồng xu, mắt ẩn, khóa, dán, QR, người dùng+, cúp, hộp quà, nhà, robot, mũi tên…). Đánh dấu: dùng được / cần xử lý (cắt 9-slice, đặt border) / **thiếu** / lỗi. Kiểm tra font tiếng Việt cho cả tiêu đề (kiểu gỗ-vàng) và chữ thường.

0.3. Đo mockup thành **bảng bố cục + bảng thành phần** cho 2 màn có mockup:
- **Home:** hồ sơ (avatar, "Hồ sơ" + bút), số vàng `1.250` + nút `+`, nút cài đặt; logo HIT ME; nhân vật trưng bày giữa bệ tròn với 2 mũi tên trái/phải; 2 thẻ **Nhân vật** (Nam Làng) và **Vũ khí** (Dép, mũi tên 2 bên); nút lớn vàng **CHƠI NHANH** ("Online • Tối đa 6 người"); **TẠO PHÒNG** (xanh dương) và **VÀO PHÒNG** (xanh lá); **LUYỆN VỚI BOT**; thanh dưới 3 tab **Trang chủ / Xếp hạng / Nhiệm vụ**.
- **Vào phòng:** nút quay lại + nút thêm bạn; 2 tab **Mã phòng / Công khai**; khung **NHẬP MÃ PHÒNG** (ô mã `HM4826` + nút xóa, nút **Dán mã**, **Quét mã QR**, dòng "Đã tìm thấy phòng"); khung **THÔNG TIN PHÒNG** (tên phòng + nhãn Riêng tư/Công khai, chủ phòng + `4/6 người`, ảnh bản đồ "Sân làng", hàng "Thời gian ngắm 5 giây", hàng "Vị trí đối thủ: Ẩn khi ngắm"); khung **Bạn sẽ tham gia với** (nhân vật + vũ khí); nút vàng **VÀO PHÒNG**; dòng nhỏ "Chưa có mã? Nhờ bạn bè gửi lời mời."
- Ghi tỉ lệ vị trí/kích thước trên màn dọc 9:19.5 và vùng an toàn.

0.4. **Danh sách màn chưa có mockup.** Cần đủ cho luồng, nhưng chưa có hình: **Tạo phòng**, **Sảnh chờ** (sau khi vào/tạo phòng), **Chọn nhân vật**, **Chọn vũ khí**, **Hồ sơ/đổi tên**, **Cài đặt**, tab **Xếp hạng**, tab **Nhiệm vụ**, **Công khai** (danh sách phòng), hộp thoại thiếu vàng/lỗi. Với mỗi màn: agent đề xuất **wireframe bằng chữ** (danh sách thành phần + bố cục) dựa trên phong cách Home/Vào phòng, để chủ dự án duyệt. **Không tự dựng khi chưa duyệt wireframe.**

0.5. Sơ đồ điều hướng đề xuất: `Boot → Home → { Chơi nhanh | Tạo phòng | Vào phòng | Luyện với bot | Nhân vật | Vũ khí | Hồ sơ | Cài đặt | Xếp hạng | Nhiệm vụ }`; `Tạo phòng/Vào phòng → Sảnh chờ → Trận`; `Trận → Kết quả → Home`.

0.6. Viết `MD/PHASE3_AUDIT.md`: kiểm kê asset UI, 2 bảng bố cục, danh sách asset thiếu, **wireframe các màn chưa có mockup**, sơ đồ điều hướng, rủi ro, kế hoạch Bước 1–6, và **câu hỏi cần xác nhận** (mục 2).

> ⛔ **DỪNG tại đây.** Gửi báo cáo cho chủ dự án và chờ xác nhận (đặc biệt là wireframe).

---

## 2. CÂU HỎI CẦN CHỦ DỰ ÁN XÁC NHẬN
- **A. Chơi nhanh** ở Phase 3 làm gì? Mặc định: hiện màn "Đang tìm trận…" giả trong 2–3 giây rồi vào trận với bot (đánh dấu rõ là giả, Phase 4 thay bằng matchmaking thật).
- **B. Tạo phòng** gồm những lựa chọn nào? Mặc định theo thông tin ở màn Vào phòng: tên phòng, Riêng tư/Công khai, số người tối đa (2–6), bản đồ (Sân làng), thời gian ngắm (mặc định 5 giây), vị trí đối thủ (Ẩn khi ngắm). Phòng riêng sinh **mã 6 ký tự** dạng `HM4826`.
- **C. Sảnh chờ**: hiển thị danh sách người chơi, chủ phòng có nút **Bắt đầu**, người khác có nút **Sẵn sàng**; ở Phase 3 người khác là bot giả. Đồng ý không?
- **D. Quy tắc mã phòng:** định dạng `HM` + 4 số? Có phân biệt hoa/thường? Mã không tồn tại, phòng đầy, phòng đã bắt đầu thì thông báo lỗi nào?
- **E. Kho đồ:** vũ khí và nhân vật nào **mở sẵn**? Mặc định: Nam Làng + Dép mở sẵn; các nhân vật còn lại hiện **khóa** kèm giá bằng vàng (chưa mua được thật).
- **F. Xếp hạng / Nhiệm vụ** ở Phase 3: chỉ làm danh sách dữ liệu giả (có thể cuộn) để kiểm tra giao diện?

---

## BƯỚC 1 – Hạ tầng UI & điều hướng

- `UIScreen` (lớp cơ sở: `Show/Hide`, hiệu ứng vào/ra), `ScreenManager` (đăng ký màn, ngăn xếp quay lại, phát sự kiện), hỗ trợ **nút Back của Android**.
- Scene chính `Boot.unity` → `Main.unity` (chứa mọi màn UI dạng prefab, tải theo nhu cầu). Chuyển Main ↔ Trận bằng tải scene (có màn tải ngắn).
- **Dịch vụ giả có interface** (để Phase 4–5 thay): `IProfileService` (tên, avatar, vàng, nhân vật/vũ khí đang chọn, đã mở khóa), `IRoomService` (tìm phòng theo mã, tạo phòng, danh sách công khai), `ISettingsService`. Cài đặt lưu `PlayerPrefs`; hồ sơ giả lưu JSON cục bộ.
- Bảng ngôn ngữ VI/EN đầy đủ cho mọi chữ; **công tắc VI/EN hoạt động ngay** trong Cài đặt và đổi chữ tức thì.
- Thành phần UI dùng lại: `HmButton` (3 kiểu màu, trạng thái nhấn/vô hiệu, âm bấm), `HmPanel`, `HmTab`, `HmInputField`, `HmToast`, `HmDialog`, `HmCurrencyBadge`.

**Xong khi:** chuyển được qua lại giữa các màn rỗng bằng nút và nút Back Android; công tắc VI/EN đổi chữ ngay.

## BƯỚC 2 – Màn Home (khớp mockup)

Dựng đúng thành phần ở Bước 0.3: hồ sơ, vàng, cài đặt, logo, nhân vật trưng bày trên bệ (idle animation, **đổi nhân vật bằng 2 mũi tên**), thẻ Nhân vật / Vũ khí (mũi tên chọn, mở màn chọn), CHƠI NHANH, TẠO PHÒNG, VÀO PHÒNG, LUYỆN VỚI BOT, thanh dưới 3 tab với tab đang chọn sáng.
- Nền sân làng: ghép từ layer asset (như Phase 2), có chuyển động nhẹ (cờ, hoa, nhân vật thở).
- **LUYỆN VỚI BOT** → vào màn trận Phase 2 thật (1 người + 5 bot), xong trận về Home.

**Xong khi:** screenshot Home cạnh mockup khớp bố cục (sai số ≤ 3%), mọi nút phản hồi; Luyện với bot chơi được trọn trận và quay về.

## BƯỚC 3 – Màn Vào phòng & Tạo phòng & Sảnh chờ

- **Vào phòng** (khớp mockup): 2 tab; nhập mã có xác thực ngay khi gõ (đủ 6 ký tự → tra `IRoomService` giả), **Dán mã** đọc clipboard, **Quét mã QR** (Phase 3: hiện màn xin quyền camera + giao diện quét, kết quả giả hoặc nhập tay; ghi rõ là giả); khi tìm thấy hiện khung **Thông tin phòng** và nút **VÀO PHÒNG**; lỗi (mã sai, phòng đầy) hiện toast/hộp thoại đúng nội dung; ô mã có nút xóa.
- Tab **Công khai**: danh sách phòng giả có thể cuộn, mỗi dòng: tên phòng, số người `x/6`, bản đồ, nút vào.
- **Tạo phòng** (theo wireframe đã duyệt): các lựa chọn ở câu B, nút tạo → sinh mã 6 ký tự, vào Sảnh chờ, có nút **Sao chép mã** / chia sẻ.
- **Sảnh chờ** (theo wireframe đã duyệt): 6 chỗ, người chơi + bot giả, chủ phòng **Bắt đầu**, người khác **Sẵn sàng**, nút rời phòng. Bấm Bắt đầu → vào trận Phase 2.

**Xong khi:** đi trọn luồng `Home → Tạo phòng → Sảnh chờ → Trận → Kết quả → Home` và `Home → Vào phòng (mã HM4826) → Sảnh chờ → Trận`; màn Vào phòng khớp mockup (≤ 3%).

## BƯỚC 4 – Chọn nhân vật, chọn vũ khí, hồ sơ

- **Chọn nhân vật:** cuộn ngang 5 nhân vật, xem trước idle, hiện đã sở hữu/khóa + giá, nút **Chọn**; chưa mua được thật (hiện hộp thoại "Sắp có" hoặc trừ vàng giả nếu chủ dự án đồng ý).
- **Chọn vũ khí:** Dép mở sẵn; các vũ khí còn lại hiện khóa/"Sắp có" (đúng phạm vi MVP, không dựng thêm hiệu ứng).
- **Hồ sơ:** xem/đổi tên (lần đầu miễn phí, từ lần 2 hiện giá bằng vàng, chỉ hiện logic và hộp thoại xác nhận), chọn avatar.
- Lựa chọn nhân vật/vũ khí **được dùng trong trận** (người chơi vào trận bằng nhân vật đã chọn).

**Xong khi:** đổi nhân vật ở Home → vào Luyện với bot thấy đúng nhân vật đó; đổi tên được; dữ liệu còn sau khi tắt mở lại.

## BƯỚC 5 – Cài đặt, Xếp hạng, Nhiệm vụ, hộp thoại

- **Cài đặt:** nhạc, SFX (2 kênh), ngôn ngữ VI/EN, rung màn hình, hiển thị FPS (tùy chọn), chọn chất lượng, liên kết điều khoản/chính sách (placeholder), phiên bản app.
- **Xếp hạng:** danh sách giả có cuộn (hạng, avatar, tên, điểm), dòng của bạn ghim.
- **Nhiệm vụ:** danh sách nhiệm vụ giả (tiến độ, phần thưởng vàng, nút Nhận), cập nhật vàng giả.
- Hộp thoại/toast dùng chung: lỗi, xác nhận, thiếu vàng, mất kết nối (chuẩn bị cho Phase 4).

**Xong khi:** mọi màn mở được, cuộn mượt, không chữ bị tràn/cắt, đổi ngôn ngữ cập nhật mọi màn.

## BƯỚC 6 – Hiệu năng & kiểm thử

- Mục tiêu: ≥ 30 FPS (khuyến nghị 60) khi duyệt menu trên máy Android tầm trung; thời gian chuyển màn ≤ 0.5 giây; tải Home ≤ 3 giây.
- Chạy lại toàn bộ test Phase 1–2: **phải pass 100%**.
- Test mới: bảng ngôn ngữ không thiếu key, sinh/kiểm tra mã phòng (định dạng, trùng lặp), `ScreenManager` (ngăn xếp/Back), lưu/đọc cài đặt và hồ sơ, không có tham chiếu `Missing`.
- Kiểm tra trên **3 tỉ lệ màn** (9:16, 9:19.5, 9:21) và có tai thỏ; chữ tiếng Việt đủ dấu ở mọi màn.
- Đi thử toàn bộ luồng 5 lần liên tiếp, không lỗi, không rò rỉ bộ nhớ.

---

## TIÊU CHÍ XONG (Definition of Done)
- [ ] `MD/PHASE3_AUDIT.md` (kèm wireframe) được chủ dự án duyệt trước Bước 1.
- [ ] Project biên dịch sạch; test Phase 1–2 vẫn pass; test mới pass.
- [ ] Home và Vào phòng khớp mockup (≤ 3%), dựng bằng prefab + 9-slice, không cắt từ ảnh phẳng.
- [ ] Các luồng chạy được: Luyện với bot; Tạo phòng → Sảnh chờ → Trận; Vào phòng bằng mã → Sảnh chờ → Trận; Chơi nhanh (giả); Chọn nhân vật/vũ khí; Hồ sơ; Cài đặt; Xếp hạng; Nhiệm vụ.
- [ ] Nhân vật/vũ khí đã chọn được dùng trong trận.
- [ ] VI/EN đổi tức thì ở mọi màn; chữ đủ dấu.
- [ ] Dịch vụ giả nằm sau interface (`IProfileService`, `IRoomService`, `ISettingsService`) để Phase 4–5 thay thế.
- [ ] ≥ 30 FPS khi duyệt menu trên Android tầm trung (có số đo); chuyển màn ≤ 0.5s.
- [ ] Không xóa `ASSET`/`SFX`; không sửa `LocalBot.unity`.
- [ ] `MD/PHASE3_REPORT.md`: đã làm gì, bằng chứng, **danh sách asset thiếu**, phần nào là dữ liệu giả cần thay ở Phase 4–5, số đo hiệu năng, tồn đọng.
- [ ] Commit sạch, tag `v0.3-menu`.

## ĐIỀU CẤM (nhắc lại)
Không làm mạng/server/Supabase/đăng nhập thật; không sửa logic Core; không cắt UI từ mockup phẳng; không tự dựng màn chưa có wireframe được duyệt; không tự tạo asset giả để che chỗ thiếu; không viết thêm .md ngoài danh sách; không báo xong khi thiếu bằng chứng.

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (screenshot/video/log/test)
ASSET THIẾU: ...
DỮ LIỆU GIẢ CẦN THAY SAU: ...
VẤN ĐỀ/CÂU HỎI: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```
