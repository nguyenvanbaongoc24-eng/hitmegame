# HIT ME – PHASE 4, LƯỢT 4 (CHỈ ANTIGRAVITY; Codex nghỉ đến tối)

Đầu vào: commit `ed456f7`, 177/177 EditMode test. L1–L6 xong. Antigravity cũng đã tìm ra nguyên nhân Render không chạy.

## Ba điều cần biết trước

**1. Nguyên nhân Render (đã tìm ra, bạn làm trên dashboard).** Service Render đang theo nhánh `main`, nơi không có mã server hay Dockerfile; mã server nằm ở nhánh `prep/codex`. Antigravity đã thêm `Dockerfile` và `render.yaml` vào gốc nhánh `prep/codex` (commit `17e5112`) và đẩy lên GitHub. Việc của bạn:
- Render → service `hitmegame` → Settings → Branch đổi thành `prep/codex` → Save → Manual Deploy → Deploy latest commit.
- Chờ build xong rồi mở `https://hitmegame.onrender.com/healthz`.
- Lưu ý: mỗi lần Codex đẩy commit lên `prep/codex`, Render sẽ tự deploy lại. Chấp nhận được lúc này; khi ổn định nên gộp vào `main`.
- Nếu build lỗi, dán 30 dòng log cuối cho mình.

**2. Antigravity đã sửa trên nhánh của Codex.** Lượt trước Antigravity commit thẳng vào `prep/codex` (hai tệp ở gốc). Việc này sai quy ước "mỗi bên một nhánh". Tối nay khi Codex chạy lại, dặn Codex bắt đầu bằng `git pull --rebase origin prep/codex` để tránh xung đột. Từ giờ Antigravity không commit vào nhánh Codex nữa; mọi thứ cần cho Codex đi qua tệp `MD/` (Antigravity ghi, Codex đọc bằng `git show`).

**3. Mã Unity chưa được đẩy lên GitHub.** Lệnh push nhánh `feature/ui-ux-production` bị hủy/dừng nên toàn bộ client hiện **chỉ nằm trên máy bạn**. Đây là rủi ro mất dữ liệu; M1 bên dưới xử lý ngay.

## Cần nhớ khi đọc con số hiệu năng

- FPS 59,9 ở CPU 4x/6x đo trong Chromium chạy nền, có thể dùng GPU phần mềm (SwiftShader). Giả lập CPU không giả lập GPU điện thoại, nên con số này chỉ cho thấy phần logic ổn, chưa đủ chứng minh máy yếu chạy mượt.
- HOME_SHOWN 4G chuẩn 4,18 giây là tốt, nhưng 4G chậm (4 Mbps) tới **23,8 giây**. Tải ~10 MB là điểm yếu nếu người chơi ở vùng sóng yếu. Lượt này giảm tiếp.

## Việc bạn làm tay

1. Đổi nhánh Render (mục 1). Xong thì báo mình hoặc dán cho Antigravity để chạy K7.
2. Đăng nhập GitHub trên máy nếu push bị đòi mật khẩu (dùng GitHub Desktop hoặc `gh auth login`). Nếu Antigravity báo chưa push được vì xác thực, bạn là người bấm.
3. Đổi mật khẩu database Supabase (nếu chưa).
4. Gửi mình `Evidence/round42/MISSING_ASSETS.md` (nếu muốn mình viết bộ prompt tạo ảnh).

## PROMPT DÁN VÀO ANTIGRAVITY

```
Tiếp tục từ commit ed456f7. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online). KHÔNG commit vào nhánh prep/codex hay sửa code server nữa; mọi thứ Codex cần ghi vào MD/ bên nhánh Unity. Không xóa asset/ và SFX/. Mọi "đạt" kèm bằng chứng; fail thì ghi fail.

M1. Sao lưu và đẩy mã lên GitHub (làm trước)
- Chẩn đoán vì sao push feature/ui-ux-production bị treo (xác thực, tệp lớn, thiếu LFS, mạng). Kiểm tra .gitignore không để Builds/Library/Temp lọt vào; liệt kê tệp lớn nhất trong lịch sử. Sửa được thì push; không sửa được thì dừng và ghi chính xác lệnh/bước cần người dùng làm vào MD/GIT_PUSH_HELP.md.
- Tạo bản sao lưu cục bộ: git bundle của nhánh vào D:\HITME GAME\_backup\ (tên có ngày). Báo kích thước.

M2. Hồi quy toàn bộ E2E
- Từ L1 đến L6 đã sửa nhiều chỗ nhưng mới chạy lại có 1 kịch bản. Chạy lại toàn bộ 10 kịch bản + K2-A + K2-C + B4 (10 trận) trên bản Release mới. Ghi cột người thật/script như trước; fail thì ghi fail và nguyên nhân.
- Thêm kịch bản mới: lần đầu mở game xóa cờ HitMe_TutorialCompleted → hướng dẫn xuất hiện → hoàn tất và bỏ qua; mở lại không hiện nữa.

M3. Giảm dung lượng tải cho mạng yếu
- Mục tiêu: tổng truyền tải ≤ 6 MB (hiện ~10,1 MB) và HOME_SHOWN trên 4 Mbps ≤ 12 giây, không làm hình lệch mockup. Cách làm gợi ý theo thứ tự hiệu quả: (1) liệt kê tệp nặng nhất trong build (build report), (2) nén âm thanh (Vorbis/ADPCM, mono cho SFX, giảm bitrate nhạc), (3) giảm kích thước/nén texture còn lớn, gộp atlas, (4) strip code/kiểm tra Managed Stripping, (5) bỏ asset không dùng, (6) tải trễ nhạc nền trận và nhân vật chưa chọn sau khi Home hiện.
- Màn tải: thanh tiến độ có phần trăm và MB đã tải, chữ Việt/Anh, thông báo "mạng chậm" nếu quá 15 giây.
- Bảng trước/sau vào Evidence/round43/M3_size.md; hoàn tác bước nào làm hỏng hình hay test.

M4. Ghi rõ giới hạn của số đo hiệu năng
- Bổ sung vào Evidence/round43/L2_perf.md: trình duyệt dùng GPU phần mềm hay thật? (kiểm tra chrome://gpu hoặc WebGL renderer qua JS). Nếu có thể, chạy lại với chế độ dùng GPU thật của máy (flag --use-angle, headless=false) và so sánh. Kết luận rõ phần nào KHÔNG thể suy ra cho điện thoại thật.

M5. Bộ đặc tả ảnh còn thiếu
- Từ MISSING_ASSETS.md tạo MD/ASSET_REQUEST_SHEET.md: mỗi món một dòng gồm tên, id, kích thước chuẩn (khung 256x256 hoặc kích thước đang dùng), số khung/pose cần có, nền trong suốt, hướng nhìn, viền/đổ bóng đồng bộ với bộ có sẵn, tên tệp đặt đúng chuẩn dự án, vị trí thư mục. Để người dùng đưa nguyên cho công cụ tạo ảnh. Không tự vẽ thay.

M6. Chọn sân (map) theo dữ liệu
- Kiểm tra asset/ có đủ hai sân từ mockup (sân đấu phố Việt Nam, sân đình làng với ao sen) không. Nếu đủ: Maps.json (id, tên VI/EN, ảnh, kích thước sân hợp lệ, nhạc), chọn sân ở Home/Lobby (chủ phòng chọn, người khác thấy), hiệu ứng đổi sân mượt; luật chơi và kích thước vùng chơi KHÔNG đổi theo sân. Chưa nối server (ghi message cần thêm vào PROTOCOL_MISMATCH.md để Codex làm sau). Nếu thiếu ảnh: ghi vào MISSING_ASSETS.md thay vì làm tạm.

M7. Báo lỗi trong game (để người thử không cần giải thích)
- Nút "Báo lỗi" trong Cài đặt: thu thập phiên bản/commit build, ngôn ngữ, trình duyệt/hệ điều hành, màn hình hiện tại, mã phòng, 50 dòng nhật ký gần nhất đã lọc (không chứa token/mật khẩu), rồi sao chép vào clipboard để người dùng dán. Hiển thị phiên bản build ở cuối Cài đặt.
- Ghi nhật ký có cấp độ (info/warn/error) vào bộ đệm vòng; test kiểm không lọt token.

M8. Kiểm tra Render sau khi người dùng đổi nhánh (chỉ khi người dùng báo đã đổi hoặc /healthz trả 200)
- Chạy K7 trên https + wss: kịch bản 1, 4, 5, 6, 10 với người chơi thật. Ghi thời gian khởi động lạnh, độ trễ trung vị vòng 5s, tỷ lệ pass. Nếu /healthz vẫn lỗi thì ghi chẩn đoán (mã HTTP, thời gian) và KHÔNG sửa server.

QUY TẮC: chạy toàn bộ test 1 lần mỗi nhiệm vụ; build WebGL không quá 1 lần cho mỗi 2 nhiệm vụ (M2 và M3 cần đo thì được); commit riêng; cập nhật MD/STATUS.md; hết quota thì commit phần chạy được và ghi phần dở. Thứ tự nếu quota ít: M1 → M2 → M3 → M7 → M8 → M6 → M5 → M4.

BÁO CÁO CUỐI (tối đa 12 dòng): M1–M8 xong/dở/chặn; đã push được chưa; E2E hồi quy x/y kịch bản pass; dung lượng trước/sau và HOME_SHOWN 4 Mbps; GPU phần mềm hay thật; số test; commit hash.
```

## Tối nay khi Codex chạy lại

Codex bắt đầu bằng `git pull --rebase origin prep/codex` (có commit của Antigravity ở gốc), rồi làm `hit-me-codex-luot-5.md` theo thứ tự R1 → R4 → R5 → R6 → R3 → R2 → R7. R1 sẽ xác nhận Dockerfile gốc và `render.yaml` Antigravity thêm có đúng cổng, đúng lệnh khởi động không.

## Một việc nên làm sớm (không gấp hôm nay)

Trước khi mở cho người ngoài, kiểm tra quy định hiện hành ở Việt Nam về game trực tuyến (cấp phép, xác thực tài khoản người chơi, bảo vệ dữ liệu cá nhân, giới hạn độ tuổi). Mình không khẳng định chi tiết quy định ở đây; nên tham khảo ý kiến người am hiểu pháp lý hoặc cơ quan quản lý trước khi phát hành rộng rãi, còn thử nội bộ với người quen thì ít rủi ro hơn.
