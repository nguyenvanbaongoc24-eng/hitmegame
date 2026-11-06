# Việc Unity còn chờ

- Khi `ui=web`, ẩn nhãn tên và HP vẽ trên đầu nhân vật trong sân. Web HUD đã có thẻ tên/HP riêng; CSS không điều khiển nhãn world-space của Unity. Cần cờ `showWorldLabels=false` qua `SetSettings`; chế độ `ui=unity` giữ nhãn theo thiết kế hiện có. Kiểm trên iPhone Safari 393×695 và chế độ màn hình chính 393×852. Ảnh gốc do chủ dự án gửi trong nhận xét beta.3; ảnh HUD web giả lập ở `Evidence/mobile/20261106/match_hud/` không chứng minh phần nhãn Unity đã sửa.
- Cầu nối phòng online trong Unity chưa bật; Web giữ `features.online=false` ở `/next/` cho tới khi có kiểm thử thật.
