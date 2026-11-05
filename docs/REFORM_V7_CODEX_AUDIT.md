# HIT ME V7 — audit kỹ thuật (2026-11-05)

## Phạm vi đã kiểm

- Worktree riêng `feature/codex-reform-v7`, gốc `7725fbc`; không sửa worktree Unity của Antigravity.
- Unity 6 `6000.6.4f1`; scene build `Assets/Scenes/Graybox.unity`; build qua `HitMe.Editor.WebBuildPipeline.BuildWebRelease` và template `Assets/WebGLTemplates/Phase1Mobile/index.html`.
- Home, phòng, bot, Settings và các hộp thoại là Unity uGUI, dựng từ `Assets/Editor/ProductionUISetup.cs`, điều khiển bởi `Assets/Scripts/UI/ProductionUIController.cs`. WebGL template chỉ lo tải canvas, trạng thái tải và lỗi.
- Client online ở `Assets/Scripts/Net/`; URL và giao thức ở `MD/NET_PROTOCOL.md`. Render `/healthz` trả 200 nhưng `POST /guest-token` trả 503 trong lần kiểm cuối, nên chưa xác nhận luồng vào phòng thật.

## Nguyên nhân và thay đổi

| Phát hiện | Sửa / trạng thái |
|---|---|
| Loader từng dùng tiến độ và trạng thái E2E tự tạo | Đã chuyển sang tiến độ Unity đo được, hiển thị lỗi và retry (`82528f2`, `d7e49fd`). |
| Mã phòng mẫu có sẵn trong ô nhập | Đã xóa (`dbb3018`). |
| Xếp hạng mẫu dễ bị hiểu là dữ liệu thật | Đã ngừng hiện hàng mẫu; chưa nối bảng xếp hạng từ server (`2cec5c4`, `bf65ffa`). |
| WebGL độc lập tắt âm Unity dù không có web UI nhận sự kiện | Đã cho Unity phát âm ở chế độ độc lập; chế độ nhúng vẫn giao web UI (`f0a7190`, `855c1c1`). Âm nghe được trên iPhone chưa xác nhận. |
| Settings đọc mức âm từ `AudioService` trong khi BGM phát qua `AudioManager` | Đã đọc giá trị từ `AudioManager`, giữ cập nhật hai lớp; thả thanh SFX phát âm thử (`dd3fd8b`). |
| Tab Home gán nhầm sang Login; Rank/Quest chỉ được gắn listener ở Editor | Đã gắn listener lúc chạy game, chuyển Login sang nút rõ ràng trong Hồ sơ (`4ce207e`). |

## Chưa đủ bằng chứng

Các sửa trên chưa được tích hợp vào Vercel production. Chưa xác nhận nghe nhạc/SFX trên iPhone, phòng online thật, hồ sơ và số dư tồn tại qua thiết bị khác, hoặc trận nhiều người trên production. Không dùng các test EditMode làm bằng chứng thay cho thử tay những mục này.
