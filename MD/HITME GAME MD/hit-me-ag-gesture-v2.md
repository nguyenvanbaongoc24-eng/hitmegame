# HIT ME – LỆNH CHO ANTIGRAVITY: áp luật điều khiển v1.2 (làm trước khi gắn `v0.2-rc1`)

> Dán cho AG **cùng file** `hit-me-gameplay-rules-v1.2.md` (nguồn sự thật, đọc kỹ mục 2–7). Luật điều khiển đã đổi so với bản AG đang cài. **Commit sau mỗi bước** (`WIP:` nếu dang dở). Không tự chấm "đạt" khi thiếu bằng chứng.

## 0. Quy tắc
- Không xóa `ASSET`, `SFX`, `Evidence`, `MD`; không sửa `LocalBot.unity`.
- **Không sửa logic combat ở Core.** Lần này **được phép** thay `PlacementGestureState` (máy trạng thái cử chỉ) và test của nó, vì đây là phần nhập liệu, không phải luật chiến đấu. Mã cử chỉ cũ chuyển vào `Assets/_Archive/`, không xóa hẳn.
- Không tạo .md mới. Cập nhật các tài liệu đã có: `MD/01_GAMEPLAY_RULES.md` (chỉ thay các mục điều khiển/pha đặt bằng nội dung v1.2, **giữ nguyên phần còn lại**) và `MD/PHASE2_REPORT.md`.
- Phần đang dang dở trong `hit-me-ag-close-phase2-open-phase3.md` vẫn có hiệu lực; việc này là một phần của **mục 8 (thước ngắm)** trong bảng đối chiếu DoD.

## BƯỚC 1 – Xác nhận hiện trạng cử chỉ (chỉ đọc, ngắn)
Báo cáo 8–10 dòng: file nào xử lý cử chỉ hiện nay (`PointerSample`, `PlacementGestureState`, `PlacementInput`, `AimInput`…), cử chỉ cũ đang làm gì, chỗ nào mâu thuẫn với v1.2 (nhấn giữ trên nhân vật mới ngắm; chạm chỗ trống mới đặt; hướng tính ở tọa độ màn hình hay tọa độ game?). Hướng ngắm hiện đang đi qua `ScreenToGame` (homography) chưa?

## BƯỚC 2 – Máy trạng thái cử chỉ v1.2 (C# thuần)
- Nếu trên nhánh `prep/codex` đã có `tools/patches/gesture_v2/` (Codex viết sẵn lớp thuần + test + bộ phát lại sự kiện), **dùng làm gốc**: chép vào `Scripts/Core/Phase1` (hoặc nơi `PlacementGestureState` đang nằm), nối với `PointerSample`, rồi chuyển test sang NUnit EditMode. Chưa có thì tự viết theo `hit-me-gameplay-rules-v1.2.md`.
- Giữ nguyên hình dạng giao diện nếu được (`PointerSample`, pha chạm), thêm: `Snapshot` gồm `hasPlaced`, `position`, `facing`, `aimActive`, `gestureId`.
- **15 test** ở mục 6 của file luật phải pass; chạy lại toàn bộ test cũ.
- Hướng ngắm = `chuẩn_hóa(ScreenToGame(ngón) − ScreenToGame(điểm chạm))`. Ngưỡng `minAimDrag` đổi từ mm sang pixel bằng `Screen.dpi` (dự phòng 160), so khoảng cách **trên màn hình**. Nếu homography chưa có, dùng ánh xạ hiện tại và ghi rõ vào báo cáo.
- Hằng số hướng nhìn mặc định (`DefaultFacing = hướng lên`) đặt **một chỗ trong Core**, dùng chung cho client và server.

## BƯỚC 3 – Nối vào màn trận (View)
1. **Đầu vòng:** sân trống + chấm mờ gợi ý vị trí vòng trước + chuỗi "Chạm để đặt nhân vật". Nhân vật chưa hiện cho tới lần chạm đầu.
2. **Chạm xuống trong sân:** nhân vật hiện/dời tới điểm chạm (hiệu ứng nhảy ≤ 0,15 s), vòng sáng mỏng ≥ 9 mm quanh điểm chạm.
3. **Giữ + trượt:** thước ngắm (vệt chấm + hình nón + mũi tên) theo hướng ném thật, kéo dài ra ngoài ngón, vẽ trong sân; nhân vật quay mặt theo hướng. Nhân vật **không** chạy theo ngón.
4. **Nhả ngón:** thước giữ nguyên tới lúc khóa. **Chạm chỗ khác:** làm lại hoàn toàn: dời nhân vật tức thì, hướng reset về mặc định (lên), xóa thước cũ, rồi giữ + trượt lại như chọn vị trí mới.
5. Chạm bắt đầu trên HUD/nút không đặt nhân vật; kéo ra ngoài sân vẫn tính hướng; ngón thứ hai bị bỏ qua.
6. Hết 5 giây: khóa và tự ném. Không chạm: đứng vị trí vòng trước (vòng 1 ngẫu nhiên), ném theo hướng mặc định.
7. Chuỗi chữ hướng dẫn (VI/EN, bảng ngôn ngữ): "Chạm để đặt nhân vật", "Giữ và kéo để chọn hướng ném", "Chạm chỗ khác để đặt lại · Giữ và kéo để đổi hướng".
8. Bí mật vị trí không đổi: pha đặt chỉ thấy nhân vật của mình.
9. `commit` gửi `aimDir = null` khi chưa từng ngắm.

## BƯỚC 4 – Bằng chứng
- Phát lại chuỗi sự kiện giả lập (chuột và cảm ứng giả lập) cho 6 tình huống: đặt+ngắm; đặt rồi dời; ngắm rồi hủy; không chạm; không trượt; chạm ngoài sân. Lưu `Evidence/phase2/gesture_*.png` (mỗi tình huống một ảnh) và một video ≤ 20 giây `gesture_demo.mp4`.
- Ảnh 3 trạng thái chuỗi hướng dẫn trên WebGL (chưa chạm / đã đặt / đã ngắm), chữ đủ dấu.
- Kết quả test (số lượng pass), AutoPlay 10 trận không lỗi (bot gửi `commit` trực tiếp nên không đi qua cử chỉ).
- Build WebGL, giữ template `Phase1Mobile` và cache-busting; **chủ dự án sẽ thử trên iPhone** (Safari: `touch-action: none`, không cuộn trang khi kéo).
- Cập nhật `MD/01_GAMEPLAY_RULES.md` (v1.2) và bảng DoD (mục 8 thước ngắm).

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn ảnh/video/test)
KHÔNG ĐẠT / CÒN THIẾU: ...
CHỜ CHỦ DỰ ÁN: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```

## Điều cấm (nhắc lại)
Không sửa logic combat; không xóa `ASSET`/`SFX`/`Evidence`/`MD`; không tạo .md mới; không để nhân vật chạy theo ngón khi ngắm; không hiện nhân vật khác trong pha đặt; không báo xong khi thiếu bằng chứng.
