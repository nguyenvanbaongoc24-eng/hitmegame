# HIT ME — RELEASE NOTES (`0.1.0-beta.2`)

- **Phiên bản:** `0.1.0-beta.2`
- **Mã build (`buildId`):** `759f498-3d96cd7`
- **Commit Unity (`feature/ui-ux-production`):** `759f498` (WebGL Brotli Release binary: `b86353b`)
- **Commit Web UI (`origin/prep/codex`):** `3d96cd7` (read-only `git archive`, 6/6 SHA-256 fingerprints synced)
- **Commit triển khai (`web-dist`):** `2736071` (điểm quay lui trước xuất bản: `f6b9d27`)
- **Kênh đã xuất bản:** `/next/` và `/ui-preview/` (kênh ổn định `/` giữ nguyên ở `0.1.0-beta.1` / `1f1cdfa`)

## 1. URL Trực tuyến (Vercel)
- **Bản ổn định (`/` — giữ nguyên):** https://hitmegame-iota.vercel.app/ (`release.json`: `1f1cdfa`)
- **Bản ứng viên Hybrid (`/next/`):** https://hitmegame-iota.vercel.app/next/ (`release.json`: `759f498-3d96cd7`)
- **Bản xem trước giao diện (`/ui-preview/`):** https://hitmegame-iota.vercel.app/ui-preview/ (`release.json`: `759f498-3d96cd7`)

## 2. Thay đổi chính trong `0.1.0-beta.2`
1. **Đồng bộ giao thức cầu nối (`WebBridge.cs` & `PROTOCOL_MISMATCH.md`):**
   - Ẩn tọa độ `x, y` của đối thủ trong sự kiện `match`, `event`, `emote` suốt pha `placing` (chỉ gửi vị trí của người chơi cục bộ cho tới khi công bố đồng thời ở pha `reveal`).
   - Bổ sung `characterId` và `roundSurvived` trong mảng `result.rankings`.
   - Khử trùng lặp xung `match` 10 Hz trong shell Hybrid để tránh việc `innerHTML` tái tạo DOM giữa chừng khi người chơi chạm nút trên Safari/WebKit.
   - Tắt `AudioSource` của Unity mặc định trên bản Web (`HitMe_AudioUnity = 0`; truyền `?audio=unity` để bật lại).
2. **Đồng bộ dữ liệu & tài sản (`catalog.json` v1):**
   - 4 nhân vật mở: `nam_lang` (Nam Làng), `nu_nang_dong` (Nữ Năng Động), `co_ba` (Cô Ba), `anh_teen` (Anh Teen).
   - 3 nhân vật khóa ("Sắp ra mắt"): `quay_boy` (Quậy Boy), `cho_vang` (Chó Vàng), `meo_mun` (Mèo Mun).
   - 1 vũ khí mở: `dep_to_ong` (Dép Tổ Ong); 2 vũ khí khóa: `chao_chong_dinh`, `vot_pickleball`.
   - Sửa lỗi `ProductionUIController.StartPracticeWithBots` ghi đè nhân vật về `Nam Làng` và lỗi thứ tự khởi chạy `?autostart=practice&char=co_ba&weapon=dep&bots=3`.
3. **Đóng gói đa kênh & Header Vercel (`vercel.json`):**
   - Dùng chung thư mục `/Build/` cho cả `/` (`1f1cdfa`) và `/next/` (`b86353b`) để không nhân đôi tệp `.br` và giữ nguyên `/` 100%.
   - Hiển thị `Phiên bản: 0.1.0-beta.2 (759f498-3d96cd7)` trong bảng Cài đặt Web.

## 3. Kết quả Cổng kiểm thử (`R3` & `R5`)
- **EditMode Tests (Unity):** `191/191 PASS` (0 bài kiểm bị xóa hoặc nới ngưỡng).
- **Release Hygiene (`tools/e2e/test_release_hygiene.py`):** `PASS` (`__hitmeState`, `__hitmeUiDump`, `__hitmeAudioLog`, `__OrigWebSocket` đều `undefined` khi không có `?e2e=1`).
- **Quét bí mật (`scan_secrets.py`):** `0` vi phạm trong gói xuất bản.
- **Ma trận 6 khung nhìn (`deploy_gate.py`):** `6/6 PASS` (`393×695` DPR 3, `393×780` DPR 3, `393×852` DPR 3, `375×553` DPR 2, `360×640` DPR 3, `412×780` DPR 2.625).
- **Kiểm thử hộp đen Codex (`test_next_live.py --matches 5 --gameplay`):** `6/6` hồ sơ PASS, `17/17` ID (`HOME-01`, `HOME-HERO`, `BTN-LABEL`, `BTN-09`, `VIEW-01`, `SEL-LOCK`, `HUD-01`, `F1-MATCHES`, `F2-ROSTER`, `F7-TUTORIAL`, `F7-REOPEN`, `SET-PAUSE`, `SET-RESUME`, `SET-VOLUME`, `EMO-OPEN`, `EMO-SEND`, `ARENA-TOUCH`) PASS.

## 4. Kích thước & Hiệu năng trên URL thật
- **Kích thước gói WebGL Release (Brotli):** `data.br = 7.037 MB`, `wasm.br = 4.165 MB`, `framework.js.br = 0.063 MB`, `loader.js = 0.027 MB` (tổng truyền tải Unity = `11.292 MB` — vượt nhẹ ngân sách 10 MB khoảng `1.29 MB` do một số texture UI Kit mới chưa bật Crunch; cần bật Crunch ở bản kế tiếp để đưa về `~8.5 MB`).
- **Thời gian tải `/next/` (trực tuyến Vercel):** lần đầu `9.20s`, lần hai (có cache) `5.38s`.
- **FPS & Bộ nhớ:** `60 FPS` ở Chromium CPU throttle 4x (`13–14 FPS` trên WebKit headless software rasterizer); bộ nhớ Wasm `~128 MB`.

## 5. Lệnh Quay lui (Rollback) & Nâng cấp lên `/` (Promote)
- **Quay lui `web-dist` về trạng thái trước xuất bản (`f6b9d27`):**
  ```powershell
  cd "D:\HITME GAME\_web_dist_deploy"; git reset --hard f6b9d27; git push --force-with-lease origin web-dist
  ```
- **Nâng cấp `/next/` lên kênh ổn định `/` (chỉ chạy khi người dùng ra lệnh):**
  ```powershell
  cd "D:\HITME GAME\unity-client"; python tools/deploy_web/publish_web_dist.py --promote
  ```
