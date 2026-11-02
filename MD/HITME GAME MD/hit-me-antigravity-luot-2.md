# HIT ME – LƯỢT 2 CHO ANTIGRAVITY (A7 → A10)

Đầu vào: commit `46ff81e`, 142/142 EditMode test. A1–A6 xong. `DECISIONS.md` đã đúng `maxRounds = 5`.

Lưu ý: nút "+ KẾT BẠN" và chat/emote hiện chỉ chạy phía client (chưa có server). Chưa tính là hoàn chỉnh, sẽ nối khi sang Phase 3.

## Việc bạn làm tay

1. Giữ hai nhánh tách biệt: Codex làm trên `prep/codex` (thư mục `worktrees/codex-prep`), Antigravity làm trên nhánh Unity chính. Không merge code server vào thư mục Unity. Antigravity chỉ được lấy **tài liệu** từ nhánh của Codex (xem prompt).
2. Sau A7, chơi thử build trên iPhone và gửi mình ảnh chụp màn hình nếu thấy lệch mockup.

## PROMPT DÁN VÀO ANTIGRAVITY

```
Tiếp tục từ commit 46ff81e. Đọc lại MD/01_GAMEPLAY_RULES.md (v1.2), MD/DECISIONS.md, mockup trong thư mục asset/. Không sửa code server; không xóa asset/ và SFX/. Mọi thay đổi đi qua commit riêng từng nhiệm vụ.

A7. Build + bằng chứng (làm trước)
- Build WebGL Release Brotli (cổng 8103). Chụp màn hình Home, Lobby (1, 3, 6 người), Placing, Result, khay Emote vào Evidence/round38/screens/. Ghi kích thước build.
- So sánh từng ảnh với mockup tương ứng; ghi chỗ lệch THỰC TẾ vào Evidence/round38/UI_DIFF.md (không báo "đã khớp" nếu chưa so).

A8. Lớp mạng cho client (chuẩn bị Phase 3, chưa cần server chạy)
- Lấy tài liệu giao thức từ nhánh Codex, KHÔNG merge nhánh: git fetch, sau đó git show prep/codex:MD/NET_PROTOCOL.md > MD/NET_PROTOCOL.md (và chỉ tệp đó). Tài liệu có thể còn được Codex cập nhật; đọc kỹ rồi cài theo đúng nó.
- Tạo interface IGameClient (kết nối, tạo/vào/rời phòng, sẵn sàng, gửi vị trí+hướng ném, nhận sự kiện trận, chat/emote, kết bạn). Hai bản cài: MockGameClient (giữ hành vi hiện tại, dùng cho Luyện với bot và test) và WebSocketGameClient (theo NET_PROTOCOL.md, dùng WebGL jslib hoặc thư viện phù hợp cho WebGL).
- Token khách: tạo và lưu bền vững (PlayerPrefs/IndexedDB), gửi khi kết nối. Địa chỉ server cấu hình được (mặc định localhost, đổi được cho Render sau).
- Cờ chuyển: "Luyện với bot" luôn dùng Mock; "Tạo phòng/Vào phòng/Chơi nhanh" dùng WebSocket khi bật cờ, tự rơi về thông báo lỗi dễ hiểu nếu không kết nối được.
- EditMode test: phân tích tin nhắn, thứ tự sự kiện, xử lý mất kết nối + thông báo (session_replaced, phòng đầy, bị cấm).

A9. Cải thiện tải (áp dụng khuyến nghị an toàn)
- Lấy báo cáo bằng: git show prep/codex:Evidence/round39/C11_load_analysis.md > Evidence/round39/C11_load_analysis.md. Chọn các đề xuất rủi ro thấp (nén texture, cắt code/stripping, giảm kích thước asset, tải trễ phần không cần ở Home...). Áp dụng từng cái, đo lại HOME_SHOWN bằng Tools/measure_home_shown.py (3 lần, trung vị) trước và sau. Hiện: Dev 17,7 giây, Release Brotli 10,0 giây. Mục tiêu đề xuất: Release dưới 6 giây trên mạng giả lập. Không giảm chất lượng hình đến mức lệch mockup.
- Ghi bảng trước/sau vào Evidence/round38/A9_load.md. Hoàn tác thay đổi nào làm lỗi test hoặc hình xấu đi.

A10. Rà soát chất lượng hình và chuyển động
- Kiểm tra 5 nhân vật + Chó Vàng (và Mèo Mun nếu đã có): idle, di chuyển, ném, trúng đòn, ngã/loại đều chạy, hướng nhìn đúng hướng ném, không giật khi đứng yên.
- Quét toàn bộ chuỗi tiếng Việt trên mọi màn hình, báo chuỗi nào còn mất dấu hoặc tràn khung.
- Sửa các chỗ lệch ghi trong UI_DIFF.md mà sửa nhanh được; phần lớn hơn ghi thành danh sách ưu tiên.

QUY TẮC: chạy toàn bộ test 1 lần mỗi nhiệm vụ (lúc xong); build WebGL không quá 1 lần cho mỗi 2 nhiệm vụ (trừ A7, A9 cần đo); cập nhật MD/STATUS.md; không ghi "xong" khi chưa chạy và chụp; hết quota thì commit phần chạy được và ghi phần dở.

BÁO CÁO CUỐI (tối đa 12 dòng): A7–A10 xong/dở/chặn, số test, commit hash, đường dẫn ảnh/bảng đo, chỗ nào còn lệch mockup.
```

## Phase 3 thật sự (nối server) sẽ giao sau khi

- Codex báo xong C10b (client giả chơi đúng 3 trận) + C12 (token khách) và NET_PROTOCOL.md khớp thực tế.
- Antigravity xong A7 và A8.

Lúc đó việc là: bật cờ WebSocket, vào cùng phòng bằng 2 điện thoại + 1 máy tính, chơi trọn trận, kiểm vàng và kết quả.
