# Beta P6 — kiểm tải cục bộ

- Lệnh: `node server/Tools/RenderLoad.mjs ws://127.0.0.1:5080/ws --seconds 900 --interval 30 --chaos`.
- Kết quả: 900 giây, 30 chu kỳ, 3 phòng đồng thời (2/4/6 người), 90/90 trận và 450/450 vòng hoàn tất; 0 lỗi, 0 rớt socket ngoài chủ đích. Dữ liệu gốc: `P6_chaos_raw.txt`.
- Can thiệp có kiểm soát: 90 vào lại sau rớt mạng, 90 commit sai bị từ chối, 90 vòng không commit, 30 người rời giữa trận. Các sự kiện được đặt cố định theo vòng và loại phòng; **chưa ngẫu nhiên hóa**.
- Độ trễ từ khóa đến `round_result`: trung bình 338 ms, p95 5019 ms, lớn nhất 5104 ms. Các vòng không commit phải đợi hạn 5 giây; đây không phải thời gian tính Core. `/metrics` sau chạy: 90 trận bắt đầu/kết thúc, 90 reconnect, median xử lý vòng 0.093 ms, broadcast p95 0.019 ms.
- Bộ lấy mẫu CPU/RAM đầu tiên chọn nhầm PID 0 của Windows (`P6_resources.csv`), không hợp lệ. Mẫu đúng tiến trình `HitMe.Server` ở `P6_resources_valid.csv` chỉ bao phủ phần cuối thử nghiệm và sau thử nghiệm; đỉnh working set khoảng 134 MiB, private memory khoảng 118 MiB. Không đủ để kết luận đỉnh toàn bộ 15 phút.
- Một lượt đầu thất bại do công cụ tạo tên dài hơn 24 ký tự, bị cắt thành tên trùng; log `P6_local_raw.txt` giữ nguyên. Đã sửa tên ngắn trước lượt 900 giây trên.
- Kết luận: máy cục bộ chịu được mức 3 phòng/12 socket của bài thử. **Chưa đo trên Render Free**, chưa xác định giới hạn phòng/CPU/RAM thật của gói đó. Cần lượt 15 phút với ngắt mạng ngẫu nhiên và lấy mẫu tài nguyên từ đầu để khép P6.
