# HIT ME – BÀN GIAO CHO ANTIGRAVITY: rà soát Phase 2 (chỉ đọc) + phân tích nhịp độ trận

> Codex sắp hết token, AG tiếp nhận. **Bạn chưa có ngữ cảnh trước đó**, nên làm đúng thứ tự dưới đây. Làm từng bước nhỏ, **commit sau mỗi bước** (thông điệp bắt đầu bằng `WIP:` nếu dang dở). Không báo "xong" khi thiếu bằng chứng. Báo cáo ngắn gọn để tiết kiệm token.

---

## 0. Bối cảnh

- Game mobile "Hit Me" (Unity 6, 2D/2.5D, thị trường Việt Nam). Tối đa 6 người, mỗi người 3 HP.
- Thư mục dự án: `D:\HITME GAME` (Unity project: `unity-client`; tài liệu: `MD`; bằng chứng: `unity-client\Evidence`). Hãy xác nhận đường dẫn đúng trước khi làm.
- **Phase 1 đã đóng:** commit `9dcaf41`, tag `v0.1-graybox`, 24/24 EditMode test pass, AutoPlay 10/10 không lỗi, **đã test trên iPhone (Safari, WebGL qua LAN): cảm ứng ĐẠT**. Báo cáo: `MD/PHASE1_REPORT.md`.

### Luật game (đã chốt, KHÔNG tự đổi)
1. Mỗi vòng có **pha đặt 5 giây**: người chơi **đặt nhân vật ở bất kỳ đâu** trong sân, rồi **nhấn giữ nhân vật và lướt để kéo thước ngắm**. Chỉ chọn hướng, không có lực.
2. Trong pha đặt **không ai thấy vị trí/hướng của người khác**; chỉ thấy nhân vật của chính mình.
3. Hết 5 giây (hoặc tất cả bấm SẴN SÀNG = khóa sớm) thì **khóa**, tất cả **lộ diện đồng thời**, rồi **tự động ném** cùng lúc theo hướng thước ngắm cuối. Vũ khí bay thẳng tới tường, **dừng ở người đầu tiên trúng** (`maxHitsPerThrow`).
4. Trúng = mất 1 HP; HP về 0 = bị loại. Trận kết thúc khi còn ≤ 1 người.
5. Cử chỉ: chạm chỗ trống để đặt lại; giữ nhân vật + lướt để ngắm; lướt dưới `minAimDrag` không tính; kéo ngược về sát nhân vật thì hủy hướng.

### Nguyên tắc cứng
- Không xóa `ASSET`, `SFX`, `SpriteSources`, `Evidence`, `MD`. Không sửa `LocalBot.unity`.
- **Không sửa logic Core**, không đổi luật khi chưa có quyết định của chủ dự án (xem mục "QUYẾT ĐỊNH").
- **Không tạo file .md mới** ngoài `MD/PHASE2_AUDIT.md` (và ghi chú nhỏ vào `PHASE1_REPORT.md` nếu cần).
- Mọi kết luận kèm **bằng chứng** lưu trong `Evidence/`.
- Điểm mơ hồ → **dừng và hỏi chủ dự án**, không tự đoán.

---

## QUYẾT ĐỊNH CỦA CHỦ DỰ ÁN (chủ dự án điền trước khi gửi; để trống = CHƯA QUYẾT, giữ nguyên hiện trạng)

```
Số vòng / điều kiện kết thúc (a: còn 1 người, không giới hạn | b: tối đa N vòng rồi xếp theo HP | c: khác): ______
N vòng tối đa (nếu chọn b/c): ______
Luật khi hòa HP khi hết vòng: ______
Chính sách khi chưa ngắm (ThrowFacing | NoThrow | RepeatLastAim): ______
```

- Nếu ô nào có giá trị: chỉ chỉnh **tham số trong `GameConfig`** (`maxRounds`, `RoundLimitTiebreakPolicy`, `NoAimPolicy`) cho khớp, thêm/cập nhật test, rồi chạy lại toàn bộ test + AutoPlay.
- Nếu để trống: **không đổi**, chỉ ghi vào báo cáo "chờ chủ dự án".

---

## BƯỚC A – Kiểm tra hiện trạng (chỉ đọc)

A1. `git status`, `git log -5`, `git tag`: xác nhận tag `v0.1-graybox` ở commit `9dcaf41` (hoặc mới hơn) và đã push. Có thay đổi chưa commit thì báo, **không xóa**.
A2. Mở project, **biên dịch sạch**, chạy EditMode test (kỳ vọng 24/24).
A3. Đọc nhanh `MD/PHASE1_REPORT.md` (mục luật đang áp dụng, `GameConfig`, vấn đề đã biết chuyển sang Phase 2).

Báo cáo 8–10 dòng rồi sang Bước B.

## BƯỚC B – Phân tích nhịp độ trận (chỉ đọc, KHÔNG đổi luật)

**Vấn đề:** mô phỏng 1.000 trận bot vs bot cho **trung bình ≈ 25,8 vòng/trận, nhiều nhất 87 vòng**. `maxRounds=5` hiện chưa được áp dụng để kết thúc trận. Ván thật có thể kéo dài hơn nhiều so với kỳ vọng "ngắn, vài vòng" (HUD thiết kế ghi `VÒNG x/5`).

Làm các việc sau, kết quả ghi trong `MD/PHASE2_AUDIT.md` mục "Nhịp độ trận" (bảng + khuyến nghị, tối đa 1 trang):
B1. Xác định nguyên nhân gốc bằng số liệu: xác suất một lượt ném trúng ai đó (theo số người còn sống), kích thước sân (`arenaWidth × arenaHeight`), `playerRadius`, và cách `BotBrain` chọn vị trí/hướng. Chạy thêm mô phỏng nếu cần (cờ/tham số riêng, **không sửa mặc định**).
B2. Mô phỏng thử **các phương án** (mỗi phương án 1.000 trận, ghi số vòng trung bình / lớn nhất / phân bố người thắng) và lập bảng so sánh:
   - Đặt `maxRounds` = 5, 8, 10 (kèm luật xếp hạng theo HP; hòa thì theo số lần trúng, vẫn hòa thì hòa).
   - Thu nhỏ sân (ví dụ giảm 20–30%) hoặc tăng `playerRadius`.
   - Bot mức "thông minh" hơn (đoán vị trí theo các vòng trước, ném về phía trung tâm).
   - Sân thu hẹp dần (sudden death) sau vòng N — **chỉ phân tích, không cài đặt**.
B3. Đưa khuyến nghị 1 phương án chính + 1 dự phòng, kèm ước lượng thời gian ván (số vòng × thời gian mỗi vòng).
**Không sửa code luật**, chỉ khuyến nghị. Chủ dự án quyết định.

## BƯỚC C – Rà soát Phase 2 (chỉ làm BƯỚC 0 của file `hit-me-phase2-prompt.md`)

Chủ dự án sẽ đính kèm `hit-me-phase2-prompt.md`. Nếu chưa thấy file trong `MD/` hoặc trong tin nhắn thì **hỏi và dừng**. Khi có file: chỉ làm **BƯỚC 0 – RÀ SOÁT**, không sửa code, kết quả vào `MD/PHASE2_AUDIT.md`. Bổ sung các điểm sau:

1. **Cơ chế điều khiển đã chốt** (xem "Luật game"): hình nón + vệt chấm + mũi tên ngắm trong Phase 2 phải **bám theo thước ngắm kéo ra**; thước nhìn rõ khi ngón che một phần (vòng cảm ứng lớn hơn nhân vật, thước kéo dài ra ngoài ngón).
2. Pha đặt chỉ hiện nhân vật của chính người chơi; các nhân vật khác chỉ xuất hiện ở pha Lộ diện.
3. **Vấn đề đã thấy trên iPhone (WebGL):**
   - Chữ tiếng Việt **mất dấu** (font mặc định thiếu glyph). Kế hoạch: TextMeshPro + font có đủ tiếng Việt, **nạp sẵn (static atlas) toàn bộ ký tự tiếng Việt**; không dựa vào font hệ thống vì không dùng được trên WebGL. Nêu font nào trong `ASSET` dùng được, nếu thiếu thì ghi vào danh sách asset thiếu.
   - HUD chữ rất nhỏ ở góc trái, dòng cuối bị khung sân che; bảng kết quả đè lên nhãn bot. Ghi vào kế hoạch Bước 3 (HUD) của Phase 2, dùng Safe Area và Canvas Scaler đúng.
   - WebGL trên Safari: giữ WebGL template `Phase1Mobile` (`touch-action: none`, chặn cuộn, cache-busting), tái sử dụng cho Phase 2.
4. **Phạm vi hiệu năng WebGL/iPhone:** nêu rủi ro (kích thước build, thời gian tải, bộ nhớ texture trên Safari) và cách đo.
5. Danh sách asset thiếu / cần xử lý, bảng bố cục đo từ mockup 1, rủi ro, kế hoạch Bước 1–6.

Sau khi viết xong `PHASE2_AUDIT.md`: commit, báo cáo ngắn, rồi **DỪNG**, chờ chủ dự án duyệt. **Không làm Bước 1 trở đi của Phase 2 khi chưa được duyệt.**

---

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn log/số liệu/screenshot)
CHỜ CHỦ DỰ ÁN: ...
VẤN ĐỀ/CÂU HỎI: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```

## Điều cấm (nhắc lại)
Không đổi luật khi chưa có quyết định; không sửa Core combat tùy ý; không xóa `ASSET`/`SFX`; không tạo .md ngoài danh sách; không báo xong khi thiếu bằng chứng; không làm art/menu/mạng trong giai đoạn rà soát.
