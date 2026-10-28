"""Run with: python tools/asset_qa/asset_qa.py

Read-only asset quality audit for HIT ME. Source roots are discovered beside the
repository (ASSET*, HITME_UI_ARENA*, SpriteSources and SFX); generated CSV,
report, and diagnostic composites are written only under Evidence/asset_qa.
Requires Pillow and numpy. It never modifies, renames, or deletes source files.
"""
from __future__ import annotations

import csv
import hashlib
import math
import re
import sys
import wave
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

HERE = Path(__file__).resolve()
REPO = HERE.parents[2]
OUT = REPO / "Evidence" / "asset_qa"
FLAGGED = OUT / "flagged"
IMAGE_EXT = {".png", ".jpg", ".jpeg", ".webp"}
AUDIO_EXT = {".wav", ".mp3", ".ogg", ".flac", ".m4a"}
EVENTS = {
    "button_click": ("click", "button", "tap"),
    "place": ("place", "position"),
    "last_3_seconds": ("countdown", "tick"),
    "reveal": ("reveal",),
    "throw": ("throw",),
    "flying_sandal": ("sandal", "slipper", "dep", "fly"),
    "hit": ("hit", "bop", "impact"),
    "death": ("death", "eliminate", "heart_break"),
    "win": ("win",), "lose": ("lose",),
    "match_bgm_loop": ("bgm", "match", "loop"),
}


def source_roots() -> list[Path]:
    """Find only declared source directories, avoiding the worktree outputs."""
    parent = REPO.parents[1]  # D:/HITME GAME
    roots: list[Path] = []
    for candidate in parent.iterdir():
        name = candidate.name.lower()
        if candidate.is_dir() and (name.startswith("asset ") or name == "asset" or name.startswith("hitme_ui_arena") or name == "sfx" or "spritesources" in name):
            roots.append(candidate)
    return sorted(roots, key=lambda p: str(p).lower())


def rel(path: Path) -> str:
    try:
        return str(path.relative_to(REPO.parents[1]))
    except ValueError:
        return str(path)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def is_power_two(n: int) -> bool:
    return n > 0 and not (n & (n - 1))


def edge_mask(height: int, width: int, inset: int = 32) -> np.ndarray:
    mask = np.zeros((height, width), dtype=bool)
    inset = min(inset, height // 2, width // 2)
    mask[:inset, :] = True; mask[-inset:, :] = True
    mask[:, :inset] = True; mask[:, -inset:] = True
    return mask


def checkerboard_score(rgb: np.ndarray, alpha: np.ndarray) -> tuple[str, str]:
    """Detect opaque, alternating neutral tiles at image edges without claiming certainty."""
    h, w, _ = rgb.shape
    if h < 16 or w < 16:
        return "none", ""
    opaque = alpha == 255
    neutral = (rgb.max(axis=2) - rgb.min(axis=2)) < 22
    edge = edge_mask(h, w)
    candidate = opaque & neutral & edge
    if candidate.mean() < 0.04:
        return "none", ""
    # Compare offset pixels for several plausible checker tile half-periods.
    best = (0.0, 0, 0.0)
    gray = rgb.mean(axis=2)
    for tile in (8, 12, 16, 24, 32):
        pairs = candidate[:, :-tile] & candidate[:, tile:]
        if pairs.sum() < 100:
            continue
        delta = np.abs(gray[:, :-tile] - gray[:, tile:])
        alternating = (delta[pairs] > 18).mean() if pairs.any() else 0.0
        coverage = pairs.mean()
        score = alternating * min(1.0, coverage * 8)
        if score > best[0]:
            best = (score, tile, coverage)
    score, tile, coverage = best
    if score >= .38: level = "high"
    elif score >= .18: level = "medium"
    elif score >= .08: level = "low"
    else: return "none", ""
    return level, f"edge opaque neutral alternation, tile≈{tile}px, score={score:.2f}, coverage={coverage:.1%}"


def edge_artifacts(rgb: np.ndarray, alpha: np.ndarray) -> tuple[float, float, float]:
    semi = (alpha > 0) & (alpha < 255)
    if not semi.any():
        return 0.0, 0.0, 0.0
    pixels = rgb[semi]
    bright = ((pixels.min(axis=1) > 220)).mean() * 100
    dark = ((pixels.max(axis=1) < 35)).mean() * 100
    # Alpha transitions alternating strongly inside a 3-pixel ring approximate jaggies.
    transition = np.abs(np.diff(alpha.astype(np.int16), axis=1)) > 90
    jaggy = transition.mean() * 100
    return float(bright), float(dark), float(jaggy)


def frame_audit(rgb: np.ndarray, alpha: np.ndarray, filename: str) -> str:
    if not re.search(r"sheet|sprite|turnaround", filename, re.I):
        return ""
    h, w = alpha.shape
    messages = []
    if w % 256 or h % 256:
        messages.append("not divisible by 256")
        return "; ".join(messages)
    bottoms, heights, empty = [], [], 0
    for y in range(0, h, 256):
        for x in range(0, w, 256):
            frame = alpha[y:y + 256, x:x + 256] > 8
            ys, xs = np.where(frame)
            if not len(ys): empty += 1; continue
            bottoms.append(int(ys.max())); heights.append(int(ys.max() - ys.min() + 1))
            if xs.min() == 0 or xs.max() == 255 or ys.min() == 0 or ys.max() == 255:
                messages.append("frame touches boundary")
    if empty: messages.append(f"empty={empty}")
    if heights and max(heights) - min(heights) > 12: messages.append(f"height variance={max(heights)-min(heights)}px")
    if bottoms and max(bottoms) - min(bottoms) > 5: messages.append(f"pivot-bottom variance={max(bottoms)-min(bottoms)}px")
    return "; ".join(sorted(set(messages))) or "256-grid OK"


def nine_slice_note(path: Path, alpha: np.ndarray) -> str:
    if not re.search(r"ui|btn|button|panel|card|frame|slot", path.name, re.I):
        return ""
    h, w = alpha.shape
    opaque_border = np.concatenate((alpha[:min(8,h), :].ravel(), alpha[-min(8,h):, :].ravel(), alpha[:, :min(8,w)].ravel(), alpha[:, -min(8,w):].ravel()))
    return "9-slice candidate: " + ("border ≥8px and alpha present" if (opaque_border > 8).mean() > .15 else "border/alpha may be too thin")


def diagnostic(path: Path, rgb: np.ndarray, alpha: np.ndarray, suffix: str) -> None:
    """Create one source crop composited on green/magenta; output only under FLAGGED."""
    FLAGGED.mkdir(parents=True, exist_ok=True)
    h, w = alpha.shape
    pad = max(1, min(h, w) // 8)
    crop = Image.fromarray(np.dstack((rgb, alpha)).astype(np.uint8)).crop((0, 0, min(w, max(96, pad * 2)), min(h, max(96, pad * 2))))
    tile = Image.new("RGBA", (crop.width * 2 + 12, crop.height + 34), (35, 35, 35, 255))
    for index, color in enumerate(((0, 255, 0, 255), (255, 0, 255, 255))):
        bg = Image.new("RGBA", crop.size, color); bg.alpha_composite(crop)
        tile.alpha_composite(bg, (index * (crop.width + 12), 34))
    draw = ImageDraw.Draw(tile); draw.text((2, 4), f"{path.name} [{suffix}] green | magenta", fill=(255,255,255,255))
    safe = re.sub(r"[^A-Za-z0-9._-]+", "_", path.stem)[:100]
    tile.convert("RGB").save(FLAGGED / f"{safe}_{suffix}.png")


def scan_image(path: Path) -> dict[str, str]:
    data = {"path": rel(path), "kind": "image", "size_bytes": str(path.stat().st_size)}
    try:
        with Image.open(path) as original:
            image = original.convert("RGBA")
            arr = np.asarray(image)
            rgb, alpha = arr[:, :, :3], arr[:, :, 3]
            h, w = alpha.shape
            checker, checker_note = checkerboard_score(rgb, alpha)
            bright, dark, jaggy = edge_artifacts(rgb, alpha)
            has_alpha = "yes" if "A" in original.getbands() or "transparency" in original.info else "no"
            data.update({
                "width": str(w), "height": str(h), "mode": original.mode, "has_alpha": has_alpha,
                "transparent_pct": f"{(alpha == 0).mean()*100:.2f}", "semi_transparent_pct": f"{((alpha>0)&(alpha<255)).mean()*100:.2f}",
                "power_of_two": "yes" if is_power_two(w) and is_power_two(h) else "no", "sha256": sha256(path),
                "checkerboard": checker, "checkerboard_note": checker_note,
                "bright_fringe_pct": f"{bright:.2f}", "dark_fringe_pct": f"{dark:.2f}", "jaggy_score": f"{jaggy:.3f}",
                "spritesheet": frame_audit(rgb, alpha, path.name), "nine_slice": nine_slice_note(path, alpha), "name_warning": name_warning(path.name),
            })
            if checker != "none": diagnostic(path, rgb, alpha, "checker")
            if bright >= 8 or dark >= 8 or jaggy >= 1.5: diagnostic(path, rgb, alpha, "edge")
    except Exception as exc:
        data.update({"error": type(exc).__name__ + ": " + str(exc), "name_warning": name_warning(path.name)})
    return data


def name_warning(name: str) -> str:
    warnings = []
    if re.search(r"\s", name): warnings.append("space")
    if re.search(r"[^A-Za-z0-9._ -]", name): warnings.append("non-ascii/special")
    if re.search(r"[A-Z]", name): warnings.append("uppercase")
    return ";".join(warnings)


def approximate_noise(samples: np.ndarray) -> float:
    if len(samples) < 4: return 0.0
    diff = np.diff(samples.astype(np.float64), axis=0)
    return float(np.sqrt(np.mean(diff * diff)))


def scan_audio(path: Path) -> dict[str, str]:
    row = {"path": rel(path), "format": path.suffix.lstrip(".").lower(), "size_bytes": str(path.stat().st_size), "name_warning": name_warning(path.name)}
    if path.suffix.lower() != ".wav":
        row["note"] = "metadata not decoded (stdlib WAV-only audit)"; return row
    try:
        with wave.open(str(path), "rb") as source:
            rate, channels, width, frames = source.getframerate(), source.getnchannels(), source.getsampwidth(), source.getnframes()
            raw = source.readframes(frames)
        dtype = {1: np.uint8, 2: np.int16, 4: np.int32}.get(width)
        if dtype is None: raise ValueError(f"unsupported sample width {width}")
        samples = np.frombuffer(raw, dtype=dtype).astype(np.float64)
        if width == 1: samples = (samples - 128) / 128
        else: samples /= float(2 ** (8 * width - 1))
        peak = float(np.abs(samples).max()) if samples.size else 0
        row.update({"duration_s": f"{frames/rate:.3f}", "sample_rate": str(rate), "channels": str(channels), "peak": f"{peak:.4f}", "clipping": "yes" if peak >= .999 else "no", "noise_estimate": f"{approximate_noise(samples[::max(1,channels)]):.4f}"})
        if frames/rate > 30 or path.stat().st_size > 8 * 1024 * 1024: row["note"] = "long/large"
    except Exception as exc: row["note"] = type(exc).__name__ + ": " + str(exc)
    return row


def write_csv(path: Path, rows: list[dict[str, str]]) -> None:
    keys = sorted({key for row in rows for key in row}) or ["path"]
    with path.open("w", newline="", encoding="utf-8") as output:
        writer = csv.DictWriter(output, fieldnames=keys); writer.writeheader(); writer.writerows(rows)


def report(images: list[dict[str, str]], audio: list[dict[str, str]], roots: list[Path]) -> None:
    flagged = [row for row in images if row.get("checkerboard") in {"high", "medium"} or float(row.get("bright_fringe_pct", "0") or 0) >= 8 or float(row.get("dark_fringe_pct", "0") or 0) >= 8]
    def severity(row: dict[str, str]) -> tuple[int, str]:
        score = {"high": 100, "medium": 60, "low": 25, "none": 0}.get(row.get("checkerboard", "none"), 0)
        score += int(float(row.get("bright_fringe_pct", "0") or 0) + float(row.get("dark_fringe_pct", "0") or 0))
        if row.get("error"): score += 100
        return score, row["path"]
    names = " ".join(a["path"].lower() for a in audio)
    lines = ["# Asset QA Report", "", f"Nguồn chỉ đọc: {', '.join(rel(root) for root in roots)}.", f"Đã quét {len(images)} ảnh và {len(audio)} file âm thanh. Không ghi đè nguồn.", "", "## Top 20 cần xử lý", "", "|File|Lý do|", "|---|---|"]
    for row in sorted(flagged, key=severity, reverse=True)[:20]:
        why = "; ".join(x for x in [f"checker={row.get('checkerboard')}" if row.get("checkerboard") != "none" else "", f"bright={row.get('bright_fringe_pct')}%" if float(row.get("bright_fringe_pct", "0") or 0) >= 8 else "", f"dark={row.get('dark_fringe_pct')}%" if float(row.get("dark_fringe_pct", "0") or 0) >= 8 else "", row.get("spritesheet", "") if "variance" in row.get("spritesheet", "") else ""] if x)
        lines.append(f"|`{row['path']}`|{why}|" )
    if not flagged: lines.append("|Không có cờ mức cao/trung|Cần kiểm tra thủ công các ảnh low nếu có.|" )
    lines += ["", "## SFX theo sự kiện", "", "|Sự kiện|Trạng thái|", "|---|---|"]
    for event, words in EVENTS.items(): lines.append(f"|{event}|{'có' if any(word in names for word in words) else 'THIẾU'}|")
    duplicates = sum(1 for count in Counter(row.get("sha256") for row in images if row.get("sha256")).values() if count > 1)
    lines += ["", "## Khuyến nghị", "", f"1. Xuất lại các sprite checkerboard mức cao bằng alpha thật; diagnostic nằm trong `flagged/`.", "2. Ưu tiên xử lý viền sáng/đen trước khi import Unity; chỉ dùng `cleaned/` như bản thử, không thay file nguồn.", f"3. Có {duplicates} nhóm ảnh trùng hash; xem CSV để quyết định giữ bản chuẩn."]
    (OUT / "ASSET_QA_REPORT.md").write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> int:
    roots = source_roots()
    if not roots:
        print("No declared source roots found", file=sys.stderr); return 2
    OUT.mkdir(parents=True, exist_ok=True); FLAGGED.mkdir(parents=True, exist_ok=True)
    files = sorted((file for root in roots for file in root.rglob("*") if file.is_file()), key=lambda p: str(p).lower())
    images = [scan_image(path) for path in files if path.suffix.lower() in IMAGE_EXT]
    audio = [scan_audio(path) for path in files if path.suffix.lower() in AUDIO_EXT]
    write_csv(OUT / "asset_qa.csv", images); write_csv(OUT / "sfx_qa.csv", audio); report(images, audio, roots)
    print(f"asset_qa: images={len(images)} audio={len(audio)} output={OUT}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
