import os
import json
import datetime

BUILDS_DIR = r"D:\HITME GAME\unity-client\Builds"

BUILD_META = {
    "WebGL_20261030_96bbc27": {
        "commit": "96bbc27",
        "ngay": "2026-10-30",
        "lo": "Lo 0",
        "mo_ta": "Baseline Graybox v0.1",
        "cong_don": False,
        "nen": "none",
        "cong_goi_y": 8093
    },
    "WebGL_20261030_64bc6a9": {
        "commit": "64bc6a9",
        "ngay": "2026-10-30",
        "lo": "Lo 1",
        "mo_ta": "In-game HUD & Safe Area",
        "cong_don": True,
        "nen": "none",
        "cong_goi_y": 8094
    },
    "WebGL_20261101_9cdef40": {
        "commit": "9cdef40",
        "ngay": "2026-11-01",
        "lo": "Lo 2",
        "mo_ta": "Sanh cho / Phong / N1 Ghep nhanh",
        "cong_don": True,
        "nen": "none",
        "cong_goi_y": 8095
    },
    "WebGL_20261101_c309cf6": {
        "commit": "c309cf6",
        "ngay": "2026-11-01",
        "lo": "Lo 3",
        "mo_ta": "Ket qua tran / Roi phong / Toast",
        "cong_don": True,
        "nen": "none",
        "cong_goi_y": 8096
    },
    "WebGL_20261101_e726d61": {
        "commit": "e726d61",
        "ngay": "2026-11-01",
        "lo": "Lo 4",
        "mo_ta": "Juice feedback (Hit shake, Rung nut, Dem nguoc)",
        "cong_don": True,
        "nen": "none",
        "cong_goi_y": 8097
    },
    "WebGL_20261101_5318aee": {
        "commit": "5318aee",
        "ngay": "2026-11-01",
        "lo": "Lo 5+6",
        "mo_ta": "Audio synth, Cai dat (VI/EN/Motion), Huong dan 3 buoc",
        "cong_don": True,
        "nen": "none",
        "cong_goi_y": 8098
    },
    "WebGL_20261101_09f48d2": {
        "commit": "09f48d2",
        "ngay": "2026-11-01",
        "lo": "Lo 7+8",
        "mo_ta": "Ho so, Cua hang, Tui do, BXH, Dang nhap Khach",
        "cong_don": True,
        "nen": "none",
        "cong_goi_y": 8099
    },
    "WebGL_20261101_31810c4": {
        "commit": "31810c4",
        "ngay": "2026-11-01",
        "lo": "Lo 9",
        "mo_ta": "Game Feel (Nhip tho 7 nv, nem/trung/guc, dep xoay + bong arc)",
        "cong_don": True,
        "nen": "none",
        "cong_goi_y": 8100
    },
    "WebGL_Release_20261101_31810c4_232534": {
        "commit": "31810c4",
        "ngay": "2026-11-01",
        "lo": "Lo 10",
        "mo_ta": "WebGL Release Brotli profile (High stripping, exception none, 16.28MB)",
        "cong_don": True,
        "nen": "br",
        "cong_goi_y": 8101
    }
}

for folder in os.listdir(BUILDS_DIR):
    full_path = os.path.join(BUILDS_DIR, folder)
    if not os.path.isdir(full_path):
        continue
    index_html = os.path.join(full_path, "index.html")
    mtime = None
    if os.path.exists(index_html):
        mtime = os.path.getmtime(index_html)
    
    meta = BUILD_META.get(folder)
    if not meta:
        # Default fallback metadata
        parts = folder.split("_")
        commit = parts[-1] if len(parts) > 1 else "unknown"
        meta = {
            "commit": commit,
            "ngay": "2026-10-30",
            "lo": "Archive",
            "mo_ta": f"Build archive {folder}",
            "cong_don": False,
            "nen": "br" if "Release" in folder else "none",
            "cong_goi_y": None
        }
    
    info_file = os.path.join(full_path, "BUILD_INFO.json")
    with open(info_file, "w", encoding="utf-8") as f:
        json.dump(meta, f, indent=2, ensure_ascii=False)
    
    # Match timestamp of index.html so ageSpread in phase2_audit is 0
    if mtime:
        os.utime(info_file, (mtime, mtime))
    print(f"Created BUILD_INFO.json in {folder}")

print("Done creating BUILD_INFO.json for all builds.")
