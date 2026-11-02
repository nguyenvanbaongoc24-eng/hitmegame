# HIT ME – LỆNH CHO CODEX, ĐỢT 29: MỘT LỆNH → LINK LOCALHOST (VÀ LINK LAN) ĐỂ CHỦ DỰ ÁN TEST

> Dán cho Codex, kèm `hit-me-codex-round28-while-ag-offline.md`, `hit-me-codex-round27.md`, `hit-me-room-size-rule-v1.3.md`, `hit-me-gameplay-rules-v1.2.md`. Mục tiêu: chủ dự án chạy **một lệnh** trên máy Windows rồi mở **một địa chỉ** để (1) chơi bản WebGL hiện có của AG (Luyện với bot) và (2) **chơi thử online bằng server thật của Codex** với 2–6 tab/thiết bị. Quy tắc cũ giữ nguyên: không mở/sửa Unity, không sửa `unity-client/Assets` và `Builds/`, không đổi luật, không commit lên `main`, không đưa khóa vào mã, không triển khai thật. "Pass" chỉ khi dán được `Passed! N/N` + tên test.

## Sự thật cần nói thẳng (ghi vào README của công cụ)
Bản Unity của AG **chưa có mã mạng** (nối dây dự kiến sau 2/11). Vì vậy "gộp AG + Codex" ở giai đoạn này nghĩa là: một trang trung tâm có **hai đường**: (A) bản WebGL của AG chạy chế độ Luyện với bot; (B) **"Phòng thử online" (Online Lab)** — một trang web thử nghiệm đơn giản do Codex viết, dùng **đúng giao thức và server thật**, cho phép 2–6 người chơi thật thử luật online. Lab **không phải giao diện cuối**; ghi nhãn "BẢN THỬ NGHIỆM SERVER" trên trang. Không để chủ dự án hiểu nhầm đây là game đã có online.

## Việc làm

### L1. Trình khởi chạy một lệnh: `tools/dev/run_local.ps1` (Windows PowerShell 5.1 và pwsh)
1. Khởi chạy **server** (bản Release, cổng cấu hình được, mặc định 8080) với `ALLOWED_ORIGINS` tự thêm `http://localhost:<cổng>` và `http://<IPv4 LAN>:<cổng>`; chờ `/healthz` trả OK rồi mới tiếp.
2. Phục vụ **bản WebGL mới nhất của AG** (đọc **chỉ đọc** từ `D:\HITME GAME\unity-client\Builds\WebGL_*`, chọn thư mục mới nhất theo tên/giờ, hoặc tham số `-Build <đường dẫn>`) bằng máy chủ tĩnh của Codex: header `Cache-Control: no-store` cho `index.html`/`*.loader.js`, MIME `.wasm`, xử lý `.br/.gz` nếu có, ghi dòng `build <commit>` đọc từ `index.html`/`BuildFingerprint`. Không chép/sửa gì trong `Builds/`.
3. Phục vụ **trang trung tâm** (hub) tại `http://localhost:8100/` (cổng tham số): 4 nút lớn — **[A] Chơi với bot (WebGL của AG)** (mở `…/game/?practiceBots=1&v=<commit>`), **[B] Phòng thử online (Online Lab)**, **[C] Trạng thái server** (`/healthz`, số phòng đang mở, số kết nối), **[D] Hướng dẫn test trên điện thoại**. Hub in rõ: `build đang phục vụ`, `commit server`, `giao thức vX`.
4. In ra màn hình bảng địa chỉ: **localhost** (máy tính) và **LAN** (`ipconfig` → IPv4 đang dùng; nếu có nhiều card, liệt kê từng cái và nói điện thoại phải cùng mạng). Có tùy chọn `-Open` tự mở trình duyệt. Dừng sạch bằng Ctrl+C (tắt cả tiến trình con).
5. Không tự sửa tường lửa. Nếu cổng bị chặn từ LAN: in đúng **một** lệnh `netsh advfirewall …` để chủ dự án tự chạy (cần quyền quản trị) và cách xóa quy tắc đó.

### L2. Online Lab (`tools/dev/lab/`, HTML+JS thuần, không cần build)
Mục đích: thử **luật và luồng online** với server thật bằng trình duyệt (kể cả iPhone Safari qua LAN).
- Màn **Vào**: tên người chơi (guest token do server cấp theo H2), nút **Tạo phòng**, **Vào phòng (mã)**, **Ghép nhanh**.
- Màn **Phòng**: danh sách người `n/6`, ô trống "Đang chờ…", nút **Sẵn sàng**, host có nút **Bắt đầu**; khi chỉ 1 người: nút khóa + câu "Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot." + liên kết sang [A]; ghép nhanh quá 30 giây: gợi ý (không thêm bot).
- Màn **Trận** (sân vẽ bằng canvas, hình đơn giản, có chú thích "bản thử"): bắt đầu đếm ngược 3 giây → pha **Đặt** 5 giây đúng **cử chỉ v1.2** (chạm đặt nhân vật; giữ rồi trượt để ngắm, thước theo ngón; chạm chỗ khác = làm lại hoàn toàn, hướng reset về lên; kéo về dưới 4 mm = hủy ngắm; không ngắm = ném thẳng; chỉ ngón đầu; chạm HUD không đặt) → `lock_ready`/`commit` (`hasAim`, `aimX`, `aimY`) → **Lộ diện** → **Ném** → **Kết quả vòng** (thẻ HP công khai suốt) → **Kết thúc trận** (bảng xếp hạng N dòng) → Đấu lại/Về phòng. Nút `emote` (danh sách cho phép; chỉ hiện trên thẻ khi đặt, trên nhân vật sau lộ diện).
- Màn **Nhật ký** (bật/tắt): các thông điệp gửi/nhận (đã che token), độ trễ khứ hồi, mã lỗi + câu tiếng Việt từ `server/README.md`; nút **Ngắt kết nối** để thử vào lại bằng `reconnectToken` (30 giây).
- Tham số URL: `?name=`, `?room=`, `?server=` (mặc định cùng máy chủ với trang) để mở nhiều tab nhanh; `?bot=1` **không có** (phòng online không bot).
- Quét bí mật: trong pha Đặt, giao diện **không bao giờ** hiện vị trí/hướng người khác (kiểm bằng test).

### L3. Kiểm thử tự động `tools/dev/smoke_local.mjs` (Playwright/Chromium, không cần Unity)
1. Chạy `run_local.ps1` (hoặc gọi hàm khởi chạy trực tiếp), chờ hub.
2. Kiểm **[A]**: `GET /game/index.html` = 200, chứa `build <commit>` đúng với thư mục build; `.wasm` có MIME đúng; tệp `.data` tải được. (Không khởi chạy Unity.)
3. Kiểm **[B]** với **N = 2, 3, 6**: mở N trang Lab = N người chơi, vào cùng phòng, chơi **trọn một trận** (đủ 5 vòng hoặc còn ≤ 1 người sống); khẳng định: bắt đầu đúng, thẻ HP công khai, không lộ vị trí/hướng người khác trước lộ diện, thứ hạng cuối khớp `ServerMatch`/`Core`, `emote` đúng luật; ca **N=1** không bắt đầu được (`not_enough_players`); ngắt rồi vào lại bằng `reconnectToken`; ghép nhanh hai người; sau `quickMatchHintSeconds` có `quick_match_hint` (thu ngắn thời gian trong test).
4. Dán `Passed! N/N` + tên test + độ trễ trung bình/lớn nhất; lưu log + ảnh chụp từng màn Lab vào `Evidence/dev_lab/` kèm `.json` nguồn gốc.

### L4. Hướng dẫn chủ dự án (mục "Test cục bộ" trong `server/README.md`, ≤ 25 dòng)
Một lệnh chạy, địa chỉ hub, cách mở 2 tab (hoặc điện thoại + máy) để thử phòng, điều gì **đã** thử được (luật online, phòng, ghép nhanh, vào lại, xếp hạng) và điều **chưa** (giao diện cuối, Unity nối mạng), cách dừng, cách xử lý lỗi thường gặp (cổng bị chiếm, tường lửa, khác Wi-Fi, IP đổi). Đặt **danh sách 8 điều cần quan sát khi thử online** và mẫu báo cáo để chủ dự án điền (thiết bị, trình duyệt, phòng mấy người, lỗi, cảm nhận).

## Tiêu chí hoàn thành
- Một lệnh → hub chạy; chủ dự án mở `http://localhost:8100/` thấy 4 nút; [A] chạy được bản WebGL của AG, [B] chơi trọn trận với ≥ 2 tab.
- `smoke_local.mjs` `Passed! N/N`.
- Địa chỉ LAN in đúng IP hiện tại; điện thoại cùng mạng mở được (ghi rõ nếu chưa tự thử được).
- Không sửa `Builds/`, `unity-client/Assets`, không bí mật trong mã; kết quả đẩy `prep/codex @ <hash>`.

## Báo cáo
```
VIỆC: L1 | L2 | L3 | L4
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" + tên test; ảnh Lab; bảng độ trễ)
CHƯA LÀM / GIỚI HẠN: ...  (nêu rõ: Unity chưa nối mạng)
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
LỆNH CHẠY: powershell -ExecutionPolicy Bypass -File tools\dev\run_local.ps1 -Open
ĐỊA CHỈ: http://localhost:8100/   |   LAN: http://<IPv4>:8100/
```
