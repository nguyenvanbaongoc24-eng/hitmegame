# Triển khai HIT ME trên Render

## Nhánh triển khai và rollback

Render chỉ triển khai từ nhánh ổn định `release/server`. Chỉ cập nhật nhánh này bằng
commit đã có bằng chứng P1. Để phát hành một commit đã duyệt từ `prep/codex`:

```powershell
git push origin prep/codex:release/server
```

Để rollback, trên Render dùng **Manual Deploy → Deploy a specific commit** và chọn
commit `release/server` đã biết tốt, hoặc đẩy ref ổn định về commit đó theo quy trình
review của dự án. Không rollback bằng cách đưa secret vào Git.

## Origin bắt buộc cho beta

Trong **Render → Service → Environment**, đặt biến sau (không có dấu `/` cuối):

```text
ALLOWED_ORIGINS=https://hitmegame-iota.vercel.app
```

Danh sách nhận nhiều Origin, ngăn bằng dấu phẩy. Máy chủ chuẩn hoá chữ hoa/thường và
dấu `/` cuối trước khi so sánh; scheme và cổng vẫn phải khớp. Không dùng `*`.

| Biến | Giá trị/ý nghĩa |
|---|---|
| `ALLOWED_ORIGINS` | Bắt buộc: `https://hitmegame-iota.vercel.app`; danh sách ngăn bằng dấu phẩy. |
| `ALLOW_MISSING_ORIGIN` | Mặc định `false`; chỉ bật cho client không phải trình duyệt được kiểm soát. |
| `ALLOW_VERCEL_PREVIEW_ORIGINS` | Mặc định `false`; chỉ bật nếu cần `https://hitmegame-*.vercel.app`. |
| `GUEST_TOKEN_SIGNING_KEY` | Secret server-only phải tồn tại để `/guest-token` phát token ký. Không ghi hay gửi giá trị. |

Sau khi lưu biến, đặt Render **Build & Deploy → Branch** là `release/server`, rồi
Manual Deploy commit mới nhất. Kiểm tra:

```powershell
node tools/deploy/smoke_deployed.mjs https://hitmegame.onrender.com --origin https://hitmegame-iota.vercel.app
```

`/healthz` phải có `originsConfigured: true`, `allowedOriginsCount` ít nhất 1, cùng
`protocol`, `commit` và `draining: false`. Giá trị Origin không được xuất hiện trong
`/healthz`; endpoint này cố ý có `Access-Control-Allow-Origin: *` để client chẩn đoán
liveness an toàn.
