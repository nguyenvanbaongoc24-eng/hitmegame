# Beta P6 — kiểm tải cục bộ

## Lượt chốt: lỗi phân bổ theo seed

- Lệnh: `node server/Tools/RenderLoad.mjs ws://127.0.0.1:5080/ws --seconds 900 --interval 30 --chaos` trên server cục bộ không bật Supabase. Đầu vào theo seed `(cycle, room size)`, nên các ca rớt mạng/commit lỗi/bỏ commit/rời trận phân bổ khác nhau nhưng tái lập được. `--render-approved` là cờ bắt buộc nếu trỏ tới host từ xa; lượt này không dùng cờ đó.
- Kết quả thật: 900 giây, 30 chu kỳ × 3 phòng đồng thời (2/4/6 người), **90/90 trận, 450/450 vòng**, 0 lỗi, 0 kết nối rớt bất ngờ, đỉnh 12 socket. Can thiệp: 68 vào lại sau ngắt mạng, 63 commit lỗi, 64 vòng không commit, 24 lượt rời giữa trận. Log gốc `P6_random_raw.txt`.
- Khóa đến kết quả: trung bình 270 ms, p95 5002 ms, lớn nhất 5096 ms. p95 gồm thời gian chờ hạn 5 giây khi thiếu commit. `/metrics` cuối lượt: 90 bắt đầu/kết thúc, 68 reconnect, 63 `bad_message`, median xử lý vòng 0.074 ms, broadcast p95 0.011 ms.
- `P6_random_resources.csv`: 31 mẫu trong 901 giây, CPU tiến trình tăng 9.328 giây (~1.04% của một lõi trung bình), working set đỉnh 135.133 MiB, private memory đỉnh 119.246 MiB. Mức này chỉ thuộc máy cục bộ và khối lượng thử trên.
- Kết luận: **đạt bài thử cục bộ 3 phòng**, chưa đo trên Render Free và không suy rộng thành ngưỡng phòng thực tế của gói đó. Chưa có bằng chứng về CPU/RAM Render, cold start hay nhiều người thật.

## Lượt thăm dò trước khi sửa công cụ

- Lệnh: `node server/Tools/RenderLoad.mjs ws://127.0.0.1:5080/ws --seconds 900 --interval 30 --chaos`.
- Kết quả: 900 giây, 30 chu kỳ, 3 phòng đồng thời (2/4/6 người), 90/90 trận và 450/450 vòng hoàn tất; 0 lỗi, 0 rớt socket ngoài chủ đích. Dữ liệu gốc: `P6_chaos_raw.txt`.
- Can thiệp có kiểm soát: 90 vào lại sau rớt mạng, 90 commit sai bị từ chối, 90 vòng không commit, 30 người rời giữa trận. Các sự kiện được đặt cố định theo vòng và loại phòng; **chưa ngẫu nhiên hóa**.
- Độ trễ từ khóa đến `round_result`: trung bình 338 ms, p95 5019 ms, lớn nhất 5104 ms. Các vòng không commit phải đợi hạn 5 giây; đây không phải thời gian tính Core. `/metrics` sau chạy: 90 trận bắt đầu/kết thúc, 90 reconnect, median xử lý vòng 0.093 ms, broadcast p95 0.019 ms.
- Bộ lấy mẫu CPU/RAM đầu tiên chọn nhầm PID 0 của Windows (`P6_resources.csv`), không hợp lệ. Mẫu đúng tiến trình `HitMe.Server` ở `P6_resources_valid.csv` chỉ bao phủ phần cuối thử nghiệm và sau thử nghiệm; đỉnh working set khoảng 134 MiB, private memory khoảng 118 MiB. Không đủ để kết luận đỉnh toàn bộ 15 phút.
- Một lượt đầu thất bại do công cụ tạo tên dài hơn 24 ký tự, bị cắt thành tên trùng; log `P6_local_raw.txt` giữ nguyên. Đã sửa tên ngắn trước lượt 900 giây trên.
- Lượt phân bổ theo seed và lấy mẫu đầy đủ ở phần trên thay thế giới hạn của lượt này. Hai bộ log đều được giữ để thấy lỗi công cụ và cách sửa.
