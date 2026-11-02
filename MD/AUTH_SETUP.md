# HIT ME: chuẩn bị đăng nhập Google qua Supabase

Trạng thái 2026-11-02: đây là hướng dẫn cấu hình, **chưa** chứng minh đăng nhập Google trong game. Server hiện xác thực guest token. Chỉ bật nút đăng nhập sau khi server kiểm Supabase access token, gộp dữ liệu khách có test, và Unity WebGL hoàn tất callback.

1. Trong Supabase Dashboard của project `lpylrbtpruyzdehkcaiu`, mở **Authentication → URL Configuration**. Đặt Site URL là `https://hitmegame-iota.vercel.app`. Thêm redirect URL đúng đường callback mà client sẽ dùng trên tên miền này (ví dụ `https://hitmegame-iota.vercel.app/auth/callback`); chưa có route callback thì cần làm route trước khi thử.
2. Mở **Authentication → Sign In / Providers → Google**. Sao chép **Callback URL** hiển thị tại đây; với project hiện tại dự kiến là `https://lpylrbtpruyzdehkcaiu.supabase.co/auth/v1/callback`. Đối chiếu đúng giá trị Dashboard trước khi lưu tại Google Cloud.
3. Trong Google Cloud Console, cấu hình màn hình đồng ý OAuth và tạo **OAuth Client ID → Web application**. Authorized JavaScript origins: `https://hitmegame-iota.vercel.app`. Authorized redirect URIs: Callback URL của **Supabase ở bước 2**, không phải URL Vercel. Khi thử máy cục bộ, thêm origin localhost theo đúng cổng đang dùng.
4. Dán Client ID và Client Secret **trực tiếp vào trang Google provider của Supabase**, bật provider rồi lưu. Không ghi secret vào repo, file WebGL, log hoặc chat.
5. Client gọi `signInWithOAuth({provider:'google',options:{redirectTo:'https://hitmegame-iota.vercel.app/auth/callback'}})`; route callback trao đổi code lấy session bằng luồng PKCE, sau đó gửi **Supabase access token** tới server. Server phải xác minh token với Supabase Auth trước khi tin user ID. Không gửi Google Client Secret hoặc service key cho client.
6. Kiểm thử sau khi phần tích hợp được merge: Google hợp lệ; token hết hạn/giả mạo bị từ chối; guest đã có vàng/vật phẩm được gộp đúng một lần; đăng nhập trên thiết bị khác thấy cùng số dư; đăng nhập lại không cộng trùng. Ghi bằng chứng test và ảnh/clip WebGL thật trước khi đánh dấu đạt.

Người dùng tự làm bước 1–4 trong hai bảng điều khiển; Codex/AG làm bước 5–6 trong server/client. Facebook/Apple để sau. Tham khảo [Supabase Google sign-in](https://supabase.com/docs/guides/auth/social-sign-in/auth-google) và [Redirect URLs](https://supabase.com/docs/guides/auth/redirect-urls).
