# HIT ME – LỆNH CHO CODEX: kiểm tra độc lập Phase 2 của AG + server Phase 4

> Dán toàn bộ file này cho Codex. Codex đã xong Việc A (quét asset) và Việc C (SQL nháp) trên nhánh `prep/codex` (commit `31cbe03`, `830665d`); Việc B (server) bị chặn vì máy thiếu .NET 8 SDK. **Commit nhỏ và thường xuyên** (`WIP:` nếu dang dở), vì có thể hết token bất cứ lúc nào.

---

## 0. Quy tắc

1. **KHÔNG mở Unity** (AG đang dùng project). Không chạy Unity batchmode/build.
2. **KHÔNG sửa** `unity-client/Assets`, `ProjectSettings`, `Packages`, `MD/`. Làm trên nhánh `prep/codex` trong worktree `worktrees/codex-prep`. Không commit lên `main`.
3. `ASSET`, `SFX`, `SpriteSources` chỉ đọc.
4. Không đổi luật. Luật tham chiếu: đặt bất kỳ đâu + giữ rồi lướt kéo thước ngắm trong 5 giây, hết giờ lộ diện và tự ném, vũ khí dừng ở người đầu tiên trúng, 3 HP, sân 9,0 × 11,4, `maxRounds = 5`, hòa HP → theo số lần trúng, `NoAimPolicy = ThrowFacing`, tối đa 6 người, 7 nhân vật, không thú cưng.
5. Không tạo .md ngoài `server/README.md`. Kết quả kiểm tra (Việc 1) báo ngay trong khung chat.

---

## VIỆC 0 – Cài .NET 8 SDK (chủ dự án làm hoặc cho phép)

`dotnet --version` báo thiếu SDK (chỉ có runtime 6). Cách làm, theo thứ tự:
1. Chủ dự án chạy trong PowerShell: `winget install Microsoft.DotNet.SDK.8`, rồi **mở lại terminal**, kiểm tra `dotnet --list-sdks` phải có `8.0.x`.
2. Nếu winget không được: tải `dotnet-install.ps1` từ trang chính thức của Microsoft và chạy `./dotnet-install.ps1 -Channel 8.0 -InstallDir "$env:USERPROFILE\dotnet8"`, rồi thêm thư mục đó vào PATH của phiên làm việc.
3. Vẫn không có SDK → **dừng Việc 2**, nói rõ lý do. **Không** viết lại luật game bằng ngôn ngữ khác (sẽ có hai bản luật lệch nhau).

## VIỆC 1 – Kiểm tra độc lập Phase 2 của AG (chỉ đọc, làm ngay, không cần SDK)

AG tự báo "đạt chuẩn 100%" (commit đổi 16 file, +1168 −621) nhưng chưa có bằng chứng đủ. Bạn là người kiểm tra thứ hai, **không sửa code**, chỉ báo cáo.

Lấy mã mới nhất: `git fetch`, xem nhánh/commit Phase 2 của AG (hỏi chủ dự án nếu không thấy), đọc bằng `git diff` và `git show`, không checkout lên thư mục AG đang dùng (dùng worktree riêng chỉ đọc).

Kiểm tra và báo cáo (mỗi mục: ĐẠT / KHÔNG ĐẠT / KHÔNG XÁC ĐỊNH + dòng/tệp bằng chứng):
1. **Xóa file:** liệt kê mọi file bị xóa/đổi tên. Có file nào trong `ASSET`, `SFX`, `Evidence`, `MD`, hay `LocalBot.unity` bị đụng không? Giải thích −621 dòng.
2. **Core:** diff `Scripts/Core` so với tag `v0.1-graybox`. Core vẫn **không** có `using UnityEngine`? asmdef `noEngineReferences: true` còn? Có thay đổi logic combat ngoài danh sách cho phép (tham số `GameConfig`, xếp hạng/hòa khi hết vòng, lớp chuyển đổi tọa độ) không?
3. **Số "ma thuật" (hard-code):** `0.45`, `0.55`, `5f`, kích thước sân, `minAimDrag` có nằm ngoài `GameConfig` không?
4. **GC / hiệu năng:** `new`, `GetComponent`, `Find*`, `Shader.Find`, `new Material`, LINQ, nối chuỗi trong `Update`/`LateUpdate`/vòng chơi; tạo/hủy vật thể mỗi ván thay vì object pool.
5. **Phối cảnh:** homography có `GameToScreen` và `ScreenToGame` khứ hồi? Có test (4 góc, khứ hồi < 0,01)? Hướng ngắm đi qua `ScreenToGame` trước khi đưa vào Core?
6. **Nhập liệu:** mouse và touch có cùng một lớp pointer? Vòng chạm có tách khỏi kích thước hiển thị, tính theo mm (`Screen.dpi`, dự phòng 160)? `minAimDrag` quy đổi từ mm?
7. **Bí mật vị trí:** trong pha đặt, code có vẽ nhân vật/hướng ngắm của người khác không? (UI hay bong bóng biểu cảm có nguy cơ lộ vị trí không?)
8. **Đội hình:** 6 người một trận lấy từ 7 nhân vật; còn chỗ nào dùng tint/"Nam Làng Tint" để thay chỗ? Còn từ "pet/thú cưng" trong code/prefab/tên asset?
9. **`MatchStateMachine`:** nằm ở Core (C# thuần) hay ở `Game/`? Có phụ thuộc Unity? Server có thể dùng lại không?
10. **Test:** danh sách test hiện có (tên + số lượng) và mục luật nào **chưa** có test.
11. Báo cáo trùng: AG đã tạo `PHASE2_REPORT.md` ở 3 nơi; liệt kê và băm để biết có khác nhau không.

Xuất báo cáo ≤ 1 trang trong khung chat, **mục KHÔNG ĐẠT lên đầu**.

## VIỆC 2 – Server Phase 4 (cần SDK; làm sau Việc 0 và Việc 1)

Làm đúng như "VIỆC B" trong `hit-me-codex-parallel-tasks.md` (đính kèm), với các **cập nhật**:
- Lấy Core từ commit mới nhất mà AG đã gắn `v0.2-rc1`; nếu chưa có thì dùng `v0.1.1-balance`. Ghi rõ commit đang dùng.
- Thông số: sân 9,0 × 11,4; `playerRadius` đúng giá trị trong `GameConfig` hiện hành (không hard-code); `maxRounds = 5`; hòa HP → số lần trúng; `NoAimPolicy = ThrowFacing`.
- Liên kết file (`<Compile Include ... Link>`), **không sao chép, không sửa Core**. Không biên dịch được → báo chỗ nào và dừng.
- Server là **WebSocket thuần trên Render**, không dùng Supabase Realtime cho trận đấu.
- Bắt buộc có **test giữ bí mật**: trước thời điểm khóa không có thông điệp nào gửi cho client A chứa vị trí/hướng của client B.
- Tài liệu duy nhất: `server/README.md` (≤ 1,5 trang).
- Không triển khai, không khóa bí mật.

## VIỆC 3 – Danh sách thay thế asset lỗi (không cần SDK)

Từ kết quả Việc A, tạo `Evidence/asset_qa/REPLACE_LIST.csv` (không .md) với cột: `file_goc`, `loi` (checkerboard / viền sáng / viền đen / khung lệch / khác), `muc_do`, `ban_da_lam_sach` (đường dẫn trong `cleaned/` nếu có), `dung_cho` (nhân vật/UI/sân). Sắp theo mức độ giảm dần, đặt các file của **Nam Làng, Anh Nón Lá, Chó Vàng, Mèo Mun, dép, bóng/vòng chân** lên đầu. Đây là danh sách để chủ dự án xuất lại sprite có alpha thật hoặc để AG thay bản sạch.

---

## Báo cáo cuối mỗi việc
```
VIỆC: 0 | 1 | 2 | 3
ĐÃ LÀM: ...
FILE: ...
BẰNG CHỨNG: (lệnh đã chạy + kết quả / đường dẫn)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets`; không commit lên `main`; không sửa/xóa `ASSET`, `SFX`, `SpriteSources`; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không tạo .md ngoài `server/README.md`; không báo xong khi thiếu bằng chứng.
