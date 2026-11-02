# MD Migration Report

Ngày thực hiện: 27/10/2026

## Phạm vi và sao lưu

- Nguồn xử lý: `D:\HITME GAME\MD`.
- Root lỗi thời do người dùng chỉ định và mọi biến thể slash/escape/URL encoding đã được đưa vào phép quét.
- Root mới duy nhất: `D:\HITME GAME`.
- Bản sao lưu trước chỉnh sửa: `D:\HITME GAME\_backups\MD_20261027_214436`.
- Chỉ tài liệu Markdown trong thư mục MD được sửa; không sửa code, scene, prefab hoặc asset.

## Kiểm kê ban đầu

| File | Kích thước ban đầu | Vai trò |
|---|---:|---|
| `HITME_MASTER_PROJECT_V1.md` | 26.074 bytes | Đặc tả/master rebuild |

Thư mục `D:\HITME GAME\MD\HITME GAME MD` tồn tại nhưng không chứa file Markdown tại thời điểm kiểm kê.

## File đã sửa hoặc tạo

| File | Thay root cũ | Nội dung cập nhật |
|---|---:|---|
| `HITME_MASTER_PROJECT_V1.md` | 0 | Chuyển vị trí master sang MD; ghi project/repo/worktree/Unity/renderer/scene/build thực tế; hạ các đề xuất chưa tồn tại xuống CẦN XÁC MINH |
| `README.md` | 0 | Tạo mục lục và thứ tự ưu tiên tài liệu |
| `MD_MIGRATION_REPORT.md` | 0 | Tạo báo cáo migration |

Không có biến thể nào của root lỗi thời trong bộ MD ban đầu, vì vậy số lần thay root là 0. Các đường dẫn `D:\App\UNITY`/`D:\App\BLENDER` và `C:\Python314` là đường dẫn công cụ, không phải root project nên được giữ.

## Chỉ dẫn lỗi thời đã cập nhật

- Master không còn chỉ agent đặt tài liệu tại `D:\HITME GAME\docs`; master duy nhất nằm trong `D:\HITME GAME\MD`.
- “Xây project sạch” được thay bằng bảo toàn project thực tế tại `D:\HITME GAME\unity-client`, không tạo thêm project.
- Unity được chốt theo máy: `6000.6.4f1`; renderer hiện tại là Built-in, UI là uGUI.
- Asset source được chốt tại `D:\HITME GAME\ASSET HITME FINAL`; trạng thái chất lượng/tích hợp vẫn phải theo manifest.
- Scene thực tế là `Assets/Scenes/MainMatch.unity`, scene duy nhất bật trong Build Settings. Boot/MainMenu/Battle tách riêng không còn được mô tả như hiện trạng.
- Worktree Codex/AG và branch thực tế đã được ghi vào master.
- WebGL output và localhost:8015 được mô tả là quy trình đã dùng trong báo cáo cũ, phải kiểm tra lại ở mỗi HEAD/build.
- Cổng M0 không còn yêu cầu “repo sạch” theo nghĩa xóa thay đổi; yêu cầu là kiểm kê và bảo toàn thay đổi chưa commit.

## Đường dẫn đã xác minh

- Tồn tại: `D:\HITME GAME\unity-client`.
- Tồn tại: `D:\HITME GAME\worktrees\codex-unity-core`.
- Tồn tại: `D:\HITME GAME\worktrees\ag-visual-ui`.
- Tồn tại: `D:\HITME GAME\ASSET HITME FINAL`.
- Tồn tại trong worktree Codex: `Builds\WebGL`.
- Tồn tại trong cả hai worktree: `coordination` và `docs`.

## Đường dẫn chưa tồn tại hoặc chưa xác minh

- Không tồn tại tại root: `D:\HITME GAME\reference`.
- Không tồn tại tại root: `D:\HITME GAME\source-assets`.
- Không tồn tại tại root: `D:\HITME GAME\art-work`.
- Không tồn tại tại root: `D:\HITME GAME\builds`.
- Không tồn tại tại root: `D:\HITME GAME\coordination`.
- `Boot`, `MainMenu` và `Battle` chưa phải các scene build hiện tại — **CẦN XÁC MINH** trước khi tách scene.
- URL `http://localhost:8015/` xuất hiện trong báo cáo runtime cũ; tình trạng server/build hiện hành phải kiểm tra lại — **CẦN XÁC MINH**.

## Mâu thuẫn và trạng thái Git cần lưu ý

- `D:\HITME GAME` không phải Git root trực tiếp. Repository chính là `D:\HITME GAME\unity-client`.
- `unity-client`: nhánh `main`, HEAD tại thời điểm kiểm kê `3caaf5a73a756dbb64e704c1df935a2327387bab`, working tree không có thay đổi được báo cáo.
- Worktree Codex: `feature/codex-unity-core`, HEAD `b24fc105cb1ea43e36a580e0b0e61477bdf87b47`; có các log build chưa theo dõi, đã được bảo toàn.
- Worktree AG: `feature/ag-visual-ui`, HEAD `113770e9730dd74ca808c1c0b153b0ca78aa6552`; có asset, prefab, scene staging và báo cáo chưa theo dõi, đã được bảo toàn.
- Có project `D:\HITME GAME\My project`, nhưng không phải canonical gameplay project. Không chọn project này chỉ vì cấu hình URP của nó.
- Các báo cáo chuyên đề hiện có không thống nhất hoàn toàn về mức độ hoàn thiện arena/UI/runtime. Master yêu cầu kiểm chứng lại trên HEAD và không kế thừa trạng thái hoàn thành.

## Kiểm tra sau migration

- Quét root cũ: không còn kết quả trong `D:\HITME GAME\MD`.
- Link nội bộ mới: README liên kết được tới master và báo cáo cùng thư mục.
- Encoding: UTF-8 và tiếng Việt được giữ nguyên.
- Code block/nội dung gameplay cốt lõi không bị viết lại.
- Không tạo các đường dẫn còn thiếu và không thực thi prompt gameplay/build cũ.
