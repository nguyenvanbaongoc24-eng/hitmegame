# HIT ME – PHASE 4, LƯỢT 2

Đầu vào: Antigravity commit `d6770c9`, 159/159 EditMode test. Báo cáo lượt này đủ minh bạch (có cột người thật/script, ghi rõ chỗ nào chưa đạt). Mình ghi nhận các điểm sau.

## Đánh giá nhanh báo cáo

| Mục | Nhận xét |
|---|---|
| K1–K5 | Đạt. Có bằng chứng người chơi thật: 2 người, 3 người, và 10 trận bộ nhớ ổn định (Wasm 98 MB không đổi). Client không lặp `room_ready`, lỗi trước là do bot. |
| 6 người thật | **Chưa đạt**: chỉ lên được 4/6 trình duyệt đồng thời do CPU giả lập. Phòng 6 người mới được kiểm bằng bot script, nên cần kịch bản trộn (xem K2-C). |
| K6 Protocol v2 | Đang chặn vì Codex chưa bàn giao. |
| K7 Render | `hitmegame.onrender.com` đã có DNS và cổng 443, nhưng `/healthz` không phản hồi trong 90 giây. Cần bạn kiểm tra (mục dưới). |
| P4-1 | 7 vật phẩm, giá 199 → ×1,3, cao cấp ×3. Còn danh sách thiếu ảnh. |

## Việc bạn làm tay

1. **Render `/healthz` không phản hồi.** Mở bảng điều khiển Render, vào service, xem trạng thái:
   - Gói miễn phí tự "ngủ" khi không có người dùng; mở `https://hitmegame.onrender.com/healthz` trên trình duyệt, đợi 1–2 phút cho khởi động lạnh rồi tải lại.
   - Xem tab Logs: có báo lỗi build/khởi động không? Có đang lắng nghe đúng cổng (biến `PORT`) không?
   - Chụp dòng log cuối hoặc dán cho mình nếu vẫn lỗi.
2. **Cho Codex chạy tiếp** với prompt S1–S5 trong `hit-me-phase3c-phase4.md` (chưa thấy báo cáo của Codex). Nếu Codex đã chạy, gửi mình báo cáo.
3. **Mở `Evidence/round42/MISSING_ASSETS.md`** và gửi mình danh sách ảnh thiếu; mình sẽ viết bộ prompt tạo ảnh/sprite cho từng món.
4. **Chốt các số nháp** (hoặc trả lời "dùng đề xuất"):

| Mục | Đề xuất (chờ chốt) |
|---|---|
| Điểm rank mỗi trận (ví dụ trận 4 người) | Hạng 1: +25, hạng 2: +10, hạng 3: −5, hạng 4: −15; sàn 0; mỗi cỡ trận nhân hệ số (2 người ×0,6; 6 người ×1,2) |
| Nhiệm vụ ngày | 3 nhiệm vụ/ngày: chơi 3 trận (+50 vàng), trúng 5 đòn (+60), thắng 1 trận (+100); đặt lại 00:00 giờ Việt Nam |
| Đổi tên lần hai trở đi | 99 vàng |

## PROMPT DÁN VÀO ANTIGRAVITY

```
Tiếp tục từ commit d6770c9. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online). Không sửa code server. Không xóa asset/ và SFX/. Mọi "đạt" kèm bằng chứng; fail thì ghi fail.

K2-C. Phòng 6 người trộn
- 6 người: 3 người chơi thật (context Unity) + 3 người script. Chơi 3 trận trọn vẹn. Ghi rõ trong E2E_SUMMARY.md là "trộn", không coi là 6 người thật.

K6. Protocol v2: nếu Codex đã bàn giao MD/PROTOCOL_V2_CHANGES.md (git show prep/codex:MD/PROTOCOL_V2_CHANGES.md > MD/PROTOCOL_V2_CHANGES.md) thì nối chat câu mẫu, rankings + vàng trong match_end, kết bạn; cập nhật E2E. Chưa có thì bỏ qua và ghi chặn.

K7. Render: chỉ chạy lại khi người dùng báo /healthz đã phản hồi. Khi chạy: kịch bản 1, 4, 5, 6, 10 qua https + wss với người chơi thật; ghi độ trễ trung vị vòng 5s; ghi rõ thời gian khởi động lạnh.

P4-2. Cửa hàng + Túi đồ hoàn thiện (cục bộ)
- Đọc Items.json; mua bằng vàng (ví cục bộ qua Mock), kiểm đủ vàng, không mua trùng, trang bị món đã mua, hiển thị món đang dùng trong trận; hiệu ứng trúng đòn khác nhau theo độ hiếm (đẹp hơn khi giá cao hơn, nhưng KHÔNG ảnh hưởng sức mạnh). Giá đọc từ dữ liệu.
- Thiếu ảnh: dùng khung giữ chỗ có chữ "THIẾU ẢNH", không vẽ thay bằng hình sai chủ đề; cập nhật MISSING_ASSETS.md.

P4-3. Hồ sơ, Đăng nhập, Cài đặt
- Màn Đăng nhập: Khách (đã có), Google, Facebook, Apple (nút + luồng giao diện; chưa nối nhà cung cấp thật, bấm thì hiện "Sắp ra mắt" cho các nút chưa nối). Màn Hồ sơ: tên, avatar, rank, vàng, số trận. Đổi tên: lần đầu miễn phí, từ lần hai hiện giá (đọc từ cấu hình) và xác nhận.
- Cài đặt: âm nhạc và hiệu ứng tách riêng, ngôn ngữ VI/EN, ghi nhớ.

P4-4. Rank + Nhiệm vụ (giao diện + logic cục bộ)
- Tab Xếp hạng: danh sách giả cục bộ + hạng của người chơi; bậc rank (Đồng/Bạc/Vàng/Bạch Kim/Kim Cương hoặc tên Việt hóa hợp chủ đề) theo ngưỡng điểm trong tệp cấu hình.
- Tab Nhiệm vụ: 3 nhiệm vụ ngày theo bảng đề xuất, tiến độ cập nhật từ sự kiện trận, nhận thưởng bằng nút, đặt lại theo giờ Việt Nam. Dữ liệu đọc từ tệp cấu hình.

P4-5. Âm thanh
- Nhạc nền menu + trận (vòng lặp mượt, hạ âm khi chuyển cảnh), SFX giao diện (bấm nút, mua đồ, nhận thưởng), tách kênh nhạc/hiệu ứng theo Cài đặt. Dùng tệp có trong SFX/; thiếu thì ghi vào MISSING_ASSETS.md.

QUY TẮC: chạy toàn bộ test 1 lần mỗi nhiệm vụ; build WebGL không quá 1 lần cho mỗi 2 nhiệm vụ; commit riêng; cập nhật MD/STATUS.md; hết quota thì commit phần chạy được và ghi phần dở.

BÁO CÁO CUỐI (tối đa 12 dòng): K2-C, K6, K7, P4-2…P4-5 xong/dở/chặn; số test; ảnh chụp từng màn mới; commit hash.
```

## PROMPT DÁN VÀO CODEX (sau khi xong S1–S5)

```
Tiếp tục nhánh prep/codex. Luật ưu tiên: luật trong repo. Không sửa Core/Unity, không in hay commit khóa.

T1. Rank phía server: điểm theo hạng và cỡ trận (bảng đề xuất trong MD/PHASE4_ECONOMY.md, ghi "chờ chốt" nếu chưa duyệt), bậc rank đọc từ cấu hình, chỉ server cập nhật, idempotent theo matchId, bảng xếp hạng (top N + hạng của người hỏi). Test.
T2. Nhiệm vụ ngày: 3 nhiệm vụ, tiến độ cập nhật từ sự kiện trận trên server, nhận thưởng một lần, đặt lại 00:00 giờ Việt Nam (có test đồng hồ giả qua ranh giới ngày).
T3. Đăng nhập: IAuthProvider cài thật bằng Supabase Auth (Google, Facebook, Apple) khi có khóa/cấu hình, gộp tài khoản khách vào tài khoản đăng nhập mà không mất vàng/vật phẩm; viết MD/AUTH_SETUP.md hướng dẫn người dùng bật nhà cung cấp trong Supabase và đăng ký ứng dụng Google/Facebook/Apple (từng bước, liệt kê URL chuyển hướng cần khai).
T4. Kết bạn thật: gửi/chấp nhận/từ chối/xóa, danh sách bạn, chặn spam yêu cầu; chat chỉ dùng câu mẫu.
T5. Chống gian lận cơ bản: kiểm tra số liệu từ client (vị trí nằm trong sân, hướng hợp lệ, không gửi sau khóa giờ), ghi log vi phạm, không tin tính toán phía client.
QUY TẮC: chạy toàn bộ suite 1 lần mỗi nhiệm vụ; commit riêng; cập nhật MD/STATUS.md; không ghi "xong" khi chưa có test thật.
BÁO CÁO CUỐI (tối đa 10 dòng).
```

## Cổng mở vòng thử nội bộ (Phase 5)

- [ ] Render phản hồi `/healthz` và E2E K7 chạy xanh
- [ ] Protocol v2 khớp hai phía
- [ ] Đăng nhập thật ít nhất Google; vàng/vật phẩm/rank lưu trong Supabase
- [ ] Cửa hàng mua/trang bị qua server, không mua trùng
- [ ] Có đủ ảnh vũ khí hoặc đã quyết định ra mắt với bộ nhỏ hơn

Đạt rồi thì vòng thử: mời 5–10 người quen chơi thật trên điện thoại, thu phản hồi và lỗi trước khi mở rộng.
