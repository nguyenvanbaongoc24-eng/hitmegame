from pathlib import Path

targets = [
    Path("unity-client/tools/deploy_web/dist/index.html"),
    Path("unity-client/tools/deploy_web/index.html"),
]

for target_file in targets:
    if not target_file.exists():
        continue
    text = target_file.read_text(encoding="utf-8")
    text = text.replace(
        '{ id: "nav_bottom", name: "Bottom Tab Navigation", rect: { x: 0, y: h - 60, width: w, height: 55 }, text: "Tuong / Tui do / Cua hang / Xep hang / Nhiem vu", clickable: true }',
        '{ id: "nav_bottom", name: "Bottom Tab Navigation", rect: { x: 0, y: h - 60, width: w, height: 55 }, text: "Tuong / Tui do / Cua hang / Xep hang / Nhiem vu", clickable: true, ignoreSafeBottom: true }'
    )
    text = text.replace(
        '{ id: "txt_footer_version", name: "Footer Version Info", rect: { x: (w - 200) / 2, y: h - 18, width: 200, height: 16 }, text: "HIT ME v1.2", clickable: false }',
        '{ id: "txt_footer_version", name: "Footer Version Info", rect: { x: (w - 200) / 2, y: h - 18, width: 200, height: 16 }, text: "HIT ME v1.2", clickable: false, ignoreSafeBottom: true }'
    )
    target_file.write_text(text, encoding="utf-8")
    print(f"Patched safe area flags in {target_file}")
