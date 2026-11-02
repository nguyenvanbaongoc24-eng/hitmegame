# HIT ME – BÀN GIAO CHO ANTIGRAVITY: đóng Phase 1 và rà soát Phase 2

> Codex đã hết token, Antigravity (AG) tiếp nhận. **Bạn chưa có ngữ cảnh trước đó**, nên làm đúng thứ tự dưới đây. Làm từng bước nhỏ, **commit sau mỗi bước** để lỡ hết token vẫn không mất việc. Không báo "xong" khi thiếu bằng chứng.

---

## 0. Bối cảnh ngắn

- Game mobile "Hit Me" (Unity, 2D/2.5D, thị trường Việt Nam). Tối đa 6 người, mỗi người 3 HP.
- Thư mục dự án: `D:\HITME GAME` (Unity project ở `unity-client`, tài liệu ở `MD`, bằng chứng ở `unity-client\Evidence`). Hãy xác nhận đúng đường dẫn trước khi làm.
- Repo GitHub: `HIT-ME`. Tag hiện tại: `v0.1-graybox` (commit `9d2e08c`).
- Phase 1 (graybox offline, 1 người + 5 bot) gần xong: **17/17 test pass**, AutoPlay 10/10 không lỗi, đã có 4 screenshot. Báo cáo ở `MD/PHASE1_REPORT.md`.

### Luật game (đã chốt, KHÔNG tự đổi)
1. Mỗi vòng có **pha đặt 5 giây**: người chơi **đặt nhân vật ở bất kỳ đâu** trong sân, rồi **nhấn giữ nhân vật và lướt để kéo thước ngắm**. Chỉ chọn hướng, không có lực.
2. Trong pha đặt **không ai thấy vị trí/hướng của người khác**.
3. Hết 5 giây (hoặc tất cả đã bấm SẴN SÀNG) thì **khóa**, tất cả nhân vật **lộ diện đồng thời**, rồi **tự động ném** vũ khí cùng lúc theo hướng thước ngắm cuối cùng. Vũ khí bay thẳng tới tường sân, **dừng ở người đầu tiên trúng** (tham số `maxHitsPerThrow`).
4. Trúng = mất 1 HP; HP về 0 = bị loại. Trận kết thúc khi còn ≤ 1 người; hết `maxRounds` (5) mà còn nhiều người thì xếp theo HP.
5. Cử chỉ: chạm chỗ trống để đặt lại vị trí; giữ nhân vật + lướt để ngắm; lướt dưới `minAimDrag` thì không tính; kéo ngược về sát nhân vật thì hủy hướng.
6. Nút SẴN SÀNG = khóa sớm (không bắt buộc).

### Nguyên tắc cứng
- Không xóa `ASSET`, `SFX`, `SpriteSources`, `Evidence`, `MD`. Không sửa `LocalBot.unity`.
- **Không sửa logic combat ở `Core/`** trừ khi tìm ra lỗi thật; nếu sửa phải chạy lại toàn bộ test và nêu lý do.
- **Không tạo file .md mới** ngoài những file được nêu trong từng bước.
- Mọi kết luận phải kèm **bằng chứng** lưu trong `Evidence/`.
- Điểm luật mơ hồ → **dừng và hỏi chủ dự án**, không tự đoán.

---

## BƯỚC A – Kiểm tra hiện trạng (chỉ đọc)

A1. Đọc `MD/PHASE1_REPORT.md` (đặc biệt: luật đang áp dụng, `GameConfig`, bảng DoD).
A2. Chạy `git status`, `git log -5`, `git tag`; xác nhận tag `v0.1-graybox` nằm ở commit mới nhất và đã push. Nếu có thay đổi chưa commit thì báo, **không** tự xóa.
A3. Mở project Unity, **kiểm tra biên dịch sạch** và chạy lại toàn bộ EditMode test (kỳ vọng 17/17 pass). Báo kết quả.
A4. Mở 4 ảnh trong `Evidence/phase1/` (`shot_1_placing`, `shot_2_reveal`, `shot_3_throwing`, `shot_4_result`) và mô tả ngắn từng ảnh. Có đúng pha, có thước ngắm trong Placing, có bảng kết quả + nút Chơi lại trong Result không? Thiếu thì ghi rõ.

Báo cáo ngắn 10–15 dòng cho chủ dự án, rồi sang Bước B.

## BƯỚC B – Kiểm tra nhập liệu CẢM ỨNG (việc chính còn lại của Phase 1)

Hiện mới kiểm tra bằng chuột; cử chỉ giữ-rồi-lướt trên ngón tay thật chưa được thử.

B1. Cho biết project đang dùng **Input Manager cũ hay Input System**. Đảm bảo **chuột và cảm ứng cùng đi qua một lớp "pointer"** và đẩy vào cùng máy trạng thái cử chỉ đã có (không viết hai đường xử lý riêng).
B2. Nếu Unity bản này hỗ trợ, bật **Input System + Device Simulator** để giả lập cảm ứng trong Editor và thử các thao tác: chạm đặt lại, giữ + lướt ngắm, lướt dưới ngưỡng, kéo ngược hủy, đa điểm chạm (một ngón thứ hai không được làm hỏng thao tác ngắm). Nếu không bật được thì nêu lý do và đề xuất cách khác.
B3. **Che ngón tay:** khi ngón che nhân vật, thước ngắm có nhìn rõ không? Đề xuất/triển khai: vòng cảm ứng lớn hơn nhân vật, thước kéo dài ra ngoài ngón tay, hiển thị hướng bằng vệt chấm rõ ràng. Kiểm tra `minAimDrag` (0.25 đơn vị thế giới) có hợp lý trên màn khoảng 6 inch không và đề xuất giá trị nếu cần (chỉnh trong `GameConfig`, không hard-code).
B4. **Build để thử trên điện thoại thật:** ưu tiên WebGL (mở qua mạng nội bộ) hoặc Android APK. Viết `Evidence/phase1/touch_test.md` (tối đa 15 dòng): cách cài/mở và danh sách thao tác cần thử. **Chủ dự án sẽ thử trên điện thoại.**
B5. Chạy lại toàn bộ test, nếu có sửa code thì thêm test cho phần mới. Commit.
B6. Báo trung thực: cái gì đã thử trên thiết bị thật, cái gì mới giả lập.

## BƯỚC C – Luật "chưa ngắm thì sao?" (CHỜ CHỦ DỰ ÁN QUYẾT)

Codex đang áp dụng: **hết giờ mà chưa ngắm/đã hủy ngắm thì ném theo hướng mặt hiện tại (mặc định hướng lên)**. Đây chưa được chủ dự án duyệt.
- **Không đổi** hành vi này trước khi có câu trả lời.
- Chuẩn bị sẵn để đổi dễ: đưa lựa chọn vào `GameConfig` dạng enum `NoAimPolicy { ThrowFacing, NoThrow, RepeatLastAim }`, mặc định `ThrowFacing`, kèm test cho cả 3 giá trị.
- Ghi vào `PHASE1_REPORT.md` mục "Quyết định luật chờ duyệt" cho đủ: chưa ngắm, chủ động hủy ngắm, vòng 1 chưa có hướng cũ.

## BƯỚC D – Đóng Phase 1

D1. Cập nhật bảng DoD trong `MD/PHASE1_REPORT.md`; chỉ ghi **ĐẠT** khi có bằng chứng. Mục "chơi tay" và "thử cảm ứng trên máy thật" ghi **"chờ chủ dự án xác nhận"**.
D2. Commit, dời tag `v0.1-graybox` về commit cuối nếu có thay đổi, push.
D3. Báo cáo ngắn: đã làm gì, còn gì chờ chủ dự án.

> ⛔ **DỪNG sau Bước D.** Chờ chủ dự án xác nhận (chơi tay, thử cảm ứng, chọn luật ở Bước C) trước khi sang Phase 2.

---

## BƯỚC E – Phase 2, chỉ phần RÀ SOÁT (Bước 0), không sửa code

Chủ dự án sẽ đính kèm file prompt Phase 2 (`hit-me-phase2-prompt.md`). Nếu chưa thấy file đó trong `MD/` hoặc trong tin nhắn, **hỏi chủ dự án** và dừng.

Khi có file: chỉ làm **BƯỚC 0 – RÀ SOÁT** của Phase 2 và viết `MD/PHASE2_AUDIT.md`. Bổ sung hai điểm sau vào bản rà soát:
1. **Cơ chế điều khiển đã chốt** (xem mục "Luật game" ở trên): hình nón + vệt chấm + mũi tên ngắm trong Phase 2 phải **bám theo thước ngắm kéo ra**, không dùng nút chọn hướng; thước phải nhìn rõ khi ngón tay che một phần.
2. Pha đặt chỉ hiện nhân vật của chính người chơi; các nhân vật khác chỉ xuất hiện ở pha Lộ diện.

Sau đó **DỪNG**, chờ chủ dự án duyệt `PHASE2_AUDIT.md`. Không làm Bước 1 trở đi của Phase 2 khi chưa được duyệt.

---

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn log/screenshot/test)
CHỜ CHỦ DỰ ÁN: ...
VẤN ĐỀ/CÂU HỎI: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```

## Điều cấm (nhắc lại)
Không đổi luật; không sửa Core combat tùy ý; không xóa `ASSET`/`SFX`; không tạo .md ngoài danh sách; không báo xong khi thiếu bằng chứng; không làm art/menu/mạng trong Phase 1.
