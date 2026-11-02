# HIT ME – LỆNH CHO CODEX, ĐỢT 4

> Dán cho Codex, kèm `hit-me-gameplay-rules-v1.2.md` (bản mới nhất) và `hit-me-codex-round3.md` (mô tả S2–S4, P4, R3, P5, P6 vẫn có hiệu lực). **Commit nhỏ; chỉ báo "pass" khi dán được dòng `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Đánh giá báo cáo vừa rồi
| Việc | Kết quả | Ghi chú |
|---|---|---|
| R1 | **Chưa hoàn tất** (Codex tự báo đúng) | Có `Passed! 17/17` và 6 replay, nhưng replay chưa chứa trạng thái mong đợi nên chưa tự đối chiếu được. Tức là replay mới là dữ liệu, chưa là bằng chứng. |
| R2 | **Hoàn tất một nửa, hợp lý** | `core_cleanup.patch` đã qua `git apply --check`. Chỉ chuyển `CharacterData`/`WeaponData` (kèm `.meta`, giữ GUID). Không archive `BotBrain`, `GameModels`, `MatchStateMachine` vì "còn phụ thuộc mã/scene": chấp nhận là thận trọng, nhưng **chưa cho biết phụ thuộc đó là gì** nên chưa quyết được. |
| S1 | **Đạt** | Phòng, `HMxxxx`, 2–6 người, `room_state`, kiểm tra đầu vào, `Passed! 7/7`. |
| S2–S4 | Chưa làm | Phần quan trọng nhất (bí mật vị trí) chưa có. |

## T1 – Hoàn tất R1: replay tự đối chiếu (làm đầu tiên, ngắn)
1. Mỗi `replays/*.json` thêm khối `expected`: `{ hasPlaced, position, facing, aimActive }` **sau mỗi sự kiện quan trọng** và ở trạng thái cuối (sau `Lock()`).
2. `GestureReplay` chạy từng tệp và so sánh với `expected` (sai số số thực 1e-4); một test chạy tất cả tệp trong thư mục (thêm tệp mới tự được kiểm).
3. Bắt buộc có replay cho đúng luật v1.2 hiện hành: **chạm mới ở chỗ khác → hướng reset về mặc định**; ngắm rồi kéo ngược dưới 4 mm → hướng về mặc định; chạm bắt đầu ngoài sân; ngón thứ hai bị bỏ.
4. Dán `Passed! N/N` mới.

## T2 – Hoàn tất R2: báo cáo phụ thuộc còn lại (chỉ đọc, ngắn)
Trả lời **một bảng** cho `BotBrain.cs`, `GameModels.cs`, `MatchStateMachine.cs`: ai tham chiếu tới nó (tệp + dòng, hay GUID trong `.unity/.prefab/.asset`), loại tham chiếu (code / scene / asset), và có thể thay bằng lớp tương ứng của `Core/Phase1` mà không đổi hành vi không. Cuối bảng: **đề xuất 3 lựa chọn cho AG** (giữ tạm / chuyển sang `_Archive` kèm sửa tham chiếu / xóa), mỗi lựa chọn kèm rủi ro. **Không sửa gì.**

## S2 – Vòng chơi + giữ bí mật (việc chính, làm ngay sau T1, T2)
Theo mục S2 của `hit-me-codex-round3.md`. Nhắc các điểm hay sai:
- Server chỉ nhận trạng thái cuối `commit = { roundId, position, aimDir | null }`; không nhận hay phát từng bước cử chỉ.
- **Bí mật:** trước khi khóa không thông điệp nào gửi cho A chứa vị trí/hướng của B; test quét **toàn bộ** byte đã gửi cho từng kết nối (kể cả `room_state`, `error`, `round_start`).
- Khóa khi hết `placeSeconds` **hoặc** tất cả `ready`; tính bằng `RoundResolver` của Core; `reveal` + `round_result` phát đồng thời cho mọi người.
- `aimDir = null` → `ThrowFacing` bằng **cùng `DefaultFacing`** với cử chỉ ở R1; thêm test xác nhận hai nơi bằng nhau.
- Người chưa `commit` khi hết giờ: đứng vị trí vòng trước (vòng 1: ngẫu nhiên theo `seed`), `ThrowFacing`; `commit` trễ → bỏ qua; hai `commit` cùng vòng → lần sau đè lần trước cho tới khi khóa.
- Thời gian qua `TimeProvider`, test bằng đồng hồ giả; `match_end` khi hết `maxRounds` hoặc còn ≤ 1 người, xếp hạng theo HP rồi số lần trúng.
- Test tương đương Core: cùng `seed` và cùng `commit` → kết quả từng vòng giống hệt chạy trực tiếp `Core/Phase1`.

Sau S2 mới làm S3, S4, P4 theo thứ tự cũ.

## Mẫu báo cáo
```
VIỆC: T1 | T2 | S2 | S3 | S4 | P4 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N")
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai thật; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả `dotnet test`.
