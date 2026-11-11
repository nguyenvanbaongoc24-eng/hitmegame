/* Real Unity event renderer. This module never simulates a match or draws on the arena. */
const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const lang = () => document.documentElement.lang === 'en' ? 'en' : 'vi';
const phrase = (vi, en) => lang() === 'en' ? en : vi;
const title = entry => entry?.name?.[lang()] || entry?.name?.vi || entry?.name || '';
const head = (catalog, id) => catalog.characters.find(x => x.id === id)?.assets?.head || './assets/nam_lang_head.png';
const phaseText = phase => ({placing:phrase('CHỌN HƯỚNG NÉM','AIM YOUR THROW'),reveal:phrase('LỘ DIỆN','REVEAL'),throw:phrase('ĐANG NÉM','THROWING'),throwing:phrase('ĐANG NÉM','THROWING'),resolve:phrase('KẾT QUẢ VÒNG','ROUND RESULT'),roundend:phrase('KẾT QUẢ VÒNG','ROUND RESULT'),matchend:phrase('HẾT TRẬN','MATCH END')})[phase] || (phase && phase !== '…' ? escape(phase) : phrase('CHỌN HƯỚNG NÉM','AIM YOUR THROW'));
const heartFullSvg = '<svg viewBox="0 0 24 24" width="13" height="13" fill="#ef4444" aria-hidden="true" style="display:inline-block;vertical-align:-1px;"><path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/></svg>';
const heartEmptySvg = '<svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="#64748b" stroke-width="2" aria-hidden="true" style="display:inline-block;vertical-align:-1px;"><path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/></svg>';
const hearts = (hp, max) => `<span class="live-hearts" aria-label="${hp}/${max} HP">${heartFullSvg.repeat(Math.max(0, Math.min(6, hp || 0)))}${heartEmptySvg.repeat(Math.max(0, Math.min(6, (max || 3) - (hp || 0))))}</span>`;
const smile = '<svg viewBox="0 0 24 24" width="26" height="26" fill="none" stroke="currentColor" stroke-width="2.2" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M8 14c2 3 6 3 8 0"/><path d="M8.5 9h.01M15.5 9h.01" stroke-linecap="round" stroke-width="3"/></svg>';

export function validateLiveEvent(event) {
  if (!new Set(['ready','loading','screen','selection','profile','balance','lobby','match','result','layout','error','event','emote','sfx','music']).has(event.type)) return 'unknown_type';
  if (event.type === 'match' && (!Array.isArray(event.players) || !Number.isFinite(Number(event.round)) || typeof event.phase !== 'string')) return 'match_shape';
  if (event.type === 'result' && !Array.isArray(event.rankings)) return 'result_shape';
  if (event.type === 'screen' && typeof event.name !== 'string') return 'screen_shape';
  if (event.type === 'layout' && (!event.playableRect || !Number.isFinite(Number(event.playableRect.y)))) return 'layout_shape';
  if (event.type === 'event' && typeof event.kind !== 'string') return 'event_shape';
  return null;
}

export function renderLiveMatch(state, catalog) {
  const match = state.match || {};
  let players = Array.isArray(match.players) && match.players.length ? match.players : [];
  if (!players.length) {
    const bCount = Math.max(1, Math.min(5, state.botCount || 3));
    const botPool = ['nu_nang_dong', 'co_ba', 'anh_teen', 'quay_boy', 'cho_vang', 'meo_mun'].filter(id => id !== state.characterId);
    players = [
      { id: 'player', name: phrase('BẠN', 'YOU'), characterId: state.characterId, slot: 0, hp: 3, maxHp: 3, alive: true, isLocal: true },
      ...Array.from({ length: bCount }, (_, i) => ({
        id: `bot-${i+1}`,
        name: `Bot ${i+1}`,
        characterId: botPool[i % botPool.length],
        slot: i+1,
        hp: 3,
        maxHp: 3,
        alive: true,
        isLocal: false
      }))
    ];
  }
  const shownPlayers = match.tiebreak ? players.filter(player=>player.alive!==false) : players;
  const phase = String(match.phase || 'placing').toLowerCase();
  const top = Math.max(84, Math.min(195, Number(state.layout?.playableRect?.y) || 132));
  const bottom = Math.max(100, Math.min(195, (window.visualViewport?.height || innerHeight) - ((Number(state.layout?.playableRect?.y) || 132) + (Number(state.layout?.playableRect?.h) || 400))));
  const cards = shownPlayers.map(player => {
    const local = player.isLocal || player.id === 'player';
    const character = catalog.characters.find(x => x.id === player.characterId);
    const bubble = state.emoteBubbles?.[player.id];
    const rawName = local ? phrase('BẠN', 'YOU') : (player.name || title(character) || `BOT ${Number(player.slot || 0)}`);
    const shortName = rawName.length > 8 ? rawName.slice(0, 7) + '…' : rawName;
    return `<div class="live-card ${local?'local':''} ${player.alive===false?'out':''} ${bubble?'emoting':''}" data-player="${escape(player.id)}">
      ${local ? `<span class="live-card-you" aria-label="${phrase('BẠN','YOU')}">${phrase('BẠN','YOU')}</span>` : ''}
      <img src="${escape(head(catalog, player.characterId))}" alt="">
      <div><b>${escape(shortName)}</b>${hearts(Number(player.hp),match.tiebreak?1:(Number(player.maxHp)||3))}</div>
      ${bubble?`<output class="live-card-bubble">${escape(bubble)}</output>`:''}
    </div>`;
  }).join('');
  const seconds = Math.max(0, Number.isFinite(Number(match.timeLeft)) ? Number(match.timeLeft) : 5);
  const toast = state.toasts?.[0];
  const events = state.roundEvents?.length ? `<div class="live-round-events" aria-label="${phrase('Diễn biến vòng','Round events')}">${state.roundEvents.map(item=>`<span>${escape(item)}</span>`).join('')}</div>` : '';
  const tutorial = state.tutorialStep >= 0 ? `<div class="live-tutorial" role="dialog" aria-label="${phrase('Hướng dẫn','Tutorial')}"><b>${phrase('HƯỚNG DẪN','TUTORIAL')} ${state.tutorialStep+1}/3</b><p>${escape((lang()==='en' ? ['Tap the arena to place your character','Hold and drag to aim','After 5 seconds, everyone throws together!'] : ['Chạm vào sân để đặt nhân vật','Giữ và kéo để chọn hướng ném','Hết 5 giây, mọi người ném cùng lúc!'])[state.tutorialStep])}</p><div><button data-action="tutorial-skip">${phrase('BỎ QUA','SKIP')}</button><button data-action="tutorial-next">${phrase('TIẾP','NEXT')}</button></div></div>` : '';
  const defaultBotCount = (state && state.botCount) ? state.botCount : 3;
  const living = players.length > 0 ? players.filter(player=>player.alive!==false).length : defaultBotCount + 1;
  const roundLabel = match.tiebreak ? phrase('HIỆP PHỤ','TIEBREAK') + ' ' + (Number(match.tiebreakCount) || 1) : phrase('VÒNG','ROUND') + ' ' + (Number(match.round) || 1);
  const weapon = catalog.weapons.find(x=>x.id===state.weaponId);
  const gearSvg = '<svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>';
  const activeAlive = Number.isFinite(Number(match.aliveCount))&&Number(match.aliveCount)>0?Number(match.aliveCount):(living>0?living:defaultBotCount+1);
  return `<main class="stage live-match" data-screen="match" style="--live-top:${top}px;--live-bottom:${bottom}px"><header class="live-top"><div class="live-summary"><b>${escape(roundLabel)}</b><strong>${phase==='placing'?`${Math.ceil(seconds)} ${phrase('GIÂY','SEC')}`:''}</strong><button data-action="settings" class="r43-match-settings-btn" aria-label="${phrase('Cài đặt','Settings')}">${gearSvg}</button></div><div class="live-cards">${cards}</div><div class="live-phase">${phaseText(phase)}</div>${events}${toast?`<output class="live-toast">${escape(toast.message)}</output>`:''}</header><div class="live-arena" aria-hidden="true"></div><footer class="live-bottom"><div class="live-progress"><b>${activeAlive} ${phrase('người còn sống','alive')}</b></div><div class="live-controls"><span class="live-weapon-chip">${weapon?.icon?`<img src="${escape(weapon.icon)}" alt="">`:''}${escape(match.weaponName || title(weapon))}</span>${phase==='placing'?`<button class="live-ready" data-action="ready" aria-label="${phrase('Khóa hướng sớm','Lock aim early')}" ${state.readyLocked?'disabled':''}>${state.readyLocked?phrase('ĐÃ KHÓA','LOCKED'):phrase('SẴN SÀNG','READY')}</button>`:''}<button class="live-emote-trigger ${state.emoteCooldownUntil>Date.now()?'cooldown':''}" data-action="emote" aria-label="${phrase('Biểu cảm','Emote')}" ${state.emoteCooldownUntil>Date.now()?'disabled':''}>${smile}</button></div></footer>${tutorial}</main>`;
}

export function updateLiveMatchInPlace(root, match, state, catalog) {
  const matchEl = root ? root.querySelector('.live-match') : null;
  if (!matchEl) return false;

  const phase = String(match.phase || 'placing').toLowerCase();
  const pText = phaseText(phase);
  const phaseEl = matchEl.querySelector('.live-phase');
  if (phaseEl && phaseEl.textContent !== pText) phaseEl.textContent = pText;

  const roundLabel = match.tiebreak ? phrase('HIỆP PHỤ','TIEBREAK') + ' ' + (Number(match.tiebreakCount) || 1) : phrase('VÒNG','ROUND') + ' ' + (Number(match.round) || 1);
  const roundEl = matchEl.querySelector('.live-summary b');
  if (roundEl && roundEl.textContent !== roundLabel) roundEl.textContent = roundLabel;

  const seconds = Math.max(0, Number.isFinite(Number(match.timeLeft)) ? Number(match.timeLeft) : 0);
  const timerEl = matchEl.querySelector('.live-summary strong');
  const tText = phase === 'placing' ? `${Math.ceil(seconds)} ${phrase('GIÂY','SEC')}` : '';
  if (timerEl && timerEl.textContent !== tText) timerEl.textContent = tText;

  const ringEl = matchEl.querySelector('.r38-timer-ring');
  if (ringEl) {
    const ringSpan = ringEl.querySelector('span');
    if (ringSpan) ringSpan.textContent = phase === 'placing' ? `${Math.ceil(seconds)}s` : '—';
    const pct = Math.max(0, Math.min(100, (seconds / 5.0) * 100));
    ringEl.style.setProperty('--timer-pct', `${pct}%`);
  }

  const players = Array.isArray(match.players) ? match.players : [];
  const living = players.filter(p => p.alive !== false).length;
  const defBots = (state && state.botCount) ? state.botCount : 3;
  const aliveNum = Number.isFinite(Number(match.aliveCount)) && Number(match.aliveCount) > 0 ? Number(match.aliveCount) : (living > 0 ? living : defBots + 1);
  const progEl = matchEl.querySelector('.live-progress b');
  const aliveText = `${aliveNum} ${phrase('người còn sống','alive')}`;
  if (progEl && progEl.textContent !== aliveText) progEl.textContent = aliveText;

  const cat = (typeof catalog !== 'undefined' && catalog) || (state && state.catalog) || window.__hitmeCatalog || {};
  const weaponsList = (cat && cat.weapons) || (state && state.weapons) || [];
  const chipEl = matchEl.querySelector('.live-weapon-chip');
  if (chipEl) {
    const curW = weaponsList.find(x => x.id === (state?.weaponId || match.weaponId)) || weaponsList[0];
    const wName = match.weaponName || (curW?.name?.vi || (curW?.id === 'chao_chong_dinh' ? 'Chảo Chống Dính' : curW?.id === 'vot_pickleball' ? 'Vợt Pickleball' : 'Dép Tổ Ong'));
    const wIcon = curW?.icon || (curW?.id === 'chao_chong_dinh' ? './assets/weapon_chao_chong_dinh.png' : curW?.id === 'vot_pickleball' ? './assets/weapon_vot_pickleball.png' : './assets/weapon_dep_to_ong.png');
    chipEl.innerHTML = `${wIcon ? `<img src="${escape(wIcon)}" alt="">` : ''}${escape(wName)}`;
  }

  if ((phase === 'throw' || phase === 'throwing') && !matchEl.querySelector('.live-flying-weapon')) {
    const arena = matchEl.querySelector('.live-arena');
    if (arena) {
      const curW = weaponsList.find(x => x.id === state?.weaponId) || weaponsList[0];
      const flyEl = document.createElement('div');
      flyEl.className = 'live-flying-weapon';
      const flyImg = curW?.id === 'chao_chong_dinh' ? './assets/weapon_chao_full.png'
                   : curW?.id === 'vot_pickleball' ? './assets/weapon_pickleball_full.png'
                   : './assets/weapon_dep_full.png';
      flyEl.innerHTML = `<img src="${flyImg}" alt="" style="width:52px;height:52px;object-fit:contain;filter:drop-shadow(0 4px 8px rgba(0,0,0,0.6));">`;
      flyEl.style.cssText = 'position:absolute;left:50%;bottom:20%;transform:translate(-50%,0);z-index:20;pointer-events:none;animation:liveThrowFly 0.65s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;';
      arena.appendChild(flyEl);
      setTimeout(() => { flyEl.remove(); }, 800);
    }
  }

  const readyBtn = matchEl.querySelector('.live-ready');
  if (readyBtn) {
    if (phase !== 'placing') {
      readyBtn.style.display = 'none';
    } else {
      readyBtn.style.display = '';
      if (state.readyLocked) {
        readyBtn.disabled = true;
        readyBtn.textContent = phrase('ĐÃ KHÓA','LOCKED');
      }
    }
  }

  players.forEach(p => {
    const card = matchEl.querySelector(`.live-card[data-player="${p.id}"]`);
    if (card) {
      if (p.alive === false && !card.classList.contains('out')) card.classList.add('out');
      else if (p.alive !== false && card.classList.contains('out')) card.classList.remove('out');
      const heartsContainer = card.querySelector('.live-hearts');
      if (heartsContainer) {
        heartsContainer.innerHTML = hearts(Number(p.hp), match.tiebreak ? 1 : (Number(p.maxHp) || 3));
      }
    }
  });

  return true;
}

export function renderLiveResult(state, catalog) {
  const result = state.result || {};
  const rankings = Array.isArray(result.rankings) ? result.rankings : [];
  const me = rankings.find(x => x.id === 'player');
  const won = me && me.rank === 1;
  const sharedWinners = Array.isArray(result.winnerIds) && result.winnerIds.length > 1;
  const rows = rankings.map(player => `<div class="live-rank"><b>${escape(player.rank ?? '—')}</b><img src="${escape(head(catalog, player.characterId || (player.id==='player'?state.characterId:'nam_lang')))}" alt=""><span>${escape(player.name || player.id)}</span><small>${phrase('TRÚNG','HITS')} ${Number.isFinite(Number(player.hits))?escape(player.hits):'—'}<br>${phrase('VÒNG SỐNG','ROUNDS')} ${Number.isFinite(Number(player.roundSurvived))?escape(player.roundSurvived):'—'}</small></div>`).join('');
  return `<main class="stage live-result" data-screen="result"><div class="live-result-panel"><h1>${sharedWinners?phrase('ĐỒNG VÔ ĐỊCH','CO-CHAMPIONS'):won?phrase('CHIẾN THẮNG!','VICTORY!'):phrase('KẾT QUẢ TRẬN','MATCH RESULT')}</h1><p>${phrase('Vòng','Round')} ${escape(result.round || state.match?.round || '—')} · ${phrase('Đã chơi','Played')} ${escape(state.sessionMatches || 1)} ${phrase('trận','matches')}</p><div class="live-ranks">${rows || `<p>${phrase('Đang nhận bảng xếp hạng…','Waiting for rankings…')}</p>`}</div><button class="live-primary" data-action="rematch">${phrase('CHƠI LẠI','REMATCH')}</button><button class="live-secondary" data-action="exit-match">${phrase('VỀ TRANG CHỦ','BACK HOME')}</button></div></main>`;
}
