# HIT ME V7 — ma trận hành động

`Đã gắn` chỉ xác nhận source và scene builder; cột “thực tế” phân biệt kiểm trong browser với phần chưa thử trên thiết bị. Build của nhánh này chưa lên production.

| Màn | Nút / thao tác | Kết quả mong đợi | Nơi gắn | Thực tế / trạng thái |
|---|---|---|---|---|
| Loading | Thử lại | Tải lại sau lỗi | `Phase1Mobile/index.html` | Đã có xử lý; chưa ép lỗi mạng trên iPhone. |
| Home | Trang chủ | Giữ Home | `ProductionUIController.BindEvents` | Đã sửa gán nhầm Login; test scene 202/202. |
| Home | Xếp hạng | Mở bảng, không bịa số liệu | `BindEvents → ShowLeaderboardDialog` | Đã mở và đóng được trong WebGL cục bộ; bảng ghi “Chưa có dữ liệu”. Bảng thật từ server chưa nối. |
| Home | Nhiệm vụ | Thông báo chưa khả dụng | `BindEvents → ShowToast` | Đã gắn; thao tác browser chưa cho kết quả ổn định, cần thử cảm ứng thật. Nhiệm vụ thật chưa nối. |
| Home | Hồ sơ | Mở hồ sơ | `BindEvents → ShowProfileDialog` | Có handler; dữ liệu liên thiết bị chưa xác minh. |
| Hồ sơ | Đăng nhập | Mở hộp đăng nhập | `BindEvents → ShowLoginDialog` | Đã tách khỏi tab Home. Google/Facebook/Apple đang vô hiệu. |
| Home | Cài đặt | Mở Settings | `BindEvents` | Đã mở trong browser cục bộ. |
| Settings | Nhạc / SFX | Đổi mức phát và lưu | `BindEvents`, `AudioManager`, `AudioService` | Đã đồng bộ mức; âm nghe trên iPhone chưa xác minh. |
| Settings | Thả thanh SFX | Phát âm thử một lần | `SfxSliderPreview` | Đã biên dịch/test suite; chưa thử nghe thực tế. |
| Home | Tạo phòng | Gửi yêu cầu và chờ `room_state` | `BindEvents`, online client | Server production `guest-token` trả 503; chưa đạt E2E. |
| Home | Vào phòng | Kiểm mã, gửi join và chờ `room_state` | `BindEvents`, online client | Chưa đạt E2E trên production. |
| Home | Chơi nhanh | Vào hàng chờ thật | `BindEvents`, online client | Chưa đạt E2E trên production. |
| Home | Luyện với bot | Chọn bot/nhân vật, vào trận | `BindEvents`, `MatchController` | EditMode đạt; thử tay trên bản mới chưa xác nhận. |
| Lobby | Sẵn sàng / Rời phòng | Đồng bộ trạng thái server | `BindEvents`, online client | Chưa xác nhận hai điện thoại cùng phòng. |
| Trận | Đặt / ngắm / khóa | Cử chỉ và thời gian theo Core | `MatchController` | Test logic có; bản mới chưa thử cảm ứng iPhone. |
| Kết quả | Chơi lại / Về Home | Khởi tạo trận mới / về Home | `BindEvents` | Test logic có; bản mới chưa thử tay. |

Các nút cửa hàng và túi đồ Home còn bị ẩn trong `ProductionUISetup`; không coi là đã phát hành. `Xếp hạng` hiện chỉ mở trạng thái không có dữ liệu, không có bảng người chơi thật.
