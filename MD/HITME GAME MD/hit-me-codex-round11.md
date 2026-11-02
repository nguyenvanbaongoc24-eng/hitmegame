# HIT ME – LỆNH CHO CODEX, ĐỢT 11

> Dán cho Codex, kèm `hit-me-codex-round3.md` (mô tả S3, S4, P4, R3, P5, P6) và `hit-me-gameplay-rules-v1.2.md`. **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Đánh giá báo cáo vừa rồi
Tiến độ tốt (34 phút làm, mỗi gói một commit có kết quả test):
- **T1 đạt:** replay kiểm trạng thái sau từng sự kiện và sau `Lock()`, thêm ca ngón thứ hai; `Passed! 17/17` (`1987fbc`).
- **S2a đạt:** `Round` + `TimeProvider`, ghi đè commit trước khóa; `11/11` (`c561971`).
- **S2b đạt:** dùng `MatchState`/`RoundResolver` của Core, vị trí fallback theo `seed`, `ThrowFacing`; `14/14` (`158d437`).
- **S2c đạt có điều kiện:** commit đi qua WebSocket gateway, sau khóa phát `reveal`, `round_result`, `match_end`; `20/20` (`8449ca8`, `91da3c7`, `26f2210`; commit cuối `ddba6fe`). **Điều kiện:** tôi chưa thấy bằng chứng test giữ bí mật đủ chặt (xem Y1 bên dưới).
- Chưa làm X3/X2 vì AG chưa báo bản sạch / F1–F4: đúng.

## Y1 – Chứng minh test giữ bí mật (ngắn, làm đầu tiên)
Trả lời trong khung chat, kèm tên test và đoạn lệnh/kết quả:
1. Test nào kiểm "trước khóa không thông điệp nào gửi cho A chứa vị trí/hướng của B"? Nó **quét toàn bộ byte** đã gửi cho **từng** kết nối (kể cả `room_state`, `round_start`, `error`, thông điệp lỗi khi `commit` bị từ chối), hay chỉ vài loại thông điệp?
2. Test dùng **giá trị dấu vết** (ví dụ vị trí của B có số lẻ đặc biệt như `3.14159`) rồi tìm chuỗi đó (và dạng làm tròn) trong mọi tin gửi cho A trước khóa?
3. Có test với **6 kết nối**, không chỉ 2? Có kiểm trường hợp A reconnect trước khóa không nhận lại `commit` của B?
4. Thêm các test còn thiếu nếu có. Dán `Passed! N/N` mới.

## S3 – Vào lại + cô lập lỗi (theo `round3`)
`reconnectToken`, giữ chỗ 30 giây rồi loại; hook AFK để `TODO` rỗng; **client chậm/đứt không treo năm client còn lại** (hàng đợi gửi có giới hạn, hết hạn thì ngắt kết nối đó). Test: mất kết nối 10 giây vào lại đúng chỗ (trạng thái, HP, ready giữ nguyên, **không lộ `commit` của người khác khi vào lại**); quá 30 giây bị loại; một client không đọc tin không chặn vòng.

## S4 – Xác thực đầu vào + mô phỏng + triển khai nháp (theo `round3`)
`NaN`/∞; vị trí ngoài sân → kẹp vào sân; hướng độ dài 0/không hữu hạn → coi như không ngắm; thông điệp ≤ 1 KB, ≤ 20 tin/giây/kết nối; `GET /healthz`, đọc `PORT`; `Dockerfile` + `render.yaml` nháp (không triển khai). **Mô phỏng 20 trận × 6 bot** trong tiến trình: 0 ngoại lệ, 0 treo, mỗi trận đúng luật. Test chung `DefaultFacing` của cử chỉ và của server **bằng nhau**.

## P4 – Vector vàng
200 vòng mẫu từ `Core/Phase1` (nhiều vị trí/hướng, biên: đứng sát nhau, hai người cùng một đường ném, người bị loại, trúng đồng thời) → `server/Tests/golden/rounds.json` kèm `seed` và phiên bản `GameConfig`; test server đọc nó. Về sau test EditMode của Unity (AG) đọc **cùng tệp** để chứng minh client/server không lệch luật.

## Sau đó (nếu còn token)
R3 (kịch bản dọn repo, chỉ viết) → P5 (danh sách thay asset theo ưu tiên; nguồn gốc là `D:\HITME GAME\ASSET HITME FINAL`) → P6 → P7 (kiểm định bộ asset gốc, trong `hit-me-weapon-sprite-fix.md`). **X3** khi AG báo build sạch; **X2** khi AG báo F1–F4.

## Báo cáo
```
VIỆC: Y1 | S3 | S4 | P4 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N")
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Dành cho AG (không chặn việc đang làm)
Codex đã nâng `tools/patches/gesture_v2` (replay có `expected`, ca ngón thứ hai, khóa input; `prep/codex` @ `1987fbc` trở đi). Khi có lúc, **kéo bản replay/test này về** để Test16 của Unity so sánh trạng thái mong đợi thật (D0 mục 4 của `hit-me-round8-ag-D-codex-review.md`). Không làm gián đoạn F1–F4 và build sạch.

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả `dotnet test`.
