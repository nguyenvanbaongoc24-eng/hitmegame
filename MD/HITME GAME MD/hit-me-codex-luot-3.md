# HIT ME – LƯỢT 3 CHO CODEX (C5b → C11)

Đầu vào: nhánh `prep/codex`, commit `37afaf4`, server suite 123/123. C4 đạt, C7 đạt (cục bộ), C5 và C6 còn dở.

## Cần bạn xử lý trước khi dán prompt

1. **Sửa `MD/DECISIONS.md` ở thư mục chính: `maxRounds=20` → `5`.** Con số 20 xuất phát từ gợi ý của mình ở file đầu tiên (mục 0), không phải luật đã duyệt. Việc này sửa sau khi Antigravity xong lượt đang chạy, tránh xung đột tệp.
2. **Duyệt mức thưởng vàng** (Codex đang chờ để nối vào trận thật). Đề xuất để bạn chọn hoặc sửa số:

| Trận | Hạng 1 | Hạng 2 | Hạng 3 | Hạng 4+ |
|---|---|---|---|---|
| 2 người | 100 | 0 | – | – |
| 3–4 người | 150 | 80 | 40 (chỉ khi 4 người) | 0 |
| 5–6 người | 200 | 120 | 60 | 0 |

   Nếu bạn chưa muốn chốt, trả lời "dùng bảng đề xuất" và Codex sẽ đặt làm hằng số nháp (ghi rõ "chờ chốt").
3. **Tạo project Supabase** và giữ sẵn URL + service key (không dán vào chat; Codex chỉ đọc từ biến môi trường `.env`, đã nằm trong `.gitignore`).
4. **Docker**: không bắt buộc cài. Render tự build image từ Dockerfile trên server của họ, nên C6 chỉ cần kiểm tra file cấu hình đúng chuẩn.

## PROMPT DÁN VÀO CODEX

```
Tiếp tục nhánh prep/codex từ commit 37afaf4. Luật ưu tiên: MD/01_GAMEPLAY_RULES.md và luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6 người, không bot trong phòng online). Không sửa Core, không sửa Unity. Không sửa MD/DECISIONS.md ở thư mục chính (Antigravity đang dùng); nếu cần ghi quyết định, tạo MD/DECISIONS_CODEX.md.

C8. Lệnh cấm AFK bền vững
- Chuyển lệnh cấm 120 giây từ RAM sang IProfileStore (lưu thời điểm hết cấm). Server khởi động lại vẫn nhớ. Test: cấm → "khởi động lại" store → vẫn bị từ chối; hết hạn thì vào được.

C5b. Adapter Supabase
- Cài SupabaseProfileStore (REST) theo schema đã có. Đọc SUPABASE_URL và SUPABASE_SERVICE_KEY từ biến môi trường; thiếu biến thì test Supabase tự bỏ qua (Skip) với thông báo rõ, KHÔNG làm hỏng suite.
- Viết bộ contract test dùng chung chạy trên cả InMemory và Supabase (hồ sơ, vàng, rank, cấm, idempotent theo matchId).
- Không commit khóa. Ghi cách chạy test Supabase thật vào server/README.md.

C9. Cộng vàng khi trận kết thúc (thật, trong luồng trận)
- Nối vào kết thúc trận: tính hạng, cộng vàng cho top-3 theo bảng thưởng (hằng số nháp RewardTable, ghi "chờ chốt" nếu bảng chưa được duyệt), cập nhật rank, idempotent theo matchId. Rời trận/AFK bị loại không nhận thưởng. Test đủ các cỡ trận 2–6 người.

C10. Đặc tả giao thức mạng cho client (quan trọng cho Phase 3)
- Viết MD/NET_PROTOCOL.md: kết nối + token, mọi loại message client↔server (JSON hoặc định dạng đang dùng), ví dụ thật cho từng loại, các mã lỗi (phòng đầy, người bị cấm, session_replaced, hết ân hạn...), trình tự một trận từ tạo phòng đến kết quả, và quy tắc đồng hồ 5s.
- Viết thêm client giả dòng lệnh (Tools/FakeClient) kết nối WebSocket thật, mô phỏng N người (2–6) chơi trọn một trận. Kiểm thử cuối: 3 trận liên tiếp đúng kết quả + vàng.

C6b. Rà cấu hình Render
- Kiểm tra Dockerfile/render.yaml đúng cú pháp bằng công cụ tĩnh nếu có; không cần Docker cục bộ. Bổ sung MD/DEPLOY_RENDER.md: từng bước người dùng bấm, biến môi trường cần đặt, cách xác nhận /healthz, và cách trỏ client Vercel tới địa chỉ WSS.

C11. Báo cáo tải chậm (chỉ đọc, không sửa Unity)
- Dev 17,7 giây và Release Brotli 10,0 giây đến HOME_SHOWN là chậm cho mobile. Phân tích thư mục build: file nặng nhất, kích thước nén, thời gian tải/giải nén/khởi tạo từng phần. Lập danh sách đề xuất xếp theo hiệu quả/độ khó (nén texture, cắt code, tách asset, tải trễ), ghi vào Evidence/round39/C11_load_analysis.md để giao Antigravity sau.

QUY TẮC: chạy toàn bộ suite tối đa 1 lần mỗi nhiệm vụ; không đọc log dài (60 dòng cuối); commit riêng từng nhiệm vụ; cập nhật MD/STATUS.md; không ghi "xong" khi chưa có test chạy thật. Hết quota: commit phần xanh, ghi phần dở.

BÁO CÁO CUỐI (tối đa 12 dòng): C8, C5b, C9, C10, C6b, C11 xong/dở/chặn, số test, commit hash.
```

## Điều kiện để sang Phase 3

| Hạng mục | Trạng thái |
|---|---|
| Server: phòng, trận, AFK, đo tải | Xong |
| Server: vàng thật + Supabase + giao thức (C5b, C9, C10) | Lượt này |
| Client Unity (A1–A5) | Antigravity đang làm, cần bạn báo kết quả |
| Nối client–server (A3 thật, A6, A7) | Làm ngay khi C10 + A1–A5 xong |

Khi Antigravity xong lượt hiện tại, gửi mình báo cáo của nó để mình giao tiếp A3 (nối server thật bằng NET_PROTOCOL.md), A6, A7 và các việc từ C11.
