# Chẩn đoán Online từ WebGL

Client gọi `GET /healthz` trước, với timeout tối đa 60 giây. Endpoint này có CORS `*`,
không dùng cookie và không trả danh sách Origin. Sau khi server sống, client gọi
`POST /guest-token`, rồi mới mở WebSocket.

| Tín hiệu quan sát | Câu hiển thị cho người chơi | Hành động UI |
|---|---|---|
| `/healthz` không phản hồi hoặc quá hạn | Máy chủ đang khởi động hoặc không truy cập được. | Hiện đếm tối đa 60 giây, **Thử lại** và **Luyện với bot**. |
| `/healthz` OK, nhưng `/guest-token` lỗi CORS/mạng | Origin chưa được cho phép. | Không lặp WebSocket; báo cấu hình server cho vận hành. |
| `/healthz` OK, `/guest-token` 200, WebSocket đóng 1006 | Kết nối WebSocket bị gián đoạn. | Thử lại có giới hạn; không gán nhầm là lỗi Origin. |
| `/healthz` OK nhưng `protocol` khác bản client | Có bản mới – tải lại. | Nút tải lại; không mở phòng. |
| `/healthz.draining` là `true` | Máy chủ đang khởi động lại. | Chờ ngắn rồi thử lại; không tạo phòng mới. |

`origin_not_allowed` trong HTTP 403 chỉ thấy được bởi công cụ thô vì trình duyệt không
cho JavaScript đọc body của WebSocket handshake bị từ chối. Vì vậy UI dùng kết hợp
healthz và guest-token như bảng trên, thay vì suy diễn từ WebSocket 1006.
