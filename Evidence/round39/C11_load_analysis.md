# C11 – Phân tích tải WebGL build 8c6b328

Đã chạy `python tools/analyze_webgl_load.py` → **Passed! 2/2 resource traces**. Số liệu thô: `C11_resources.json`. Edge headless, viewport 414×896, mạng giả lập 20 Mbps/40 ms. Phép đo C7 độc lập (3 lượt/build): Dev trung vị 17,672 s; Release Brotli 10,006 s.

| Build | Tệp lớn nhất | Tệp thứ hai | Phản hồi tài nguyên cuối | HOME_SHOWN | Khoảng sau tải |
|---|---|---|---:|---:|---:|
| Dev | wasm 18,47 MB (14,01 s) | data 17,49 MB (13,74 s) | 14,78 s | 17,92 s | 3,14 s |
| Release Brotli | data.br 12,04 MB (7,05 s) | wasm.br 4,39 MB (3,73 s) | 7,29 s | 9,93 s | 2,64 s |

Kích thước toàn bộ tệp build: Dev **36,31 MB**, Release **16,53 MB**. Thời gian từng tệp ở bảng là `PerformanceResourceTiming.duration`, có thể tải song song nên không cộng lại. “Khoảng sau tải” gồm giải nén, biên dịch/khởi tạo WebAssembly, khởi tạo Unity, asset và giao diện; số liệu hiện tại **không tách riêng** các phần đó. Không suy ra thời gian trên iPhone từ Edge headless.

Đề xuất cho AG, theo tác động dự kiến / độ khó:

1. **Ưu tiên cao, trung bình:** tìm asset nặng trong Unity data, giảm kích thước texture/sprite atlas và bỏ asset không dùng khỏi scene/build. `data.br` vẫn 12,04 MB, là nút thắt Release.
2. **Ưu tiên cao, trung bình:** kiểm managed code stripping và các package/assembly được kéo vào WebGL; `wasm.br` 4,39 MB. Đối chiếu chức năng trước/sau bằng PlayMode và iPhone.
3. **Ưu tiên vừa, trung bình:** chia asset chỉ dùng ở màn khác và tải sau khi Home hiện, nếu kiến trúc UI cho phép. Đừng trì hoãn asset bắt buộc để bấm Home.
4. **Ưu tiên vừa, thấp:** đảm bảo hosting gửi `Content-Encoding: br`, `application/wasm` đúng MIME và cache tệp có hash. Server đo đã làm vậy; cần xác minh host thực tế.
5. **Ưu tiên thấp trước khi đo sâu:** đo thêm bằng browser profiling trên iPhone/thiết bị thật để tách tải mạng khỏi khởi tạo; không dựa vào khoảng 2,64 s để kết luận riêng về giải nén.
