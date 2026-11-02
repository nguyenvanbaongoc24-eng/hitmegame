# HIT ME – VIỆC TIẾP THEO CHO CODEX (xếp theo ưu tiên, làm lần lượt)

> Dán toàn bộ file này cho Codex. Ghi nhận từ báo cáo trước của bạn: .NET SDK `8.0.427` đã có; `Core/Phase1` link được vào .NET 8; server mới ở trạng thái WIP (`prep/codex @ 7e3ce04`); `REPLACE_LIST.csv` có 325 dòng (`@ 62ea52a`). **Commit nhỏ, thường xuyên** (`WIP:` nếu dang dở). Nếu còn ít token: làm **P1 → P3**, commit, ghi dòng "Đang dở: ..." vào `server/README.md` rồi dừng.

---

## 0. Quy tắc (giữ nguyên)

1. **Không mở Unity.** Không sửa `unity-client/Assets`, `ProjectSettings`, `Packages`, `MD/`. Làm trên `prep/codex` (worktree riêng). Không commit lên `main`.
2. `ASSET`, `SFX`, `SpriteSources`: chỉ đọc.
3. Không đổi luật. Không sửa Core. Luật: đặt bất kỳ đâu + giữ rồi lướt kéo thước ngắm trong 5 giây, hết giờ lộ diện và tự ném, vũ khí dừng ở người đầu tiên trúng, 3 HP, sân 9,0 × 11,4, `maxRounds = 5`, hòa HP → số lần trúng, `NoAimPolicy = ThrowFacing`, ≤ 6 người, 7 nhân vật, không thú cưng.
4. Không tạo .md mới ngoài `server/README.md`. Báo cáo kiểm tra trả lời trong khung chat.
5. Mỗi việc kết thúc bằng bằng chứng (lệnh đã chạy + kết quả).

---

## P1 – Bản đồ "hai lõi luật" (chỉ đọc, ưu tiên cao nhất, làm trước)

**Vấn đề bạn vừa phát hiện:** 5 file trong `Assets/Scripts/Core/` còn `using UnityEngine` (`BotBrain.cs`, `CharacterData.cs`, `GameModels.cs`, `MatchStateMachine.cs`, `WeaponData.cs`), trong khi `Core/Phase1` là C# thuần đã được kiểm thử. Nếu client chạy luật bằng một lớp còn server chạy bằng lớp khác thì hai bên sẽ **lệch luật** (đúng điều Phase 4 phải tránh).

Trả lời bằng **một bảng** trong khung chat, mỗi lớp/tệp trong `Scripts/Core/**` và `Scripts/Game/**`:

| Lớp / tệp | Có `UnityEngine`? | Là luật (tính trúng, HP, kết thúc, bot) hay dữ liệu/hiển thị? | Trùng chức năng với lớp nào ở `Core/Phase1`? | Ai đang dùng (Game / Editor / Tests / không ai) |

Sau bảng trả lời 4 câu:
1. `MatchController` (Game) hiện chạy luật bằng lớp nào: `Core/Phase1` hay `MatchStateMachine.cs`/`BotBrain.cs` mới?
2. `MatchStateMachine.cs` và `BotBrain.cs` có trùng với `MatchState`/`BotBrain` của Phase1 không? Có test nào phủ chúng không?
3. `CharacterData.cs`, `WeaponData.cs` có phải `ScriptableObject` (hợp lệ nhưng **không nên nằm trong Core**)?
4. Đề xuất tối thiểu (chỉ viết, không làm): lớp nào chuyển sang `Game/Data`, lớp nào xóa vì trùng, lớp nào giữ. Mục tiêu: **một** lõi luật thuần C#, dùng chung cho client và server.

## P2 – Vệ sinh repo (chỉ đọc)

Diff `v0.1-graybox..origin/main` đổi **462 file, +129.616 −4.456**. Cần biết số lớn này là gì:
1. Chia theo phần mở rộng/thư mục: số file và số dòng thêm (`.cs`, `.meta`, `.prefab`, `.unity`, `.png`, `.wav/.mp3/.ogg`, `.js/.wasm/.data`, `.md`, `.csv`…).
2. **20 file lớn nhất** đang nằm trong repo (kích thước). Có file > 50 MB không (GitHub chặn > 100 MB)?
3. Bản build WebGL (`Evidence/phase2/WebGLBuild/`) hoặc `Library/`, `Temp/`, `Logs/`, `Builds/` có bị commit không? Kiểm tra `.gitignore` hiện có.
4. Ảnh/âm thanh nhị phân có dùng Git LFS chưa? Kích thước thư mục `.git`.
5. Vị trí `MD/`: có nằm **ngoài** repo không (báo cáo của bạn nói bản `MD/PHASE2_REPORT.md` khác hash với hai bản trong repo)? Nếu có, tài liệu không được quản lý phiên bản.
6. Ghi đề xuất vào `tools/repo_hygiene/proposed.gitignore` và `proposed.gitattributes` (Git LFS cho `*.png *.jpg *.psd *.wav *.mp3 *.ogg`, bỏ qua thư mục build/`Library`). **Chỉ viết đề xuất, không áp dụng, không viết lại lịch sử.**

## P3 – Server Phase 4: Room Authority + bí mật vị trí (việc chính)

Tiếp tục từ skeleton WIP. Dùng **chỉ `Core/Phase1`** (liên kết file, không sao chép, không sửa). Cập nhật theo commit mới nhất của AG khi có `v0.2-rc1`; ghi rõ commit đang dùng trong `server/README.md`.

**Chức năng**
- Phòng trong bộ nhớ: tạo (mã `HM`+4 số, kiểm tra trùng), vào, rời, 2–6 người, chủ phòng bắt đầu.
- Giao thức JSON có `v` và `type`: `join`, `ready`, `commit`, `leave` / `room_state`, `round_start`, `reveal`, `round_result`, `match_end`, `error`. `room_state` gửi kèm cấu hình trận (kích thước sân, `playerRadius`, `placeSeconds`, `maxRounds`) lấy từ `GameConfig` để client không tự đặt số riêng.
- Vòng chơi: server giữ kín các `commit`; khóa khi hết `placeSeconds` hoặc tất cả `ready`; tính bằng `RoundResolver` của Core; phát `reveal` + `round_result`; hết `maxRounds` hoặc còn ≤ 1 người thì `match_end`.
- Thời gian theo đồng hồ server, đưa vào qua `System.TimeProvider` để test được bằng đồng hồ giả. Client chỉ nhận `deadline`.
- Vào lại sau mất kết nối: cấp `reconnectToken`, giữ chỗ 30 giây. AFK: chỉ để hook `TODO` rỗng.

**Xác thực đầu vào (server không tin client)**
- `commit`: vị trí `NaN/∞` hoặc ngoài sân → kẹp vào sân; hướng không phải số hữu hạn/độ dài 0 → coi như không ngắm (`ThrowFacing`); mỗi người một `commit` hợp lệ cho mỗi `roundId` (lần sau đè lần trước cho tới khi khóa); `commit` sau khóa → bỏ qua.
- Giới hạn kích thước thông điệp (≤ 1 KB), giới hạn tần suất (ví dụ ≤ 20 thông điệp/giây/kết nối), `type` lạ → `error`, không ném ngoại lệ ra ngoài; một client chậm/đứt không được làm treo các client khác.
- Có `GET /healthz`, đọc cổng từ biến môi trường `PORT`. `Dockerfile` và `render.yaml` bản nháp, **không triển khai**.

**Kiểm thử (`server.Tests`, xUnit), tất cả phải pass**
1. **Bí mật:** trước khóa, không thông điệp nào gửi cho A chứa vị trí/hướng của B (quét toàn bộ nội dung đã gửi). Kiểm cả thông điệp `room_state`/`error`.
2. **Tương đương Core:** cùng `seed` + cùng commit → kết quả từng vòng giống hệt Core chạy trực tiếp.
3. **Thời gian:** hết giờ chưa commit → `ThrowFacing`; commit trễ → bỏ qua (dùng đồng hồ giả).
4. **Vào lại:** mất kết nối 10 giây rồi vào lại đúng chỗ; quá 30 giây → bị loại khỏi phòng.
5. **Đầu vào xấu:** `NaN`, vị trí ngoài sân, JSON hỏng, thông điệp quá lớn, `type` lạ, hai `commit` cùng vòng.
6. **Mô phỏng:** 20 trận × 6 bot trong tiến trình: 0 ngoại lệ, 0 treo, mỗi trận đúng luật.
7. **Cô lập lỗi:** một client không đọc thông điệp (chậm) không chặn vòng của 5 client còn lại.

Tài liệu duy nhất: `server/README.md` (≤ 1,5 trang): cách chạy, cách test, danh sách thông điệp + ví dụ JSON, quyết định kỹ thuật, điều chưa làm. 9 cảnh báo nullable đến từ Core: **không sửa Core**; ghi lại và dùng `<NoWarn>` có chú thích cho phần link.

## P4 – Bộ "vector vàng" để client và server cùng kiểm (không cần Unity)

Từ `Core/Phase1`, sinh 200 vòng mẫu (nhiều vị trí/hướng khác nhau, cả trường hợp biên: đứng sát nhau, hai người cùng một đường ném, người bị loại, ném trúng đồng thời) thành `server/Tests/golden/rounds.json` (đầu vào → HP/đội hình/kết quả mong đợi, kèm `seed` và phiên bản `GameConfig`). Test server dùng nó. Về sau test EditMode của Unity sẽ đọc cùng file để bảo đảm **client và server không lệch luật**.

## P5 – Danh sách thay asset: xếp lại theo ưu tiên + ảnh đối chiếu

`REPLACE_LIST.csv` đang xếp `sprite_001`–`sprite_025` (chảo) lên đầu. Chảo **không** thuộc MVP (vũ khí MVP là dép). Làm lại:
1. Thêm cột `uu_tien`: **P0** = asset dùng ngay trong trận MVP (Nam Làng, Anh Nón Lá, Chó Vàng, Mèo Mun, dép, bóng/vòng dưới chân, hiệu ứng trúng đòn); **P1** = 3 nhân vật còn lại + UI/Home/Vào phòng/Biểu cảm; **P2** = vũ khí khác (chảo, pickleball…) và phần còn lại. Sắp theo `uu_tien` rồi mức độ lỗi.
2. Với **P0**, xuất `Evidence/asset_qa/contact/P0_green.png` và `P0_magenta.png`: ảnh ghép bản gốc và bản `cleaned` cạnh nhau, cận cảnh chân/viền, để chủ dự án nhìn xem bản đã làm sạch có dùng được không. Ghi rõ file nào làm sạch **chưa đạt** (còn ô vuông, mất chi tiết, viền răng cưa).
3. Cột `ban_da_lam_sach` chỉ điền khi bản đó đạt; còn lại để trống kèm lý do.

## P6 – (tùy chọn, nếu còn token) bản vá tham khảo cho AG

`ProjectileView.cs` dùng `new Material(Shader.Find(...))` lúc chạy: trên WebGL, shader có thể bị loại khỏi bản build (đúng lỗi Safari từng gặp) và tạo rác bộ nhớ mỗi lần. **Không sửa tệp.** Đọc tệp ở commit mới nhất của AG và viết bản vá tham khảo `tools/patches/projectileview_material.patch` (dùng vật liệu gán sẵn trong prefab hoặc cache một `Material` dùng chung) để AG áp dụng bằng `git apply --3way`. Nếu AG đã sửa tệp thì bỏ qua.

---

## Báo cáo cuối mỗi việc
```
VIỆC: P1 | P2 | P3 | P4 | P5 | P6
ĐÃ LÀM: ...
FILE: ...
BẰNG CHỨNG: (lệnh + kết quả / đường dẫn)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ CHỦ DỰ ÁN / AG: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets`; không commit lên `main`; không viết lại lịch sử git; không sửa/xóa `ASSET`, `SFX`, `SpriteSources`; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không tạo .md ngoài `server/README.md`; không báo xong khi thiếu bằng chứng.
