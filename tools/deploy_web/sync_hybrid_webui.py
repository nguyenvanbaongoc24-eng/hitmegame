#!/usr/bin/env python3
"""
sync_hybrid_webui.py
Extracts the latest web-ui/dist from prep/codex (commit 22ff912+),
merges canonical catalog assets, and configures /ui-preview/ and /next/
for the live hybrid Web UI + Unity engine.
"""

import sys
import shutil
import io
import tarfile
import subprocess
from pathlib import Path

REPO_ROOT = Path(r"D:\HITME GAME\unity-client")
DEPLOY_WEB_DIR = REPO_ROOT / "tools" / "deploy_web"
DIST_DIR = DEPLOY_WEB_DIR / "dist"
CATALOG_DIR = DEPLOY_WEB_DIR / "catalog"
CODEX_WORKTREE_DIST = REPO_ROOT.parent / "worktrees" / "codex-prep" / "web-ui" / "dist"

def extract_webui_dist(target_dir: Path):
    print(f"[*] Extracting web-ui/dist into {target_dir}...")
    if CODEX_WORKTREE_DIST.exists() and (CODEX_WORKTREE_DIST / "app.js").exists():
        print(f"[+] Copying directly from codex-prep worktree: {CODEX_WORKTREE_DIST}")
        if target_dir.exists():
            shutil.rmtree(target_dir)
        shutil.copytree(CODEX_WORKTREE_DIST, target_dir)
        return

    # Fallback to git archive from prep/codex
    res = subprocess.run(["git", "archive", "--format=tar", "prep/codex", "web-ui/dist"], cwd=REPO_ROOT, capture_output=True, check=True)
    tar_bytes = res.stdout
    if target_dir.exists():
        shutil.rmtree(target_dir)
    target_dir.mkdir(parents=True, exist_ok=True)
    with tarfile.open(fileobj=io.BytesIO(tar_bytes)) as tf:
        for member in tf.getmembers():
            if not member.name.startswith("web-ui/dist/"):
                continue
            rel_name = member.name[len("web-ui/dist/"):]
            if not rel_name:
                continue
            dest_path = target_dir / rel_name
            if member.isdir():
                dest_path.mkdir(parents=True, exist_ok=True)
            elif member.isfile():
                dest_path.parent.mkdir(parents=True, exist_ok=True)
                with tf.extractfile(member) as src, open(dest_path, "wb") as dst:
                    shutil.copyfileobj(src, dst)
    print(f"[+] Extracted web-ui/dist into {target_dir}")

def sync_catalog(target_dir: Path):
    assets_dir = target_dir / "assets"
    assets_dir.mkdir(parents=True, exist_ok=True)
    
    # Copy catalog.json to target_dir/catalog.json and target_dir/assets/catalog.json
    cat_src = CATALOG_DIR / "catalog.json"
    if cat_src.exists():
        shutil.copy2(cat_src, target_dir / "catalog.json")
        shutil.copy2(cat_src, assets_dir / "catalog.json")
        print(f"[+] Synced catalog.json to {target_dir}")
        
    for f in CATALOG_DIR.iterdir():
        if f.name == "catalog.json":
            continue
        shutil.copy2(f, assets_dir / f.name)
        # Also copy to root of target_dir as fallback
        shutil.copy2(f, target_dir / f.name)
    print(f"[+] Synced all catalog assets into {assets_dir} and {target_dir}")

def patch_next_app_js(app_js_path: Path):
    """
    Enhances next/app.js to:
    1. Display real HUD when screen === 'match' with transparent arena
    2. Display real Result when screen === 'result' with Rematch / Home
    3. Allow pointer-events to pass to Unity canvas for placing and aiming
    """
    text = app_js_path.read_text(encoding="utf-8")

    # Add autostart to onUnity ready
    text = text.replace("if (event.type==='ready') {state.roster=event.roster||[];state.weapons=event.weapons||[];state.version=event.version||'';state.build=event.build||'';state.ready=true;if(state.overlay==='loading')state.overlay=null;}", "if (event.type==='ready') {\n    state.roster=event.roster||[];state.weapons=event.weapons||[];state.version=event.version||'';state.build=event.build||'';state.ready=true;\n    if(state.overlay==='loading')state.overlay=null;\n    if (params.get('autostart')==='practice') {\n      const cId = params.get('char')||state.characterId;\n      const wId = params.get('weapon')||state.weaponId;\n      const bCnt = parseInt(params.get('bots')||'3', 10);\n      setTimeout(() => {\n        send('StartPractice', {characterId: cId, weaponId: wId, botCount: bCnt});\n      }, 300);\n    }\n  }", 1)

    # Set default balance to 500 for gold chip
    text = text.replace("balance:null,", "balance:500,", 1)
    text = text.replace("if (mock) {state.balance=1250;installMockUnity(catalog);} else render();", "state.balance=500; if (mock) {installMockUnity(catalog);} else render();", 1)

    # In render(): handle state.screen for real hybrid mode
    old_render = "root.innerHTML = mock && state.screen !== 'home' ? prototype(state.screen,state,catalog) + modal() : home() + modal();"
    new_render = """root.classList.toggle('in-gameplay', state.screen === 'match');
  if (state.screen === 'match' && !mock) {
    root.innerHTML = hybridMatchScreen(state, catalog) + modal();
  } else if (state.screen === 'result' && !mock) {
    root.innerHTML = hybridResultScreen(state, catalog) + modal();
  } else {
    root.innerHTML = (mock && state.screen !== 'home') ? prototype(state.screen,state,catalog) + modal() : home() + modal();
  }"""
    if old_render in text:
        text = text.replace(old_render, new_render, 1)
        print("[+] Replaced render logic in next/app.js")

    # Handle action === 'start-practice' or 'rematch' or 'go-home' in click listener
    old_practice_check = "if (action==='start-practice') {if(mock){send('StartPractice',{characterId:state.characterId,weaponId:state.weaponId,botCount:state.botCount});startMockClock();return;} }"
    new_practice_check = """if (action==='start-practice') {
    if (mock) { send('StartPractice',{characterId:state.characterId,weaponId:state.weaponId,botCount:state.botCount}); startMockClock(); return; }
    else { send('Rematch', {}); return; }
  }
  if (action==='go-home') {
    if (!mock) { send('ExitMatch', {}); }
    return go('home');
  }"""
    if old_practice_check in text:
        text = text.replace(old_practice_check, new_practice_check, 1)
        print("[+] Updated action handlers in next/app.js")

    # Append helper functions for hybrid match HUD and result screen
    helper_code = """
// ============================================================================
// HYBRID UNITY + WEB UI HUD & RESULT
// ============================================================================
function hybridMatchScreen(s, c) {
  const me = c.characters.find(x => x.id === s.characterId) || c.characters[0];
  const w = c.weapons.find(x => x.id === s.weaponId) || c.weapons[0];
  const m = s.match;
  const roundNum = m ? m.round : 1;
  const maxRounds = (m && m.maxRounds) ? m.maxRounds : 5;
  const phase = m ? (m.phase || 'placing') : 'placing';
  const timeLeft = m ? Math.max(0, Math.ceil(m.timeLeft || 0)) : 5;
  const toast = m && m.toast ? m.toast : '';

  const players = (m && m.players && m.players.length) ? m.players : [
    { id: 'player', name: catalogTitle(me), hp: 3, maxHp: 3, alive: true, isLocal: true, characterId: s.characterId },
    { id: 'bot-1', name: 'Bot 1', hp: 3, maxHp: 3, alive: true, isLocal: false, characterId: 'nu_nang_dong' },
    { id: 'bot-2', name: 'Bot 2', hp: 3, maxHp: 3, alive: true, isLocal: false, characterId: 'co_ba' },
    { id: 'bot-3', name: 'Bot 3', hp: 3, maxHp: 3, alive: true, isLocal: false, characterId: 'anh_teen' }
  ];

  const localP = players.find(p => p.isLocal) || players[0];
  const localHp = Math.max(0, localP.hp || 0);

  return `<main class="stage preview preview-match" data-screen="match">
    <div class="match-top" style="pointer-events:auto;">
      <div class="mock-match-summary">
        <b>VÒNG ${roundNum}/${maxRounds}</b>
        <b data-name="mock-timer">${phase === 'placing' ? `${timeLeft} GIÂY` : phase.toUpperCase()}</b>
        <button data-action="settings" aria-label="Cài đặt">⚙</button>
      </div>
      <div class="mock-player-row">
        ${players.map(p => {
          const charDef = c.characters.find(ch => ch.id === p.characterId) || c.characters[0];
          const headImg = charDef?.assets?.head || './assets/nam_lang_head.png';
          const hpHearts = '♥'.repeat(Math.max(0, p.hp)) + '♡'.repeat(Math.max(0, (p.maxHp || 3) - Math.max(0, p.hp)));
          return `<div class="mock-player-card ${p.alive ? '' : 'eliminated'}">
            <img src="${headImg}" alt="">
            <span>${safeText(p.name)}<br>${p.alive ? hpHearts : 'ĐÃ LOẠI'}</span>
          </div>`;
        }).join('')}
      </div>
    </div>
    <div class="match-arena" style="pointer-events:none;background:transparent;">
      ${toast ? `<div class="mock-event" role="status">${safeText(toast)}</div>` : ''}
    </div>
    <div class="match-bottom" style="pointer-events:auto;">
      <button data-action="weapon">VŨ KHÍ: ${safeText(catalogTitle(w))}</button>
      <button data-action="send-emote" data-id="phrase_01">☺ BIỂU CẢM</button>
    </div>
  </main>`;
}

function hybridResultScreen(s, c) {
  const me = c.characters.find(x => x.id === s.characterId) || c.characters[0];
  const res = s.result;
  const rankings = (res && res.rankings && res.rankings.length) ? res.rankings : [
    { id: 'player', name: catalogTitle(me), rank: 1, hits: res ? res.hits || 3 : 3, survived: true }
  ];

  const localRank = rankings.find(r => r.id === 'player');
  const isWin = localRank && localRank.rank === 1;

  return `<main class="stage preview" data-screen="result">
    <header class="preview-header">
      <button data-action="go-home" aria-label="Quay lại">‹</button>
      <h1>KẾT QUẢ TRẬN</h1>
      <button data-action="settings" aria-label="Cài đặt">⚙</button>
    </header>
    <div class="preview-scroll">
      <h2>${isWin ? '🏆 CHIẾN THẮNG!' : 'TRẬN ĐẤU KẾT THÚC'}</h2>
      <p>Vòng ${res ? res.round || 1 : 1}/5 • Luyện với bot</p>
      <div class="preview-list">
        ${rankings.map(r => `<div>
          <span>${r.rank}. ${safeText(r.name)} (${r.hits || 0} đòn trúng)</span>
          <b>${r.survived ? (r.rank === 1 ? 'HẠNG 1' : 'SỐNG SÓT') : 'ĐÃ LOẠI'}</b>
        </div>`).join('')}
      </div>
    </div>
    <footer class="preview-footer">
      <button class="preview-primary" data-action="go-home">VỀ TRANG CHỦ</button>
    </footer>
    <button class="preview-replay" data-action="start-practice" style="pointer-events:auto;">CHƠI LẠI</button>
  </main>`;
}
"""
    if "hybridMatchScreen" not in text:
        text += helper_code
        print("[+] Appended hybridMatchScreen and hybridResultScreen to next/app.js")

    app_js_path.write_text(text, encoding="utf-8")
    print(f"[+] Successfully patched {app_js_path}")

def main():
    print("=== [1/4] Extracting web-ui/dist to ui-preview ===")
    ui_preview = DIST_DIR / "ui-preview"
    extract_webui_dist(ui_preview)
    sync_catalog(ui_preview)

    print("\n=== [2/4] Setting up next/ directory ===")
    next_dist = DIST_DIR / "next"
    if next_dist.exists():
        # Keep index.html if already generated, clear other assets
        for item in next_dist.iterdir():
            if item.name == "index.html":
                continue
            if item.is_dir():
                shutil.rmtree(item)
            else:
                item.unlink()
    next_dist.mkdir(parents=True, exist_ok=True)

    for item in ui_preview.iterdir():
        if item.name == "index.html":
            continue
        dst = next_dist / item.name
        if item.is_dir():
            shutil.copytree(item, dst)
        else:
            shutil.copy2(item, dst)
    sync_catalog(next_dist)

    print("\n=== [3/4] Patching next/app.js ===")
    patch_next_app_js(next_dist / "app.js")

    print("\n=== [4/4] Generating next/index.html ===")
    gen_script = REPO_ROOT / "tools" / "generate_next_index.py"
    if gen_script.exists():
        subprocess.run([sys.executable, str(gen_script)], check=True)
    print("\n[SUCCESS] Hybrid Web UI bundle fully prepared!")

if __name__ == "__main__":
    main()
