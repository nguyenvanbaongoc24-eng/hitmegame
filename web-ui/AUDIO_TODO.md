# Âm thanh còn tạm thời

Máy WebAudio tổng hợp những id chưa có tệp MP3: `ui_confirm`, `ui_back`, `ui_denied`, `ui_toggle`, `ui_slider_tick`, `ui_tab`, `ui_sheet_open`, `ui_sheet_close`, `ui_toast`, `ui_select_char`, `ui_select_weapon`, `ui_copy`, `room_join`, `room_leave`, `round_start`, `aim_tick`, `throw_chao`, `throw_vot`, `hit_chao`, `hit_vot`, `round_result`, `emo_pop`, `jingle_win`, `jingle_lose`.

Các id có MP3 vẫn dùng âm tổng hợp nếu tệp thiếu, giải mã thất bại hoặc chưa tải xong. Bộ WAV nguồn trong `SFX/` chỉ đọc. Nhạc và SFX bản cuối cần chủ dự án nghe thử và xác nhận quyền sử dụng thương mại.

Chưa xác minh âm thanh trên iPhone Safari thật. WebKit headless trên máy này không cung cấp `AudioContext`; bài kiểm âm thanh tự động dùng Chromium và ghi rõ giới hạn này.
