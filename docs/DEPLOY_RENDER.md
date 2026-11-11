# Triển khai HIT ME trên Render

## Origin bắt buộc cho beta

Trong **Render → Service → Environment**, đặt biến sau (không có dấu `/` cuối):

```text
ALLOWED_ORIGINS=https://hitmegame-iota.vercel.app
```

Danh sách nhận nhiều Origin, ngăn bằng dấu phẩy. Máy chủ chuẩn hoá chữ hoa/thường và
dấu `/` cuối trước khi so sánh; scheme và cổng vẫn phải khớp. Không dùng `*`.

`ALLOW_VERCEL_PREVIEW_ORIGINS` mặc định `false`. Chỉ đặt `true` nếu cần mở các URL
preview đúng mẫu `https://hitmegame-*.vercel.app`. `ALLOW_MISSING_ORIGIN` mặc định
`false`; không bật nó cho browser production.

Để `/guest-token` hoạt động, giữ `GUEST_TOKEN_SIGNING_KEY` là secret server-only đã
có hoặc tạo mới trong Render; không gửi giá trị qua chat hay đưa vào repo.

Sau khi lưu biến, Manual Deploy nhánh chứa bản vá. Kiểm tra:

```powershell
node tools/deploy/smoke_deployed.mjs https://hitmegame.onrender.com --origin https://hitmegame-iota.vercel.app
```

`/healthz` phải có `originsConfigured: true`, `allowedOriginsCount` ít nhất 1, cùng
`protocol` và `commit`. Giá trị Origin không được xuất hiện trong `/healthz`.
