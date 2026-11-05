# HIT ME – chỗ lệch cầu nối cần Antigravity xử lý

Đối chiếu `unity-client/Assets/Scripts/UI/WebBridge.cs` ngày 05-11-2026. Codex chỉ sửa `web-ui`, không sửa Unity.

1. **Cao – bí mật:** `EmitMatchState` hiện gửi `x,y` của mọi người cả trong `placing`; `EmitEmote` gửi `screen.x/y` trong mọi pha. Web không vẽ các tọa độ đó, nhưng client vẫn nhận được. Unity phải bỏ tọa độ đối thủ trước công bố; kiểm bằng transcript thô.
2. **Trung bình – kết quả:** `result.rankings` có `hits` nhưng thiếu `characterId`, `roundSurvived`. Web đang hiện `—` cho số vòng sống và chỉ có ảnh đầu dự phòng. Bổ sung từ trạng thái trận thật.
3. **Trung bình – bố cục:** `SetLayout` gửi `safeTop/safeBottom`; Unity hiện chỉ dùng `top/bottom`, và `EmitLayoutEvent` tính theo khung cố định 393×695. Kiểm iPhone notch, Safari thanh địa chỉ co, Android 20:9, xoay ngang.
4. **Trung bình – cài đặt:** Unity `SetSettings` chỉ xử lý `reducedMotion`; mức âm thanh do Web xử lý. Cần xác nhận Unity AudioSource không phát chồng ở bản Web.
5. **Thấp – sự kiện:** `event` chưa có tọa độ khi đã công bố, Web chỉ hiện toast và âm. Nếu muốn VFX web bám nhân vật, phát tọa độ sau công bố.

## Bản `/next/` đang phát hành, kiểm ngày 05-11-2026

- `app.js` tại Vercel có SHA-256 bắt đầu `84697d77e2663c89`; `web-ui/dist/app.js` ở nhánh `prep/codex` mới hơn. Cổng triển khai nay so 6 tệp (`app.js`, `style.css`, `catalog.json`, `live.js`, `audio.js`, `bridge.js`); 4 tệp đầu chưa khớp. Kiểm thử gameplay hybrid bị chặn bằng `deploymentSynced=false`, không tính là đạt.
- Ảnh Home đầu tiên bị thiếu nhân vật do chụp trước khi `.hero` tải xong. Kiểm lại sau khi `naturalWidth > 0` thấy nhân vật; đây là lỗi của công cụ chụp.
- Bản đang phát hành thực sự tràn nhãn TẠO PHÒNG/VÀO PHÒNG 29–30 px trên iPhone 393×695. Bản web mới đã tách nhãn và trạng thái “Sắp ra mắt” thành hai dòng; `test_home_geometry.py` kiểm 6 kích thước. AG cần lấy `web-ui/dist` từ `prep/codex` mới nhất, deploy lại `/next/`, rồi Codex chạy lại kiểm thử hybrid có gameplay thật.
