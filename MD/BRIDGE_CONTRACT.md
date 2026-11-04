# HIT ME – hợp đồng cầu nối Hybrid UI v1

Áp dụng cho lớp Web `web-ui/bridge.js` và `web-ui/audio.js`; tài liệu này chỉ là hợp đồng bàn giao. Unity chưa được Codex sửa theo hợp đồng mới. Mọi tin Unity → Web đi qua `window.hitmeUi.onUnity(event)` (object hoặc chuỗi JSON). Không gửi dữ liệu đối thủ trước pha công bố.

## Unity → Web

| Sự kiện | Ví dụ | Quy tắc |
|---|---|---|
| `sfx` | `{ "type":"sfx", "id":"place_pop", "vol":0.8, "pan":0 }` | `id` phải nằm trong bảng AUD của `hit-me-requirements-ui-audio-v1.md`; `vol` 0–1; `pan` −1…1. Âm trên Web có fallback tổng hợp. |
| `music` | `{ "type":"music", "id":"bgm_match", "action":"play" }` | `action` là `play` hoặc `stop`; menu/trận crossfade 600 ms. |
| `emote` | `{ "type":"emote", "kind":"sticker", "id":"sticker_01", "playerId":"p1", "screen":{"x":120,"y":340} }` | Trong pha đặt, không gửi `screen` của đối thủ và không đặt bong bóng trên sân. Chỉ sau công bố mới dùng `screen`. |

## Web → Unity

| Phương thức `WebBridge` | Payload | Hiện trạng |
|---|---|---|
| `SendEmote` | `{ "kind":"sticker|phrase", "id":"sticker_01" }` | Có trong allowlist của Web; màn chọn biểu cảm chưa làm. Server phải kiểm id và giới hạn tần suất. |
| `SetSettings` | `{ "music":0.6, "sfx":0.8, "language":"vi", "reducedMotion":false, "hideEmotes":false, "batterySaver":false }` | Có trong allowlist; màn cài đặt đầy đủ chưa làm. |
| `Pause` / `Resume` | `{}` | Chỉ cho Luyện với bot. Cả hai đã có trong allowlist; giao diện và Unity chưa nối. Trận online tiếp tục chạy khi mở Cài đặt. |

`?e2e=1` bật `window.__hitmeAudioLog = [{t,id,source}]`; bản thường không tạo nhật ký này. `source` là `file` hoặc `synth`. Bảng âm tổng hợp còn thiếu tệp trong `web-ui/AUDIO_TODO.md`. WebKit headless trên máy kiểm thử không có `AudioContext`; âm trên Safari iPhone thật phải được chủ dự án nghe và xác nhận.

## Phần cần làm ở lượt phối hợp Unity/server

Unity cần phát `sfx`/`music` đúng thời điểm từng pha, tắt AudioSource trùng lặp ở bản Web (giữ cờ quay lui), chuyển `emote` theo quy tắc bí mật, và chỉ `Pause` trong Luyện với bot. Server phải từ chối `emote` id lạ, giới hạn ≤ 1/giây và ≤ 3/pha, chỉ phát id/phase/playerId trong pha đặt; không phát tọa độ/hướng trước khóa. Đây là hạng mục chưa được chứng minh bằng bản Web hiện tại.
