# HIT ME – cầu nối Web UI / Unity, bản kiểm 05-11-2026

Nguồn đối chiếu: `unity-client/Assets/Scripts/UI/WebBridge.cs` trên nhánh `feature/ui-ux-production`. Web nhận object hoặc chuỗi JSON qua `window.hitmeUi.onUnity`; Web gọi `unityInstance.SendMessage('WebBridge', method, JSON.stringify(payload))`. Bản `/next/` chỉ lấy trận từ Unity; `/ui-preview/?mock=1` là mô phỏng.

## Unity → Web đang có

| Loại | Trường dùng trên Web | Ghi chú |
|---|---|---|
| `ready` | `build,version,roster[],weapons[]` | Mở thao tác Luyện bot; Web gửi `SetLayout` ngay sau khi nhận. |
| `screen` | `name: home|match|result|lobby` | `result` tăng số trận trong phiên. |
| `match` | `round,maxRounds,phase,timeLeft,weaponName,players[]` | `players[]`: `id,name,characterId,slot,hp,maxHp,alive,isLocal`. HUD không đọc `x,y`. |
| `layout` | `playableRect:{x,y,w,h}` | Đơn vị CSS pixel trong khung Unity 393×695; Web dùng để dành vùng chơi. `match.playableRect` lại có `left,top,right,bottom` chuẩn hóa, không được trộn hai dạng. |
| `event` | `kind,sourceId,targetId` | Toast và âm; thiếu tọa độ hiệu ứng. |
| `result` | `round,rankings:[{id,name,rank,hits,survived}],hits` | Chưa có `characterId` hoặc `roundSurvived`; Web hiển thị `—` cho số vòng sống. |
| `error` | `code,message` | Hiện toast. |
| `lobby` | `code,maxPlayers,players:[{id,name,characterId,ready,isHost}]` | Sảnh mẫu hiển thị 1–6 người. `id=local` là người dùng trên web. Bản thật cần Unity gửi lại sau mỗi thay đổi thành viên/ready. |
| `emote` | `kind,id,playerId,screen:{x,y}` | Web chỉ hiện trên HUD/toast, không dùng `screen` trước công bố. |
| `sfx` / `music` | `id,vol,pan` / `id,action` | `audio.js` phát tệp nếu có, fallback tổng hợp. |

## Web → Unity đang có

| Phương thức | Payload / hành vi |
|---|---|
| `StartPractice` | `{characterId,weaponId,botCount}`; 4 nhân vật mở, Dép Tổ Ong mở. |
| `SetCharacter`, `SetWeapon` | `{id}`; Web không cho chọn mục khóa. |
| `SetLayout` | `{top,bottom,safeTop,safeBottom}` CSS pixel, gửi lại khi visual viewport đổi. |
| `SendEmote` | `{kind,id}`; trong pha đặt chỉ hiện trên thẻ/toast, không hiện trên sân. |
| `Pause`, `Resume`, `ExitMatch`, `Rematch` | `{}`; Pause/Resume chỉ dùng cho Luyện bot; thoát cần xác nhận hai lần. |
| `SetSettings` | `{musicVolume,sfxVolume,reducedMotion}`; Web áp âm tức thời, Unity hiện mới lưu `reducedMotion`. |
| `QuickPlay` | `{characterId,weaponId}`; chỉ dùng trong `/ui-preview/?mock=1`, `/next/` còn khóa online. |
| `CreateRoom` | `{characterId,weaponId}`; Unity cần trả `lobby` hoặc `error`. |
| `JoinRoom` | `{code,characterId,weaponId}`; Unity cần trả `lobby` hoặc `error` (ví dụ `room_not_found`). |
| `LeaveRoom` | `{}`; gửi khi quay từ sảnh về Home. |
| `Ready` | `{ready:boolean}` trong sảnh; Unity gửi lại `lobby` với trạng thái mới. Nút `Ready` trong trận luyện bot vẫn là `{}` để khóa hướng sớm. |

`?e2e=1` có `window.__hitmeState` và `window.__hitmeAudioLog`; `?debug=1` hiện mã lỗi hợp đồng. Các nút online khóa bằng `catalog.features.online=false` tới Mốc B. Kiểm thử bridge fixture cục bộ khác với kiểm thử `/next/` đã deploy và khác iPhone thật.

Sảnh `/ui-preview/?mock=1` chỉ là Unity giả: nút tăng/giảm người chỉ đổi dữ liệu mẫu, không gửi phương thức mới cho Unity và không bắt đầu trận online. Chấm sức khỏe gọi `GET https://hitmegame.onrender.com/healthz` khi trang hiện, tối đa 8 giây; `200 {"status":"ok"}` là sẵn sàng, `503` là đang thức, lỗi mạng là không kết nối. Hiện Render chưa trả CORS cho nguồn Vercel: trình duyệt nhận opaque response (không đọc được mã HTTP), nên trạng thái giữ ở “Đang đánh thức máy chủ” thay vì báo sẵn sàng sai. Cần cấu hình CORS hoặc proxy cùng nguồn trước khi dùng chấm này làm điều kiện ghép trận.
