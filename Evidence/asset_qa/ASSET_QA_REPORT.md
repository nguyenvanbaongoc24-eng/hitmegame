# Asset QA Report

Nguồn chỉ đọc: ASSET HITME FINAL, HITME_UI_ARENA_PACKAGE, SFX.
Đã quét 740 ảnh và 20 file âm thanh. Không ghi đè nguồn.

## Top 20 cần xử lý

|File|Lý do|
|---|---|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_001.png`|checker=high; dark=39.15%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_006.png`|checker=high; dark=38.52%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_003.png`|checker=high; dark=34.92%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_002.png`|checker=high; dark=33.75%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_012.png`|checker=high; dark=32.90%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_004.png`|checker=high; dark=32.06%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_024.png`|checker=high; dark=28.01%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_023.png`|checker=high; dark=28.84%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_025.png`|checker=high; dark=24.00%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_005.png`|checker=high; dark=22.45%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_017.png`|checker=high; dark=18.58%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_019.png`|checker=high; dark=15.78%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_022.png`|checker=high; dark=12.47%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_008.png`|checker=high; dark=12.52%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_016.png`|checker=high; dark=11.67%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_009.png`|checker=high; dark=11.70%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_021.png`|checker=high; dark=9.71%|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_010.png`|checker=high|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_018.png`|checker=high|
|`ASSET HITME FINAL\ASSET WEAPONS\CHẢO\sprites\sprite_015.png`|checker=high|

## SFX theo sự kiện

|Sự kiện|Trạng thái|
|---|---|
|button_click|có|
|place|có|
|last_3_seconds|có|
|reveal|có|
|throw|có|
|flying_sandal|THIẾU|
|hit|có|
|death|có|
|win|có|
|lose|có|
|match_bgm_loop|có|

## Khuyến nghị

1. Xuất lại các sprite checkerboard mức cao bằng alpha thật; diagnostic nằm trong `flagged/`.
2. Ưu tiên xử lý viền sáng/đen trước khi import Unity; chỉ dùng `cleaned/` như bản thử, không thay file nguồn.
3. Có 37 nhóm ảnh trùng hash; xem CSV để quyết định giữ bản chuẩn.
