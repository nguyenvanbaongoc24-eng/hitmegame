# HIT ME – LỆNH BỔ SUNG CHO CODEX: máy trạng thái cử chỉ v1.2 (việc Q1)

> Dán cho Codex **cùng file** `hit-me-gameplay-rules-v1.2.md` (đọc kỹ mục 2–7) và `hit-me-codex-next-tasks.md`. **Thứ tự mới: P1 → Q1 → P3 → P2 → P4 → P5 → P6.** Commit nhỏ (`WIP:` nếu dang dở). Nếu sắp hết token: P1 và Q1 trước, rồi ghi "Đang dở: ..." vào `server/README.md`.

## Quy tắc (giữ nguyên)
Không mở Unity; không sửa `unity-client/Assets`, `ProjectSettings`, `Packages`, `MD/`; làm trên `prep/codex`; không đổi luật chiến đấu; không sửa Core; không tạo .md mới ngoài `server/README.md`.

## Q1 – Viết sẵn máy trạng thái cử chỉ v1.2 + test ngoài Unity

**Mục đích:** AG đang bận trong Unity và thường hết token. Bạn có SDK .NET 8 nên viết và kiểm thử phần thuần C# ở ngoài Unity, để AG chỉ việc chép vào.

1. **Đọc (không sửa):** tìm `PlacementGestureState`, `PointerSample`, `TouchPhase` trong `unity-client/Assets/Scripts` (Core/Phase1 hoặc Game). Giữ **hình dạng giao diện càng gần càng tốt** để AG thay thế dễ.
2. **Tạo** `tools/patches/gesture_v2/` gồm:
   - `PlacementGestureV2.cs`: C# thuần, **không** `using UnityEngine`, namespace theo quy ước của Core/Phase1. Sự kiện vào: `PointerDown/Move/Up(pointerId, screenPos, gamePos, insideArena, timeMs)`, `Lock()`, `BeginRound(previousPosition, defaultFacing)`. Trạng thái ra (`Snapshot`): `hasPlaced`, `position`, `facing`, `aimActive`, `gestureId`.
   - Ngưỡng `minAimDrag` nhận bằng **pixel màn hình** (đã quy đổi từ mm ở lớp ngoài, kèm hàm `MmToPixels(mm, dpi)` có dự phòng dpi = 160).
   - Hướng = `chuẩn_hóa(gamePos(ngón) − gamePos(điểm chạm))`; `DefaultFacing` lấy từ **một hằng số dùng chung** (đề xuất đặt trong lớp Core có sẵn, nếu không có chỗ thích hợp thì để trong file patch và ghi rõ vị trí đề xuất).
   - `GestureReplay.cs` (hoặc trong test): đọc `replays/*.json` (danh sách sự kiện + kết quả cuối mong đợi) để phát lại, dùng cho test và để AG nối với bản ghi cảm ứng thật sau này.
   - `tests/`: dự án xUnit hoặc NUnit nhỏ (`.NET 8`) chạy bằng `dotnet test`, **15 test** theo mục 6 của file luật, thêm các biên: `NaN`/`∞` trong tọa độ (bỏ qua sự kiện), nhiều `pointerId`, sự kiện sau `Lock()`.
   - `replays/`: tối thiểu 6 chuỗi (đặt+ngắm; đặt rồi dời; ngắm rồi hủy; không chạm; không trượt; chạm ngoài sân).
3. **Bằng chứng:** `dotnet test` xanh toàn bộ (dán kết quả: số test, thời gian). Chỉ liên kết các tệp Core cần thiết (Vec2/kiểu số học) bằng `Link`, **không sao chép, không sửa Core**.
4. Viết hướng dẫn tích hợp ngắn (≤ 12 dòng) **ngay trong chú thích đầu `PlacementGestureV2.cs`** (không tạo .md): chép tệp vào đâu, thay lớp nào, gọi ở đâu (`OnPointer*` từ `PointerSample`), test nào chuyển sang NUnit EditMode.
5. Hằng số hướng mặc định: nếu `ThrowFacing` ở server đang dùng giá trị riêng, ghi chú chỗ lệch và thêm **một test server** xác nhận server và client dùng cùng `DefaultFacing` (khi P3 xong).

Commit/push nhánh `prep/codex`, báo cáo ngắn theo mẫu.

## Báo cáo cuối
```
VIỆC: Q1
ĐÃ LÀM: ...
FILE: tools/patches/gesture_v2/...
BẰNG CHỨNG: dotnet test: N/N pass
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```
