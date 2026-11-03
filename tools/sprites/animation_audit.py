"""Independently count source frames and Unity clip bindings; read-only inputs.

README: python tools/sprites/animation_audit.py --source "D:/HITME GAME/ASSET HITME FINAL"
--unity Assets/Art/Animations --out tools/sprites/out/animation_audit.json.
Counts alone do not prove that a clip is visually valid or belongs to a given
character. The source-to-roster map is explicit and should be reviewed by AG.
"""

import argparse
import json
import re
from pathlib import Path


SOURCE_MAP = {
    "Nam Làng": "NAM LÀNG",
    "Nữ Năng Động": "MAI HỒNG",
    "Cô Ba": "ANH NÓN LÁ",
    "Anh Teen": "ANH NÚI",
    "Quậy Boy": None,
    "Chó Vàng": None,
    "Mèo Mun": None,
}
UNITY_MAP = {
    "Nam Làng": "Cau_Be_Lang",
    "Nữ Năng Động": "CÔ_NÀNG_THỂ_THAO",
    "Cô Ba": "ANH_NÓN_LÁ",
    "Anh Teen": "ANH_NÚI",
    "Quậy Boy": "NGÔ_BÁ_KHÍ",
    "Chó Vàng": "CHÚ_CHÓ_VÀNG",
    "Mèo Mun": "MÈO_MUN",
}
REQUIRED = ("Idle", "Aim", "Throw", "Hit", "Elimination", "Win")


def clip_frames(path: Path) -> int:
    text = path.read_text(encoding="utf-8")
    # Only the SpriteRenderer PPtr curve is a sequence; the repeated
    # pptrCurveMapping in serialized Unity YAML is deliberately excluded.
    match = re.search(r"m_PPtrCurves:(.*?)(?:\n  m_SampleRate:|\n  m_FloatCurves:)", text, re.S)
    return len(re.findall(r"^    - time:", match.group(1), re.M)) if match else 0


def audit(source: Path, unity: Path) -> dict:
    result = {}
    for character, folder in SOURCE_MAP.items():
        groups = {}
        if folder and (source / folder).is_dir():
            for directory in sorted((source / folder).iterdir()):
                if not directory.is_dir():
                    continue
                metadata = directory / "metadata.json"
                if metadata.exists():
                    sprites = json.loads(metadata.read_text(encoding="utf-8")).get("sprites", [])
                    missing = [row.get("file") for row in sprites if not (directory / str(row.get("file"))).is_file()]
                    groups[directory.name] = {"metadata_frames": len(sprites), "missing_png": missing}
                else:
                    groups[directory.name] = {"metadata_frames": None, "missing_png": [], "note": "no metadata"}
        clip_dir = unity / UNITY_MAP[character]
        clips = {p.stem.rsplit("_", 1)[-1]: clip_frames(p) for p in sorted(clip_dir.glob("*.anim"))} if clip_dir.is_dir() else {}
        result[character] = {"source_folder": folder, "source_groups": groups,
                             "unity_clips": clips, "missing_clips": [a for a in REQUIRED if a not in clips],
                             "source_status": "mapped" if groups else "no_verified_frame_sequence"}
    return {"source": str(source), "unity": str(unity), "characters": result,
            "limitations": ["Clip count does not establish visual quality or correct character mapping.",
                            "Source groups do not identify direction or a subset of Idle frames."]}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--unity", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    args = parser.parse_args()
    report = audit(args.source, args.unity)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    for name, record in report["characters"].items():
        counts = ", ".join(f"{key}:{value['metadata_frames']}" for key, value in record["source_groups"].items())
        print(f"{name}: source=[{counts}] clips={record['unity_clips']} missing={record['missing_clips']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
