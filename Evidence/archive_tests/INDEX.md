# THƯ MỤC LƯU TRỮ KẾT QUẢ KIỂM THỬ LỊCH SỬ (ARCHIVE TESTS INDEX)

> **Quy tắc**: Bản chuẩn duy nhất của bộ kiểm thử EditMode hiện tại được lưu tại `Evidence/phase3/test-results.xml`. Toàn bộ các tệp XML trung gian ghi nhận từng mốc phát triển lịch sử trước đây được chuyển về thư mục này để đảm bảo cấu trúc thư mục gọn gàng, minh bạch.

| Tệp XML lưu trữ | Mốc / Giai đoạn | Tổng số test | Đạt (Pass) | Hỏng (Fail) | Bỏ qua | Mục đích lịch sử |
|---|---|:---:|:---:|:---:|:---:|---|
| `test-results-replay.xml` | Phase 2 Replay Core | 51 | 51 | 0 | 0 | Kiểm thử 3 test replay cơ bản |
| `test-results-changa.xml` | Phase 2 Changa Baseline | 51 | 51 | 0 | 0 | Kiểm thử tương thích hồi quy baseline |
| `test-results-d0.xml` | D0 Polish & Gesture V2 | 62 | 62 | 0 | 0 | Kiểm thử 12 replay cử chỉ V2 + 10 visual contract |
| `test-results-p0.xml` | P0 Production UI | 72 | 72 | 0 | 0 | Kiểm thử hợp đồng giao diện Home, Lobby, Gameplay |
| `test-results-p1.xml` | P1 Match Authority | 74 | 74 | 0 | 0 | Kiểm thử thẩm quyền MatchState.Phase và kết thúc Vòng 5 |
| `test-results-p2p3.xml` | P2 & P3 SafeArea/HUD | 77 | 77 | 0 | 0 | Kiểm thử SafeArea và thẻ HP trên cổng làng |
| `test-results-s1-s3.xml` | S1-S3 Ranking Core | 86 | 86 | 0 | 0 | Kiểm thử MatchRanking trong Core/Phase1 |
| `test-results-p4.xml` | Luật v1.3 & Phase 2 Final | 92 | 92 | 0 | 0 | Kiểm thử phòng 1-6 người và xếp hạng N=2..6 |
| `test-results-round27.xml` | Đợt 27 Visual & Sprite Audit | 98 | 98 | 0 | 0 | Kiểm thử sprite sạch và F9 blind bot |
| `test-results-compile.xml` | Build Verification | 92 | 92 | 0 | 0 | Kiểm thử biên dịch trung gian |
| `test-results-head.xml` | Release Verification | 92 | 92 | 0 | 0 | Kiểm thử xác nhận commit HEAD |
| `editmode_results.xml` | Unity Batchmode Raw Output | 98 | 98 | 0 | 0 | Tệp xuất thô của Unity Editor test runner |
