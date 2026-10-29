"""Run with: python tools/asset_qa/build_replace_list.py.

Builds Evidence/asset_qa/REPLACE_LIST.csv from the read-only audit output.
It changes only the generated evidence file, ranking character/foot assets first.
"""
from __future__ import annotations
import csv
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "Evidence" / "asset_qa"
PRIORITY = ("nam_lang", "cau_be", "anh_non", "chu_cho", "cho_vang", "meo_mun", "dep", "shadow", "foot", "ring")

def usage(path: str) -> str:
    p = path.lower()
    if any(key in p for key in ("ui", "btn", "panel", "card", "slider", "toggle")): return "UI"
    if any(key in p for key in ("arena", "env", "background", "court", "sand")): return "sân"
    if any(key in p for key in ("weapon", "dep", "chao", "vot")): return "vũ khí"
    return "nhân vật/VFX"

def main() -> None:
    rows = list(csv.DictReader((OUT / "asset_qa.csv").open(encoding="utf-8")))
    result = []
    for row in rows:
        defects = []
        checker = row.get("checkerboard", "none")
        if checker in ("high", "medium", "low"): defects.append("checkerboard")
        if float(row.get("bright_fringe_pct") or 0) >= 8: defects.append("viền sáng")
        if float(row.get("dark_fringe_pct") or 0) >= 8: defects.append("viền đen")
        if "variance" in row.get("spritesheet", "") or "touches boundary" in row.get("spritesheet", ""): defects.append("khung lệch")
        if not defects: continue
        path = row["path"]; stem = Path(path).stem.replace(" ", "_")
        cleaned = OUT / "cleaned" / f"{stem}_cleaned.png"
        severity = {"high": 3, "medium": 2, "low": 1, "none": 0}[checker]
        severity = max(severity, 2 if len(defects) >= 2 else 1)
        priority = 0 if any(k in path.lower() for k in PRIORITY) else 1
        result.append((priority, -severity, {"file_goc": path, "loi": "; ".join(defects), "muc_do": ("cao" if severity == 3 else "trung" if severity == 2 else "thấp"), "ban_da_lam_sach": str(cleaned.relative_to(REPO)) if cleaned.exists() else "", "dung_cho": usage(path)}))
    result.sort(key=lambda item: (item[0], item[1], item[2]["file_goc"].lower()))
    with (OUT / "REPLACE_LIST.csv").open("w", newline="", encoding="utf-8") as file:
        writer = csv.DictWriter(file, fieldnames=("file_goc", "loi", "muc_do", "ban_da_lam_sach", "dung_cho")); writer.writeheader(); writer.writerows(row for _, _, row in result)
    print(f"replace_list: rows={len(result)} output={OUT / 'REPLACE_LIST.csv'}")

if __name__ == "__main__": main()
