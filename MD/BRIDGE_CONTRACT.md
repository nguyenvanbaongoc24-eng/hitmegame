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

`?e2e=1` có `window.__hitmeState` và `window.__hitmeAudioLog`; `?debug=1` hiện mã lỗi hợp đồng. Các nút online khóa bằng `catalog.features.online=false` tới Mốc B. Kiểm thử bridge fixture cục bộ khác với kiểm thử `/next/` đã deploy và khác iPhone thật.
