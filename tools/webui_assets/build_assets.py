"""README: python tools/webui_assets/build_assets.py [--catalog web-ui/catalog.json]

Read only Unity menu portraits/heads and weapon icons named by catalog IDs.
Write transparent, resized web copies, a source manifest and a labeled contact
sheet to web-ui/assets. Missing or visually empty sources fail the build.
The catalog is an AG-owned export when available; this tool never edits it.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
UNITY = ROOT.parents[1] / "unity-client" / "Assets" / "Art"
AG_EXPORT = ROOT.parents[1] / "unity-client" / "tools" / "deploy_web" / "catalog"
OUTPUT = ROOT / "web-ui" / "assets"


def validate(image: Image.Image, kind: str, source: Path) -> None:
    rgba = np.asarray(image.convert("RGBA"))
    alpha = rgba[:, :, 3]
    visible = rgba[alpha > 32, :3]
    width, height = image.size
    if not (alpha.min() < 20 and alpha.max() > 200):
        raise ValueError(f"{source}: true transparency required")
    if len(visible) < 100 or len(np.unique((visible // 16).reshape(-1, 3), axis=0)) < 12:
        raise ValueError(f"{source}: blank, white or monochrome image")
    if kind == "portrait" and not (.25 <= width / height <= 1.5):
        raise ValueError(f"{source}: unreasonable portrait ratio {width}/{height}")
    if kind != "portrait" and not (.6 <= width / height <= 1.6):
        raise ValueError(f"{source}: unreasonable icon/head ratio {width}/{height}")


def fit(image: Image.Image, kind: str) -> Image.Image:
    rgba = image.convert("RGBA")
    if kind == "portrait":
        size = (max(1, round(rgba.width * 720 / rgba.height)), 720)
    else:
        size = (128, 128)
    return rgba.resize(size, Image.Resampling.LANCZOS)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--catalog", type=Path, default=ROOT / "web-ui" / "catalog.json")
    parser.add_argument("--unity-art", type=Path, default=UNITY)
    parser.add_argument("--ag-export", type=Path, default=AG_EXPORT,
                        help="Read the Unity-owned catalog image export without changing Unity")
    parser.add_argument("--output", type=Path, default=OUTPUT)
    args = parser.parse_args()
    catalog = json.loads(args.catalog.read_text(encoding="utf-8"))
    sources: list[tuple[str, str, Path]] = []
    for item in catalog["characters"]:
        ident = item["id"]
        if not ident.replace("_", "").isalnum():
            raise ValueError(f"invalid character id {ident!r}")
        for kind in ("portrait", "head"):
            # The current Quậy Boy head file cuts through his face; derive a
            # provisional head from that same character's menu portrait.
            source_kind = "portrait" if ident == "quay_boy" and kind == "head" else kind
            source = args.ag_export / f"{ident}_{kind}.png" if args.ag_export.is_dir() else args.unity_art / "Characters" / ident / "Menu" / f"menu_{ident}_{source_kind}.png"
            sources.append((ident, kind, source))
    for item in catalog["weapons"]:
        ident = item["id"]
        if not ident.replace("_", "").isalnum():
            raise ValueError(f"invalid weapon id {ident!r}")
        # Art/Weapons has mislabeled sandal placeholders for these two icons.
        # Their actual named icon sprites live in Assets/Sprites.
        alternate = {"chao_chong_dinh": "chao_icon.png", "vot_pickleball": "pickleball_icon.png"}
        source = args.ag_export / f"{ident}_icon.png" if args.ag_export.is_dir() else ((args.unity_art.parent / "Sprites" / alternate[ident]) if ident in alternate else (args.unity_art / "Weapons" / f"{ident}_icon.png"))
        sources.append((ident, "icon", source))
    missing = [str(path) for _, _, path in sources if not path.is_file()]
    if missing:
        raise FileNotFoundError("Missing Unity source files:\n" + "\n".join(missing))
    prepared = []
    for ident, kind, source in sources:
        with Image.open(source) as image:
            if ident == "quay_boy" and kind == "head" and not args.ag_export.is_dir():
                image = image.crop((0, 0, image.width, round(image.height * .49)))
            validate(image, kind, source)
            prepared.append((ident, kind, source, fit(image, kind)))
    args.output.mkdir(parents=True, exist_ok=True)
    manifest = {"catalogVersion": catalog["version"], "sample": bool(catalog.get("sample")), "assets": {}}
    for ident, kind, source, image in prepared:
        target = args.output / f"{ident}_{kind}.png"
        image.save(target, optimize=True)
        manifest["assets"][f"{ident}_{kind}"] = {
            "file": target.name, "source": str(source.relative_to(args.ag_export if args.ag_export.is_dir() else args.unity_art.parent)).replace("\\", "/"),
            "sourceSha256": hashlib.sha256(source.read_bytes()).hexdigest(), "size": list(image.size),
        }
        if ident == "quay_boy" and kind == "head":
            manifest["assets"][f"{ident}_{kind}"]["note"] = "Temporary crop from same-character portrait; original head source cuts face"
    font_path = Path("C:/Windows/Fonts/arial.ttf")
    font = ImageFont.truetype(str(font_path), 19) if font_path.exists() else ImageFont.load_default()
    cells = [(item["id"], item["name"]["vi"], "head") for item in catalog["characters"]]
    cells += [(item["id"], item["name"]["vi"], "icon") for item in catalog["weapons"]]
    contact = Image.new("RGB", (4 * 220, ((len(cells) + 3) // 4) * 190), "#2c5c40")
    draw = ImageDraw.Draw(contact)
    for index, (ident, label, kind) in enumerate(cells):
        x, y = (index % 4) * 220, (index // 4) * 190
        cell = Image.new("RGBA", (128, 128), "#d435a8")
        cell.alpha_composite(Image.open(args.output / f"{ident}_{kind}.png").convert("RGBA").resize((128, 128)))
        contact.paste(cell.convert("RGB"), (x + 46, y + 4))
        draw.text((x + 8, y + 139), label, font=font, fill="white")
        draw.text((x + 8, y + 162), ident, font=font, fill="#ffdc67")
    contact.save(args.output / "contact_sheet.png", optimize=True)
    (args.output / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Prepared {len(prepared)} assets from Unity (read-only); sample catalog={manifest['sample']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
