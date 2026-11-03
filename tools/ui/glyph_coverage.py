"""Audit font coverage for UI source strings.

README: python tools/ui/glyph_coverage.py FONT.ttf SOURCE [SOURCE ...] --json report.json
Sources may be .cs, .json, .md files or directories. C#/JSON quoted strings and
Markdown prose are scanned; code fences in Markdown are skipped. A missing glyph
lists every source file and line. Exit 1 when coverage is incomplete. Use
--vietnamese to print/check the complete precomposed Vietnamese alphabet.
Requires fontTools. Source and font files are read-only.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import unicodedata
from collections import defaultdict
from pathlib import Path

from fontTools.ttLib import TTFont


QUOTED = re.compile(r'"(?:\\.|[^"\\])*"')
TONES = ("", "\u0300", "\u0301", "\u0309", "\u0303", "\u0323")


def vietnamese_characters() -> str:
    letters = "aăâeêioôơuưyAĂÂEÊIOÔƠUƯY"
    return "".join(dict.fromkeys(unicodedata.normalize("NFC", base + tone)
                                 for base in letters for tone in TONES)) + "đĐ"


def source_lines(path: Path):
    fenced = False
    for number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        if path.suffix.lower() == ".md":
            if line.lstrip().startswith(("```", "~~~")):
                fenced = not fenced
                continue
            if not fenced:
                yield number, line
            continue
        for token in QUOTED.finditer(line):
            if path.suffix.lower() == ".json" and line[token.end():].lstrip().startswith(":"):
                continue  # property name, not displayed copy
            try:
                value = json.loads(token.group())
            except json.JSONDecodeError:
                value = token.group()[1:-1]
            yield number, value


def load_cmap(font: Path) -> set[int]:
    with TTFont(font, lazy=True) as loaded:
        return {code for table in loaded["cmap"].tables if table.isUnicode()
                for code in table.cmap}


def audit(font: Path, sources: list[Path], full_vietnamese: bool = False) -> dict:
    cmap = load_cmap(font)
    locations: dict[str, set[str]] = defaultdict(set)
    paths = sorted({file for source in sources for file in
                    (source.rglob("*") if source.is_dir() else (source,))
                    if file.is_file() and file.suffix.lower() in {".cs", ".json", ".md"}})
    for path in paths:
        for number, value in source_lines(path):
            for character in value:
                if character.isprintable() and not character.isspace():
                    locations[character].add(f"{path}:{number}")
    if full_vietnamese:
        for character in vietnamese_characters():
            locations[character].add("<Vietnamese alphabet>")
    missing = {character: sorted(refs) for character, refs in sorted(locations.items(), key=lambda p: ord(p[0]))
               if ord(character) not in cmap}
    return {"font": str(font), "files": len(paths), "characters": len(locations),
            "missing": missing, "missingCount": len(missing)}


def main() -> int:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("font", type=Path)
    parser.add_argument("sources", type=Path, nargs="*")
    parser.add_argument("--json", type=Path)
    parser.add_argument("--vietnamese", action="store_true")
    args = parser.parse_args()
    if not args.sources and not args.vietnamese:
        parser.error("Provide a source or --vietnamese")
    report = audit(args.font, args.sources, args.vietnamese)
    if args.vietnamese:
        print("Vietnamese character set:", vietnamese_characters())
    for character, refs in report["missing"].items():
        print(f"U+{ord(character):04X} {character}: {', '.join(refs)}")
    print(f"Checked {report['files']} files and {report['characters']} characters; missing {report['missingCount']}")
    if args.json:
        args.json.parent.mkdir(parents=True, exist_ok=True)
        args.json.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return 1 if report["missingCount"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
