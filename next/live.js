/* Real Unity event renderer. This module never simulates a match or draws on the arena. */
const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const lang = () => document.documentElement.lang === 'en' ? 'en' : 'vi';
const phrase = (vi, en) => lang() === 'en' ? en : vi;
const title = entry => entry?.name?.[lang()] || entry?.name?.vi || entry?.name || '';
const head = (catalog, id) => catalog.characters.find(x => x.id === id)?.assets?.head || './assets/nam_lang_head.png';
const phaseText = phase => ({placing:phrase('CHỌN HƯỚNG NÉM','AIM YOUR THROW'),reveal:phrase('LỘ DIỆN','REVEAL'),throw:phrase('ĐANG NÉM','THROWING'),throwing:phrase('ĐANG NÉM','THROWING'),resolve:phrase('KẾT QUẢ VÒNG','ROUND RESULT'),roundend:phrase('KẾT QUẢ VÒNG','ROUND RESULT'),matchend:phrase('HẾT TRẬN','MATCH END')})[phase] || escape(phase || '…');
const hearts = (hp, max) => `<span class="live-hearts" aria-label="${hp}/${max} HP">${'♥'.repeat(Math.max(0, Math.min(6, hp || 0)))}<span>${'♡'.repeat(Math.max(0, Math.min(6, (max || 3) - (hp || 0))))}</span></span>`;

export function validateLiveEvent(event) {
  if (event.type === 'match' && (!Array.isArray(event.players) || !Number.isFinite(Number(event.round)) || typeof event.phase !== 'string')) return 'match_shape';
  if (event.type === 'result' && !Array.isArray(event.rankings)) return 'result_shape';
  if (event.type === 'screen' && typeof event.name !== 'string') return 'screen_shape';
  if (event.type === 'layout' && (!event.playableRect || !Number.isFinite(Number(event.playableRect.y)))) return 'layout_shape';
  if (event.type === 'event' && typeof event.kind !== 'string') return 'event_shape';
  return null;
}

export function renderLiveMatch(state, catalog) {
  const match = state.match || {};
  const players = Array.isArray(match.players) ? match.players : [];
  const phase = String(match.phase || '').toLowerCase();
  const top = Math.max(84, Math.min(195, Number(state.layout?.playableRect?.y) || 132));
  const bottom = Math.max(100, Math.min(195, (window.visualViewport?.height || innerHeight) - ((Number(state.layout?.playableRect?.y) || 132) + (Number(state.layout?.playableRect?.h) || 400))));
  const cards = players.map(player => {
    const local = player.isLocal || player.id === 'player';
    const character = catalog.characters.find(x => x.id === player.characterId);
    return `<div class="live-card ${local?'local':''} ${player.alive===false?'out':''}" data-player="${escape(player.id)}"><img src="${escape(head(catalog, player.characterId))}" alt=""><div><b>${escape(local?phrase('BẠN','YOU'):(player.name || title(character) || `BOT ${Number(player.slot || 0)}`))}</b>${hearts(Number(player.hp),Number(player.maxHp)||3)}</div></div>`;
  }).join('');
  const seconds = Math.max(0, Number(match.timeLeft) || 0);
  const toast = state.toasts?.[0];
  const tutorial = state.tutorialStep >= 0 && phase === 'placing' ? `<div class="live-tutorial" role="dialog" aria-label="${phrase('Hướng dẫn','Tutorial')}"><b>${phrase('HƯỚNG DẪN','TUTORIAL')} ${state.tutorialStep+1}/3</b><p>${escape((lang()==='en' ? ['Tap the arena to place your character','Hold and drag to aim','After 5 seconds, everyone throws together!'] : ['Chạm vào sân để đặt nhân vật','Giữ và kéo để chọn hướng ném','Hết 5 giây, mọi người ném cùng lúc!'])[state.tutorialStep])}</p><div><button data-action="tutorial-skip">${phrase('BỎ QUA','SKIP')}</button><button data-action="tutorial-next">${phrase('TIẾP','NEXT')}</button></div></div>` : '';
  return `<main class="stage live-match" data-screen="match" style="--live-top:${top}px;--live-bottom:${bottom}px"><header class="live-top"><div class="live-summary"><b>${phrase('VÒNG','ROUND')} ${escape(match.round || 1)}/${escape(match.maxRounds || 5)}</b><strong>${phase==='placing'?`${Math.ceil(seconds)} ${phrase('GIÂY','SEC')}`:phaseText(phase)}</strong><button data-action="settings" aria-label="${phrase('Cài đặt','Settings')}">⚙</button></div><div class="live-cards">${cards}</div><div class="live-phase">${phaseText(phase)}</div></header><div class="live-arena" aria-hidden="true"></div><footer class="live-bottom"><button data-action="emote">☺ ${phrase('BIỂU CẢM','EMOTE')}</button><span>${phrase('VŨ KHÍ','WEAPON')}: ${escape(match.weaponName || title(catalog.weapons.find(x=>x.id===state.weaponId)))}</span><button data-action="settings">⚙</button></footer>${toast?`<output class="live-toast">${escape(toast.message)}</output>`:''}${tutorial}</main>`;
}

export function renderLiveResult(state, catalog) {
  const result = state.result || {};
  const rankings = Array.isArray(result.rankings) ? result.rankings : [];
  const me = rankings.find(x => x.id === 'player');
  const won = me && me.rank === 1;
  const rows = rankings.map(player => `<div class="live-rank"><b>${escape(player.rank ?? '—')}</b><img src="${escape(head(catalog, player.characterId || (player.id==='player'?state.characterId:'nam_lang')))}" alt=""><span>${escape(player.name || player.id)}</span><small>${phrase('TRÚNG','HITS')} ${Number.isFinite(Number(player.hits))?escape(player.hits):'—'}<br>${phrase('VÒNG SỐNG','ROUNDS')} ${Number.isFinite(Number(player.roundSurvived))?escape(player.roundSurvived):'—'}</small></div>`).join('');
  return `<main class="stage live-result" data-screen="result"><div class="live-result-panel"><h1>${won?phrase('CHIẾN THẮNG!','VICTORY!'):phrase('KẾT QUẢ TRẬN','MATCH RESULT')}</h1><p>${phrase('Vòng','Round')} ${escape(result.round || state.match?.round || '—')} · ${phrase('Đã chơi','Played')} ${escape(state.sessionMatches || 1)} ${phrase('trận','matches')}</p><div class="live-ranks">${rows || `<p>${phrase('Đang nhận bảng xếp hạng…','Waiting for rankings…')}</p>`}</div><button class="live-primary" data-action="rematch">${phrase('CHƠI LẠI','REMATCH')}</button><button class="live-secondary" data-action="exit-match">${phrase('VỀ TRANG CHỦ','BACK HOME')}</button></div></main>`;
}
