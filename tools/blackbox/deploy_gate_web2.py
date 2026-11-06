"""Gate the web.2 release using measured browser results, never a claimed pass.

Run: python tools/blackbox/deploy_gate_web2.py
The Unity and physical iPhone gates are separate; this verifies web browser emulation.
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EVIDENCE = ROOT / 'Evidence' / 'mobile' / '20261106'


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def main():
    live = read(EVIDENCE / 'next_local_web2_final' / 'results.json')
    home = read(EVIDENCE / 'home_spacing_web2' / 'results.json')
    hud = read(EVIDENCE / 'match_hud_web2' / 'results.json')
    preview = read(ROOT / 'web-ui' / 'evidence' / 'preview_flow' / 'results.json')
    catalog = read(ROOT.parent / 'web-dist-mobile' / 'next' / 'catalog.json')
    checks = {
        'F1-F8-live-six-profiles': live['deploymentSynced'] and len(live['profiles']) == 6
            and all(row['status'] == 'PASS' for row in live['profiles'].values())
            and len(live['profiles']['iphone15_safari']['matches']) == 5,
        'BTN-11-HOME-12-13-six-profiles': len(home) == 6 and all(not row['failures'] for row in home.values()),
        'HUD-12-16-EMO-07-two-profiles': len(hud) == 2
            and all(not row['errors'] and all(row['checks'].values()) for row in hud.values()),
        'W4-preview-WebKit-Chromium': len(preview) == 2
            and all(row['status'] == 'PASS' for row in preview.values()),
        'online-stays-locked': catalog['features']['online'] is False,
    }
    output = {'checks': checks, 'failedIds': [name for name, ok in checks.items() if not ok],
              'scope': 'local browser emulation; physical iPhone and Unity fixes not included'}
    (EVIDENCE / 'web2_gate.json').write_text(json.dumps(output, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    passed = sum(checks.values())
    print(f'Passed! {passed}/{len(checks)}')
    return 0 if passed == len(checks) else 1


if __name__ == '__main__':
    raise SystemExit(main())
