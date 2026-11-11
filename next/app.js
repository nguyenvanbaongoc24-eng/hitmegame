import './safety.js';
import {onUnity, send} from './bridge.js';
import {installMockUnity} from './mock-unity.js';
import {audio} from './audio.js';
import {prototype, title as catalogTitle} from './prototype.js';
import {renderLiveMatch,renderLiveResult,validateLiveEvent,updateLiveMatchInPlace} from './live.js';

const copy = await (await fetch('./i18n.json')).json();
const catalog = await (await fetch('./catalog.json')).json();
window.__hitmeCatalog = catalog;
const t = key => copy[document.documentElement.lang === 'en' ? 'en' : 'vi'][key];

const params = new URLSearchParams(location.search);
if (params.get('lang') === 'en') document.documentElement.lang = 'en';
const legacy = params.get('ui') === 'unity';
const mock = params.get('mock') === '1';
const root = document.getElementById('ui-root');
const canvas = document.getElementById('unity-canvas');
if (legacy) root.hidden = true;

function syncViewport() {
  const viewport = window.visualViewport;
  const height = viewport?.height || window.innerHeight;
  const width = viewport?.width || window.innerWidth;
  document.documentElement.style.setProperty('--viewport-height', `${height}px`);
  document.documentElement.style.setProperty('--viewport-width', `${width}px`);
  document.documentElement.style.setProperty('--viewport-top', `${viewport?.offsetTop || 0}px`);
  document.documentElement.style.setProperty('--ui-scale', String(Math.min(1, Math.max(.77, height / 695))));
  if (!mock && state?.ready) sendLayout();
  if (params.get('debug') === 'viewport') updateViewportDebug();
}
function updateViewportDebug() {
  let panel = document.getElementById('viewport-debug');
  if (!panel) {
    panel = document.createElement('output');
    panel.id = 'viewport-debug';
    document.body.append(panel);
  }
  const inset = document.createElement('div');
  inset.style.cssText = 'position:fixed;top:env(safe-area-inset-top);bottom:env(safe-area-inset-bottom);visibility:hidden';
  document.body.append(inset);
  const r = canvas.getBoundingClientRect();
  panel.textContent = `inner ${innerWidth}×${innerHeight} | visual ${Math.round(visualViewport?.width || innerWidth)}×${Math.round(visualViewport?.height || innerHeight)} top ${Math.round(visualViewport?.offsetTop || 0)} | DPR ${devicePixelRatio} | canvas ${Math.round(r.width)}×${Math.round(r.height)} / ${canvas.width}×${canvas.height} | safe top ${Math.round(inset.getBoundingClientRect().top)} bottom ${Math.round(innerHeight-inset.getBoundingClientRect().bottom)}`;
  inset.remove();
}
window.addEventListener('resize', syncViewport);
window.visualViewport?.addEventListener('resize', syncViewport);
window.visualViewport?.addEventListener('scroll', syncViewport);

const icons = {
  play:'<path d="M8 4.5 21 12 8 19.5Z"/>',
  people:'<circle cx="9" cy="8" r="3"/><path d="M2 19v-2a6 6 0 0 1 12 0v2zM16 6a3 3 0 0 1 0 6m2 3a5 5 0 0 1 4 5"/>',
  robot:'<rect x="3" y="7" width="18" height="14" rx="4"/><circle cx="9" cy="14" r="1"/><circle cx="15" cy="14" r="1"/><path d="M12 7V3m-2 0h4"/>',
  gear:'<circle cx="12" cy="12" r="3"/><path d="m12 2 1 2.2 2.4.5 1.9-1.5 2 2-1.5 1.9.5 2.4L21.5 12l-2.2 1-.5 2.4 1.5 1.9-2 2-1.9-1.5-2.4.5L12 21.5l-1-2.2-2.4-.5-1.9 1.5-2-2 1.5-1.9-.5-2.4L2.5 12l2.2-1 .5-2.4-1.5-1.9 2-2 1.9 1.5 2.4-.5z"/>',
  edit:'<path d="m4 17 12-12 3 3L7 20l-4 1zM14 7l3 3"/>',
  coin:'<circle cx="12" cy="12" r="10"/><path d="M12 5v14M8 9c0-2 8-2 8 0s-8 2-8 4 8 2 8 4"/>',
  plus:'<path d="M12 4v16M4 12h16"/>',
  home:'<path d="m2 11 10-8 10 8v10h-7v-7H9v7H2z"/>',
  trophy:'<path d="M6 3h12v5c0 5-2 7-6 7s-6-2-6-7zM6 5H2v3c0 3 2 4 5 4m11-7h4v3c0 3-2 4-5 4m-5 3v4m-5 2h10"/>',
  gift:'<rect x="3" y="9" width="18" height="12" rx="1"/><path d="M2 9h20M12 9v12M12 9C6 9 5 3 8 3c2 0 4 3 4 6zm0 0c6 0 7-6 4-6-2 0-4 3-4 6z"/>',
  chevron:'<path d="m9 4 8 8-8 8"/>',
  heart:'<path d="M12 21 3 12C-2 7 5 1 12 7c7-6 14 0 9 5z"/>',
  clock:'<circle cx="12" cy="12" r="10"/><path d="M12 5v7l5 3"/>',
  chat:'<path d="M3 4h18v13H8l-5 4z"/>',
  close:'<path d="M4 4 20 20M20 4 4 20"/>',
  check:'<path d="m3 12 6 6L21 5"/>',
  lock:'<rect x="5" y="10" width="14" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>'
};
const icon = (name, extra='') => `<svg class="icon ${extra}" viewBox="0 0 24 24" aria-hidden="true" focusable="false">${icons[name]}</svg>`;
const safeText = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const assetId = id => /^[a-z0-9_]+$/.test(id || '') ? id : 'nam_lang';
const remembered = key => {try{return localStorage.getItem(key)}catch{return null}};
const remember = (key,value) => {try{localStorage.setItem(key,value)}catch{ /* private mode */ }};

// Auto-cleanse legacy data for new releases so all users experience fresh gameplay & FTUE
const SCHEMA_VERSION = 'v1.0.0-beta.13';
if (remembered('hitme.schema') !== SCHEMA_VERSION) {
  try {
    const legacyKeys = ['hitme.ftueCompleted','hitme.tutorialSeen','hitme.playerName','hitme.characterId','hitme.weaponId','hitme_user_profile','hitme_loadout','hitme_guest_token','guestId'];
    legacyKeys.forEach(k => { try { localStorage.removeItem(k); } catch(e){} });
    localStorage.setItem('hitme.schema', SCHEMA_VERSION);
  } catch(e) {}
}

const initialCharacter = remembered('hitme.characterId');
const initialWeapon = remembered('hitme.weaponId');
const ftueDone = remembered('hitme.ftueCompleted') === '1' || params.has('skip_ftue');
const state = {screen:mock?(params.get('scene')||'home'):'home',previousScreen:'home',catalog,roster:[],weapons:[],characterId:catalog.characters.some(x=>x.id===initialCharacter&&x.enabled)?initialCharacter:'nam_lang',weaponId:catalog.weapons.some(x=>x.id===initialWeapon&&x.enabled)?initialWeapon:'dep_to_ong',balance:null,version:'',build:'',lobby:null,serverHealth:'waking',match:null,result:null,overlay:(!ftueDone && !mock)?'ftue':null,ftueStep:1,emoteTab:'sticker',emoteBubbles:{},emoteCooldownUntil:0,readyLocked:false,practiceActive:false,reducedMotion:false,ready:false,loadingPercent:0,slideDirection:'',botCount:3,lobbyReady:false,musicVolume:audio.musicVolume,sfxVolume:audio.sfxVolume,mockRound:1,mockPhase:'placing',mockTimeLeft:5,mockHp:3,mockEvent:''};
state.layout=null;state.toasts=[];state.roundEvents=[];state.currentRound=0;state.contractErrors=[];state.sessionMatches=0;state.tutorialStep=-1;state.paused=false;
let tutorialPointer=null;
canvas.addEventListener('pointerdown', event => {
  if (!mock && state.screen==='match' && state.match?.phase?.toLowerCase()==='placing' && state.tutorialStep>=0)
    tutorialPointer={id:event.pointerId,x:event.clientX,y:event.clientY};
});
canvas.addEventListener('pointerup', event => {
  if (!tutorialPointer || tutorialPointer.id!==event.pointerId) return;
  const dragged=Math.hypot(event.clientX-tutorialPointer.x,event.clientY-tutorialPointer.y)>15;
  tutorialPointer=null;
  if (state.tutorialStep===0 || (state.tutorialStep===1 && dragged)) {state.tutorialStep++;render();}
});
canvas.addEventListener('pointercancel', () => {tutorialPointer=null;});
syncViewport();
const onlineEnabled = mock || catalog.features?.online === true;
const healthText = {ready:'Máy chủ sẵn sàng',waking:'Đang đánh thức máy chủ',offline:'Không kết nối'};
async function checkServerHealth() {
  if(document.visibilityState==='hidden')return;
  const controller=new AbortController();const timer=setTimeout(()=>controller.abort(),8000);
  let status='waking';
  try {
    const response=await fetch('https://hitmegame.onrender.com/healthz',{signal:controller.signal,cache:'no-store',mode:'no-cors'});
    status=response.type==='opaque'?'waking':response.ok&&(await response.json()).status==='ok'?'ready':response.status===503?'waking':'offline';
  } catch {status=navigator.onLine===false?'offline':'waking';} // Render lacks CORS; opaque means reachable but not proven ready.
  finally {clearTimeout(timer);}
  if(state.serverHealth!==status){state.serverHealth=status;render();}
}
checkServerHealth();setInterval(checkServerHealth,30000);
function contractError(code) { console.error(`[HITME_CONTRACT:${code}]`); state.contractErrors.push(code); if(state.contractErrors.length>10)state.contractErrors.shift();render(); }
window.addEventListener('hitme-contract-error',event=>contractError(event.detail));
let lastLayout='';
let toastTimer=null;
const toastSeen=new Map();
function queueToast(message) {
  if (!message) return;
  const now=performance.now();
  if (now-(toastSeen.get(message) ?? -Infinity)<2200) return;
  toastSeen.set(message,now);
  if (state.toasts.length<4) state.toasts.push({message});
  if (!toastTimer) {
    const next=()=>{state.toasts.shift();render();toastTimer=state.toasts.length?setTimeout(next,2200):null;};
    toastTimer=setTimeout(next,2200);
  }
  render();
}
function clearPhaseToasts() {
  if (toastTimer) clearTimeout(toastTimer);
  toastTimer=null;
  state.toasts=[];
}
function showEmoteBubble(playerId, label) {
  state.emoteBubbles[playerId]=label;
  render();
  setTimeout(()=>{delete state.emoteBubbles[playerId];render();},2500);
}
// Practice-only ambient emotes. Never derive a bot's position from this UI.
setInterval(()=>{
  if(mock||!state.practiceActive||state.screen!=='match'||!state.match?.players?.length)return;
  const bots=state.match.players.filter(player=>player.id!=='player'&&player.alive!==false);
  if(!bots.length)return;
  const bot=bots[Math.floor(Math.random()*bots.length)];
  showEmoteBubble(bot.id,['GG','Chào!','Ối!'][Math.floor(Math.random()*3)]);
},20000);
function sendLayout() {
  const height=window.visualViewport?.height||innerHeight;
  const top=Math.round(Math.max(92,Math.min(175,height*.2)));
  const bottom=Math.round(Math.max(85,Math.min(145,height*.16)));
  const probe=document.createElement('div');probe.style.cssText='position:fixed;top:env(safe-area-inset-top);bottom:env(safe-area-inset-bottom);visibility:hidden';document.body.append(probe);
  const bounds=probe.getBoundingClientRect();const safeTop=Math.max(0,Math.round(bounds.top));const safeBottom=Math.max(0,Math.round(innerHeight-bounds.bottom));probe.remove();
  const payload={top,bottom,safeTop,safeBottom};
  const key=JSON.stringify(payload);
  if(key!==lastLayout){lastLayout=key;send('SetLayout',payload);}
}
const current = () => catalog.characters.find(c => c.id === state.characterId) || catalog.characters[0];
const currentWeapon = () => catalog.weapons.find(w=>w.id===state.weaponId) || catalog.weapons[0];
const characterImage = kind => current().assets[kind];
const saveSelection = () => {
  remember('hitme.characterId',state.characterId);
  remember('hitme.weaponId',state.weaponId);
  try {
    const prof = JSON.parse(localStorage.getItem('hitme.localProfile') || '{}');
    prof.characterId = state.characterId;
    prof.weaponId = state.weaponId;
    localStorage.setItem('hitme.localProfile', JSON.stringify(prof));
  } catch(e) {}
};
const LoadoutStore = {
  getWeapon: () => remembered('hitme.weaponId') || state.weaponId || 'dep_to_ong',
  setWeapon: (id) => {
    state.weaponId = id;
    saveSelection();
    send('SetWeapon', { id });
  },
  getCharacter: () => remembered('hitme.characterId') || state.characterId || 'nam_lang',
  setCharacter: (id) => {
    state.characterId = id;
    saveSelection();
    send('SetCharacter', { id });
  }
};
window.__hitmeLoadout = LoadoutStore;
const go = screen => {state.screen=screen;state.overlay=null;render()};

function home() {
  const character = current();
  return `<main class="stage home" data-screen="home">
    <div class="backdrop" aria-hidden="true"></div>
    <div class="home-bottom-art" aria-hidden="true"></div>
    <header class="profile" data-name="profile">
      <button class="avatar round" data-action="profile" aria-label="${t('profile')}"><img src="${characterImage('head')}" alt=""></button>
      <button class="profile-name" data-action="profile"><strong>${safeText(remembered('hitme.playerName') || catalogTitle(character))}</strong><span>${t('profile')} ${icon('edit')}</span></button>
      <button class="gold" data-action="shop" aria-label="Vàng ${state.balance || 1250}">${icon('coin')}<b>${Number(state.balance || 1250).toLocaleString('vi-VN')}</b><span class="add">${icon('plus')}</span></button>
    </header>
    <button class="settings icon-round" data-name="settings" data-action="settings" aria-label="${t('settings')}">${icon('gear')}</button>
    <div class="logo" data-name="logo" aria-label="HIT ME"><img class="logo-image" src="./assets/logo_hit_me.png" alt="HIT ME"></div>
    <div class="pedestal" aria-hidden="true"></div>
    <img class="hero ${state.slideDirection ? `slide-${state.slideDirection}` : ''}" data-name="hero" src="${characterImage('portrait')}" alt="${safeText(catalogTitle(character))}">
    <button class="hero-arrow left icon-round" data-name="arrow-left" data-action="previous" aria-label="Nhân vật trước">${icon('chevron')}</button>
    <button class="hero-arrow right icon-round" data-name="arrow-right" data-action="next" aria-label="Nhân vật tiếp theo">${icon('chevron')}</button>
    <button class="selection-card character-card" data-name="character-card" data-action="character"><img src="${characterImage('head')}" alt=""><span><small>${t('character')}</small><b>${safeText(catalogTitle(character))}</b></span>${icon('chevron')}</button>
    <button class="selection-card weapon-card" data-name="weapon-card" data-action="weapon">${icon('chevron','reverse')}<img src="${safeText(currentWeapon().icon)}" alt=""><span><small>${t('weapon')}</small><b>${safeText(catalogTitle(currentWeapon()))}</b></span>${icon('chevron')}</button>
    <button class="cta quick ${onlineEnabled?'':'coming-soon'}" data-name="quick-play" data-action="${onlineEnabled?'quick':'online-locked'}">${onlineEnabled?icon('play'):icon('lock')}<span><strong>${t('quickPlay')}</strong><small>${onlineEnabled?t('quickPlayDetail'):t('comingSoon')} ${!onlineEnabled?`<i class="server-dot ${state.serverHealth}" role="status" aria-label="${healthText[state.serverHealth]}" title="${healthText[state.serverHealth]}"></i>`:''}</small></span></button>
    <button class="cta blue ${onlineEnabled?'':'coming-soon'}" data-name="create-room" data-action="${onlineEnabled?'create':'online-locked'}">${onlineEnabled?icon('people'):icon('lock')}<span><strong>${t('createRoom')}</strong>${onlineEnabled?'':`<small>${t('comingSoon')}</small>`}</span></button>
    <button class="cta green ${onlineEnabled?'':'coming-soon'}" data-name="join-room" data-action="${onlineEnabled?'join':'online-locked'}">${onlineEnabled?icon('people'):icon('lock')}<span><strong>${t('joinRoom')}</strong>${onlineEnabled?'':`<small>${t('comingSoon')}</small>`}</span></button>
    <button class="cta navy practice" data-name="practice" data-action="practice">${icon('robot')}<strong>${t('practice')}</strong>${icon('chevron')}</button>
    <nav class="tabs" data-name="tabs" aria-label="Điều hướng chính">
      <button class="active" data-action="home">${icon('home')}<span>${t('home')}</span></button>
      <button data-action="rank">${icon('trophy')}<span>${t('ranking')}</span></button>
      <button data-action="quests">${icon('gift')}<span>${t('quests')}</span></button>
    </nav>
  </main>`;
}
function modal() {
  if (!state.overlay) return '';
  const isEn = (document.documentElement.lang || 'vi') === 'en';
  const title = {character:t('chooseCharacter'),weapon:t('chooseWeapon'),settings:t('settings'),profile:t('profile'),rank:'BẢNG XẾP HẠNG',quests:'NHIỆM VỤ',join:t('joinRoom'),shop:'CỬA HÀNG VŨ KHÍ',loading:t('loadingGame'),emote:'Biểu cảm','exit-confirm':'Thoát trận',ftue:(isEn?`THIẾT LẬP (${state.ftueStep||1}/5)`:`THIẾT LẬP (${state.ftueStep||1}/5)`)}[state.overlay] || 'HIT ME';
  const choices = state.overlay === 'character' ? catalog.characters : state.overlay === 'weapon' ? catalog.weapons : [];
  const list = choices.length ? `<div class="choice-grid">${choices.map(item => `<button data-action="${item.enabled?`select-${state.overlay}`:'locked'}" data-id="${safeText(item.id)}" ${item.enabled?'':'aria-disabled="true" class="locked"'}><img src="${safeText(state.overlay==='character'?item.assets.head:item.icon)}" alt=""><span>${safeText(catalogTitle(item))}</span>${item.enabled?'':`${icon('lock')}<small>${t('comingSoon')}</small>`}</button>`).join('')}</div>` : '';
  const join = state.overlay === 'join' ? `<label>${t('roomCode')} <input id="room-code" maxlength="6" autocomplete="off" placeholder="HM4826"></label><button class="cta quick dialog-action" data-action="join-submit">${t('joinRoom')}</button>` : '';
  const settings = state.overlay === 'settings' ? `
    <div class="r40-settings-card" style="display:flex;flex-direction:column;gap:12px;width:100%;box-sizing:border-box;">
      <div style="display:flex;justify-content:space-between;align-items:center;padding-bottom:10px;border-bottom:1px solid rgba(255,255,255,0.1);">
        <span style="font-weight:800;color:#cbd5e1;font-size:13.5px;">${isEn ? 'Language' : 'Ngôn ngữ'}</span>
        <div class="r43-lang-toggle" style="display:inline-flex;border:1.5px solid #ffcd38;border-radius:8px;overflow:hidden;background:#0d1b2a;">
          <button type="button" data-action="set-lang-vi" style="padding:5px 12px;font-size:12px;font-weight:900;border:none;cursor:pointer;background:${isEn ? 'transparent' : '#ffcd38'};color:${isEn ? '#fff8e8' : '#08121f'};">VI</button>
          <button type="button" data-action="set-lang-en" style="padding:5px 12px;font-size:12px;font-weight:900;border:none;cursor:pointer;background:${isEn ? '#ffcd38' : 'transparent'};color:${isEn ? '#08121f' : '#fff8e8'};">EN</button>
        </div>
      </div>
      <div>
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:4px;font-weight:800;color:#cbd5e1;font-size:13px;">
          <span>${isEn ? 'Background Music' : 'Nhạc nền'}</span>
          <span class="music-val" style="color:#ffcd38;font-weight:900;">${Math.round(state.musicVolume*100)}%</span>
        </div>
        <input type="range" class="r40-slider" data-setting="music" min="0" max="100" value="${Math.round(state.musicVolume*100)}" style="--val-pct:${Math.round(state.musicVolume*100)}%;width:100%;">
      </div>
      <div>
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:4px;font-weight:800;color:#cbd5e1;font-size:13px;">
          <span>${isEn ? 'Sound Effects' : 'Hiệu ứng âm thanh'}</span>
          <span class="sfx-val" style="color:#ffcd38;font-weight:900;">${Math.round(state.sfxVolume*100)}%</span>
        </div>
        <input type="range" class="r40-slider" data-setting="sfx" min="0" max="100" value="${Math.round(state.sfxVolume*100)}" style="--val-pct:${Math.round(state.sfxVolume*100)}%;width:100%;">
      </div>
      <div style="display:flex;justify-content:space-between;align-items:center;padding:8px 0;border-top:1px solid rgba(255,255,255,0.08);">
        <span style="font-weight:800;color:#cbd5e1;font-size:13px;">${isEn ? 'Haptics Vibration' : 'Rung xúc giác (Haptics)'}</span>
        <label class="r40-switch"><input type="checkbox" data-setting="haptics" ${state.haptics!==false?'checked':''}><span class="r40-switch-slider"></span></label>
      </div>
      <div style="display:flex;justify-content:space-between;align-items:center;padding:8px 0;border-top:1px solid rgba(255,255,255,0.08);">
        <span style="font-weight:800;color:#cbd5e1;font-size:13px;">${isEn ? 'Show FPS Counter' : 'Chỉ số khung hình (FPS)'}</span>
        <label class="r40-switch"><input type="checkbox" data-setting="fps" ${remembered('hitme.showFps')==='1'?'checked':''}><span class="r40-switch-slider"></span></label>
      </div>
      <div style="display:flex;justify-content:space-between;align-items:center;padding:8px 0;border-top:1px solid rgba(255,255,255,0.08);">
        <span style="font-weight:800;color:#cbd5e1;font-size:13px;">${isEn ? 'Reduced Motion' : 'Giảm chuyển động'}</span>
        <label class="r40-switch"><input type="checkbox" data-setting="reduced-motion" ${state.reducedMotion?'checked':''}><span class="r40-switch-slider"></span></label>
      </div>
      <button type="button" class="live-secondary" data-action="open-ftue" style="margin-top:6px;width:100%;font-size:12.5px;">${isEn ? 'REPLAY TUTORIAL (FTUE)' : 'XEM LẠI HƯỚNG DẪN (FTUE)'}</button>
      <button type="button" class="live-secondary danger" data-action="factory-reset" style="margin-top:6px;width:100%;font-size:12.5px;background:#7f1d1d!important;border:1.5px solid #ef4444!important;color:#fee2e2!important;font-weight:900;">${isEn ? 'RESET ALL DATA (START FRESH)' : 'XÓA DỮ LIỆU & BẮT ĐẦU LẠI'}</button>
      ${!mock && state.screen === 'match' ? `
        <div style="display:flex;flex-direction:column;gap:8px;margin-top:6px;border-top:1px solid rgba(255,255,255,0.1);padding-top:10px;">
          <button type="button" class="live-secondary" data-action="${state.paused ? 'resume' : 'pause'}">${state.paused ? (isEn ? 'RESUME' : 'TIẾP TỤC') : (isEn ? 'PAUSE' : 'TẠM DỪNG')}</button>
          <button type="button" class="live-secondary danger" data-action="exit-confirm" style="background:#7f1d1d!important;border:1.5px solid #ef4444!important;color:#fee2e2!important;font-weight:900;">${isEn ? 'QUIT MATCH' : 'THOÁT TRẬN'}</button>
          <button type="button" class="live-secondary" data-action="tutorial-reopen">${isEn ? 'VIEW TUTORIAL' : 'XEM LẠI HƯỚNG DẪN'}</button>
        </div>
      ` : ''}
      <a href="https://forms.gle/hitmegame-feedback-mock" target="_blank" rel="noopener noreferrer" class="r40-settings-feedback" style="display:flex;align-items:center;justify-content:center;gap:6px;width:100%;min-height:42px;background:rgba(25,213,216,0.12);border:1.5px solid #19d5d8;border-radius:10px;color:#19d5d8;font-weight:800;font-size:13px;text-decoration:none;box-sizing:border-box;margin-top:4px;">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
        ${isEn ? 'Send Feedback' : 'Gửi góp ý trải nghiệm'}
      </a>
      <div style="display:flex;justify-content:space-between;align-items:center;padding-top:8px;border-top:1px solid rgba(255,255,255,0.08);font-size:11px;color:#94a3b8;">
        <span>Phiên bản 0.1.0-beta.12 · 5ef2718</span>
        <button type="button" data-action="copy-ver" style="background:transparent;border:none;color:#ffcd38;font-size:11.5px;font-weight:800;cursor:pointer;text-decoration:underline;">${isEn ? 'Copy' : 'Sao chép'}</button>
      </div>
    </div>
  ` : '';
  const scenarioSettings = state.overlay === 'settings' && params.get('dev') === '1' && !mock
    ? `<section aria-label="Kịch bản thử luật"><h3>Kịch bản thử luật (chỉ để kiểm tra)</h3>${['tiebreak','tiebreak3','tiebreak_draw','suddendeath','hardcap','ranking_ties'].map(name=>`<button class="live-secondary" data-action="start-scenario" data-scenario="${name}">${name}</button>`).join('')}</section>` : '';
  const confirmation = state.overlay==='exit-confirm' ? `<div style="text-align:center;padding:12px 0;"><h3 style="color:#ef4444;margin:0 0 8px;font-size:18px;">${isEn ? 'Exit Match?' : 'Thoát trận đấu?'}</h3><p style="color:#cbd5e1;font-size:13.5px;margin:0 0 16px;">${isEn ? 'Are you sure you want to exit to home?' : 'Bạn có chắc chắn muốn thoát về màn hình chính không?'}</p><div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;"><button class="live-secondary" data-action="close">${isEn ? 'STAY' : 'Ở LẠI'}</button><button class="live-primary danger" data-action="exit-match" style="background:#dc2626!important;border:1.5px solid #ef4444!important;color:#fff!important;">${isEn ? 'EXIT' : 'THOÁT TRẬN'}</button></div></div>` : '';
  const emotePhrases = document.documentElement.lang==='en' ? ['Hello everyone!','Can you dodge?','Well done!','Play again!','Oops!','GG'] : ['Chào cả nhà!','Né được không?','Giỏi lắm!','Chơi lại nhé!','Ối!','GG'];
  const stickerIcon='<svg viewBox="0 0 24 24" width="30" height="30" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M8 14c2 3 6 3 8 0M8 9h.01M16 9h.01" stroke-linecap="round"/></svg>';
  const emotes = state.overlay==='emote' ? `<div class="live-emote-tabs"><button data-action="emote-tab" data-tab="sticker" class="${state.emoteTab==='sticker'?'active':''}">Sticker</button><button data-action="emote-tab" data-tab="phrase" class="${state.emoteTab==='phrase'?'active':''}">${document.documentElement.lang==='en'?'Quick phrases':'Câu nhanh'}</button></div><div class="live-emotes ${state.emoteTab}">${state.emoteTab==='sticker'?Array.from({length:6},(_,i)=>`<button data-action="send-live-emote" data-kind="sticker" data-id="sticker_0${i+1}" aria-label="Sticker ${i+1}">${stickerIcon}<small>sticker tạm</small></button>`).join(''):emotePhrases.map((text,i)=>`<button data-action="send-live-emote" data-kind="phrase" data-id="phrase_0${i+1}">${safeText(text)}</button>`).join('')}</div>` : '';
  const rankContent = state.overlay === 'rank' ? `
    <div class="r44-rank" style="font-size:11px;color:#fde047;font-weight:700;margin-bottom:8px;background:rgba(68,50,14,0.7);padding:4px 8px;border-radius:6px;">DỮ LIỆU MẪU • BẢN THỬ NGHIỆM</div>
    <div class="leaderboard-list" style="display:flex;flex-direction:column;gap:8px;width:100%;">
      <div style="display:flex;align-items:center;justify-content:space-between;background:rgba(255,205,56,0.15);border:1.5px solid #ffcd38;padding:8px 12px;border-radius:10px;">
        <div style="display:flex;align-items:center;gap:10px;">
          <span style="font-weight:900;color:#ffcd38;font-size:16px;">#1</span>
          <img src="./assets/nam_lang_head.png" width="36" height="36" style="border-radius:50%;border:1.5px solid #ffcd38;">
          <div><b style="color:#fff;font-size:14px;display:block;">Nam Làng</b><small style="color:#94a3b8;font-size:11px;">15 trận thắng • Tỉ lệ 80%</small></div>
        </div>
        <b style="color:#ffcd38;font-size:14px;">1250 điểm</b>
      </div>
      <div style="display:flex;align-items:center;justify-content:space-between;background:rgba(255,255,255,0.06);border:1px solid rgba(255,255,255,0.15);padding:8px 12px;border-radius:10px;">
        <div style="display:flex;align-items:center;gap:10px;">
          <span style="font-weight:900;color:#e2e8f0;font-size:16px;">#2</span>
          <img src="./assets/nu_nang_dong_head.png" width="36" height="36" style="border-radius:50%;">
          <div><b style="color:#fff;font-size:14px;display:block;">Nữ Năng Động</b><small style="color:#94a3b8;font-size:11px;">12 trận thắng • Tỉ lệ 75%</small></div>
        </div>
        <b style="color:#38bdf8;font-size:14px;">980 điểm</b>
      </div>
      <div style="display:flex;align-items:center;justify-content:space-between;background:rgba(255,255,255,0.06);border:1px solid rgba(255,255,255,0.15);padding:8px 12px;border-radius:10px;">
        <div style="display:flex;align-items:center;gap:10px;">
          <span style="font-weight:900;color:#cd7f32;font-size:16px;">#3</span>
          <img src="./assets/co_ba_head.png" width="36" height="36" style="border-radius:50%;">
          <div><b style="color:#fff;font-size:14px;display:block;">Cô Ba</b><small style="color:#94a3b8;font-size:11px;">10 trận thắng • Tỉ lệ 70%</small></div>
        </div>
        <b style="color:#38bdf8;font-size:14px;">850 điểm</b>
      </div>
      <div style="display:flex;align-items:center;justify-content:space-between;background:rgba(255,255,255,0.06);border:1px solid rgba(255,255,255,0.15);padding:8px 12px;border-radius:10px;">
        <div style="display:flex;align-items:center;gap:10px;">
          <span style="font-weight:900;color:#94a3b8;font-size:16px;">#4</span>
          <img src="./assets/anh_teen_head.png" width="36" height="36" style="border-radius:50%;">
          <div><b style="color:#fff;font-size:14px;display:block;">Anh Teen</b><small style="color:#94a3b8;font-size:11px;">8 trận thắng • Tỉ lệ 60%</small></div>
        </div>
        <b style="color:#38bdf8;font-size:14px;">720 điểm</b>
      </div>
    </div>` : '';

  const questsContent = state.overlay === 'quests' ? `
    <div class="r44-quests" style="font-size:11px;color:#fde047;font-weight:700;margin-bottom:8px;background:rgba(68,50,14,0.7);padding:4px 8px;border-radius:6px;">NHIỆM VỤ NGÀY • LÀM MỚI MỖI NGÀY</div>
    <div style="display:flex;flex-direction:column;gap:8px;width:100%;">
      <div style="display:flex;align-items:center;justify-content:space-between;background:rgba(255,255,255,0.06);border:1px solid rgba(255,255,255,0.15);padding:10px 12px;border-radius:10px;">
        <div>
          <b style="color:#fff;font-size:14px;display:block;">Chơi 1 trận Luyện với bot</b>
          <small style="color:#94a3b8;font-size:11px;">Tiến độ: ${state.sessionMatches > 0 ? '1/1 (Hoàn thành)' : '0/1'}</small>
        </div>
        <span style="font-size:12px;font-weight:700;color:${state.sessionMatches > 0 ? '#4ade80' : '#ffcd38'};background:rgba(255,255,255,0.08);padding:4px 8px;border-radius:6px;">+50 Vàng</span>
      </div>
      <div style="display:flex;align-items:center;justify-content:space-between;background:rgba(255,255,255,0.06);border:1px solid rgba(255,255,255,0.15);padding:10px 12px;border-radius:10px;">
        <div>
          <b style="color:#fff;font-size:14px;display:block;">Ném trúng đối thủ 3 lần</b>
          <small style="color:#94a3b8;font-size:11px;">Tiến độ: 2/3</small>
        </div>
        <span style="font-size:12px;font-weight:700;color:#ffcd38;background:rgba(255,255,255,0.08);padding:4px 8px;border-radius:6px;">+60 Vàng</span>
      </div>
      <div style="display:flex;align-items:center;justify-content:space-between;background:rgba(255,255,255,0.06);border:1px solid rgba(255,255,255,0.15);padding:10px 12px;border-radius:10px;">
        <div>
          <b style="color:#fff;font-size:14px;display:block;">Sống sót tới Vòng 3</b>
          <small style="color:#94a3b8;font-size:11px;">Tiến độ: 3/3 (Hoàn thành)</small>
        </div>
        <span style="font-size:12px;font-weight:700;color:#4ade80;background:rgba(255,255,255,0.08);padding:4px 8px;border-radius:6px;">+70 Vàng</span>
      </div>
    </div>` : '';

  const shopContent = state.overlay === 'shop' ? `
    <div style="font-size:12px;color:#94a3b8;margin-bottom:12px;text-align:center;">Vũ khí chỉ thay đổi hình ảnh & âm thanh • Miễn phí trong Beta</div>
    <div style="display:flex;flex-direction:column;gap:8px;width:100%;">
      ${catalog.weapons.map(w => {
        const equipped = w.id === state.weaponId;
        return `
          <div style="display:flex;align-items:center;justify-content:space-between;gap:12px;background:rgba(255,255,255,0.06);border:1.5px solid ${equipped?'#ffcd38':'rgba(255,255,255,0.15)'};padding:10px 12px;border-radius:10px;">
            <div style="display:flex;align-items:center;gap:10px;min-width:0;">
              <img src="${w.icon}" width="40" height="40" style="border-radius:8px;border:1px solid #ffcd38;flex-shrink:0;">
              <div style="min-width:0;">
                <b style="color:#fff;font-size:14px;display:block;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${catalogTitle(w)}</b>
                <small style="color:#38bdf8;font-size:11px;display:block;">${w.id==='chao_chong_dinh'?'Tiếng Boong vang dội':w.id==='vot_pickleball'?'Đường bóng xoáy tốc độ':'Vũ khí huyền thoại sân đình'}</small>
              </div>
            </div>
            <button class="${equipped?'live-secondary':'live-primary'}" data-action="select-weapon" data-id="${w.id}" style="width:96px;min-width:96px;padding:8px 0;font-size:12px;margin:0;text-align:center;flex-shrink:0;">
              ${equipped?'✓ ĐANG DÙNG':'TRANG BỊ'}
            </button>
          </div>
        `;
      }).join('')}
    </div>` : '';

  const profileContent = state.overlay === 'profile' ? `
    <div style="font-size:11px;color:#fde047;font-weight:700;margin-bottom:8px;background:rgba(68,50,14,0.7);padding:4px 8px;border-radius:6px;">HỒ SƠ NGƯỜI CHƠI • BẢN THỬ NGHIỆM</div>
    <div style="display:flex;flex-direction:column;gap:10px;width:100%;align-items:center;">
      <img src="${characterImage('head')}" width="64" height="64" style="border-radius:50%;border:2px solid #ffcd38;">
      <h3 style="margin:0;color:#fff;font-size:16px;">${safeText(catalogTitle(character()))}</h3>
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:8px;width:100%;">
        <div style="background:rgba(255,255,255,0.06);padding:8px 12px;border-radius:8px;text-align:center;">
          <small style="color:#94a3b8;font-size:11px;display:block;">Trận đã chơi</small>
          <b style="color:#ffcd38;font-size:15px;">${state.sessionMatches || 1}</b>
        </div>
        <div style="background:rgba(255,255,255,0.06);padding:8px 12px;border-radius:8px;text-align:center;">
          <small style="color:#94a3b8;font-size:11px;display:block;">Tài sản</small>
          <b style="color:#ffcd38;font-size:15px;">${(state.balance || 1250).toLocaleString('vi-VN')} Vàng</b>
        </div>
      </div>
      <div style="background:rgba(255,255,255,0.06);padding:8px 12px;border-radius:8px;width:100%;box-sizing:border-box;">
        <small style="color:#94a3b8;font-size:11px;display:block;">Vũ khí đang trang bị</small>
        <b style="color:#38bdf8;font-size:14px;">${safeText(catalogTitle(currentWeapon()))}</b>
      </div>
    </div>` : '';

  const ftueStep = state.ftueStep || 1;
  const ftueContent = state.overlay === 'ftue' ? `
    <div class="r44-ftue-card" style="display:flex;flex-direction:column;gap:12px;width:100%;box-sizing:border-box;">
      ${ftueStep === 1 ? `
        <div style="text-align:center;padding:8px 0;">
          <img src="./assets/logo_hit_me.png" width="120" style="margin:0 auto 10px;display:block;">
          <h3 style="color:#ffcd38;margin:0 0 6px;font-size:17px;font-weight:900;">${isEn ? 'WELCOME TO HIT ME!' : 'CHÀO MỪNG ĐẾN VỚI HIT ME!'}</h3>
          <p style="color:#cbd5e1;font-size:13.5px;line-height:1.5;margin:0 0 16px;">
            ${isEn ? 'Vietnamese folk sandal-throwing battle game. Fun, fast-paced, pure skills!' : 'Trò chơi ném dép dân gian vui nhộn, gay cấn và đậm chất làng quê Việt Nam.'}
          </p>
          <div style="display:flex;justify-content:center;align-items:center;gap:12px;margin-bottom:20px;">
            <span style="font-size:13px;color:#94a3b8;">${isEn ? 'Language:' : 'Ngôn ngữ:'}</span>
            <div class="r43-lang-toggle" style="display:inline-flex;border:1.5px solid #ffcd38;border-radius:8px;overflow:hidden;background:#0d1b2a;">
              <button type="button" data-action="set-lang-vi" style="padding:5px 12px;font-size:12px;font-weight:900;border:none;cursor:pointer;background:${isEn ? 'transparent' : '#ffcd38'};color:${isEn ? '#fff8e8' : '#08121f'};">VI</button>
              <button type="button" data-action="set-lang-en" style="padding:5px 12px;font-size:12px;font-weight:900;border:none;cursor:pointer;background:${isEn ? '#ffcd38' : 'transparent'};color:${isEn ? '#08121f' : '#fff8e8'};">EN</button>
            </div>
          </div>
          <button type="button" class="live-primary" data-action="ftue-step-2" style="width:100%;min-height:48px;font-weight:900;font-size:15px;">${isEn ? 'GET STARTED' : 'BẮT ĐẦU NGAY'}</button>
        </div>
      ` : ftueStep === 2 ? `
        <div>
          <h3 style="color:#ffcd38;margin:0 0 4px;font-size:16px;font-weight:900;">${isEn ? 'CHOOSE YOUR NICKNAME' : 'ĐẶT BIỆT DANH CỦA BẠN'}</h3>
          <p style="color:#94a3b8;font-size:12px;margin:0 0 12px;">${isEn ? 'This name will appear on the leaderboard & match cards.' : 'Tên sẽ xuất hiện trên bảng điểm và thẻ người chơi.'}</p>
          <div style="display:flex;gap:8px;margin-bottom:14px;">
            <input id="r44-ftue-name-input" type="text" maxlength="16" value="${safeText(remembered('hitme.playerName') || 'Nam Siêu Tốc')}" style="flex:1;padding:10px 12px;background:#0d1b2a;border:1.5px solid #ffcd38;border-radius:8px;color:#fff;font-size:14px;font-weight:700;">
            <button type="button" class="live-secondary" data-action="ftue-random-name" style="padding:0 12px;font-size:16px;" title="Ngẫu nhiên">🎲</button>
          </div>
          <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
            <button type="button" class="live-secondary" data-action="ftue-step-1">${isEn ? 'BACK' : 'QUAY LẠI'}</button>
            <button type="button" class="live-primary" data-action="ftue-save-name">${isEn ? 'CONTINUE' : 'TIẾP TỤC'}</button>
          </div>
        </div>
      ` : ftueStep === 3 ? `
        <div>
          <h3 style="color:#ffcd38;margin:0 0 4px;font-size:16px;font-weight:900;">${isEn ? 'CHOOSE YOUR STARTER' : 'CHỌN NHÂN VẬT ĐỒNG HÀNH'}</h3>
          <p style="color:#94a3b8;font-size:12px;margin:0 0 12px;">${isEn ? 'All 7 characters unlocked for Beta. Change anytime!' : 'Cả 7 nhân vật đều đã mở khóa. Bạn có thể đổi lại bất cứ lúc nào!'}</p>
          <div class="choice-grid" style="max-height:220px;overflow-y:auto;margin-bottom:14px;">
            ${catalog.characters.map(c => `
              <button data-action="ftue-select-char" data-id="${c.id}" class="${c.id===state.characterId?'selected':''}" style="border:1.5px solid ${c.id===state.characterId?'#ffcd38':'rgba(255,255,255,0.15)'};background:${c.id===state.characterId?'rgba(255,205,56,0.15)':'rgba(255,255,255,0.05)'};">
                <img src="${c.assets.head}" alt="">
                <span>${safeText(catalogTitle(c))}</span>
              </button>
            `).join('')}
          </div>
          <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
            <button type="button" class="live-secondary" data-action="ftue-step-2">${isEn ? 'BACK' : 'QUAY LẠI'}</button>
            <button type="button" class="live-primary" data-action="ftue-step-4">${isEn ? 'CONTINUE' : 'TIẾP TỤC'}</button>
          </div>
        </div>
      ` : ftueStep === 4 ? `
        <div>
          <h3 style="color:#ffcd38;margin:0 0 4px;font-size:16px;font-weight:900;">${isEn ? 'SOUND & VIBRATION' : 'ÂM THANH & RUNG XÚC GIÁC'}</h3>
          <p style="color:#94a3b8;font-size:12px;margin:0 0 12px;">${isEn ? 'Customize your sound experience.' : 'Điều chỉnh trải nghiệm âm thanh theo ý bạn.'}</p>
          <div style="display:flex;flex-direction:column;gap:10px;margin-bottom:16px;">
            <div>
              <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:4px;font-size:13px;color:#cbd5e1;font-weight:700;">
                <span>${isEn ? 'Music' : 'Nhạc nền'}</span>
                <span class="music-val" style="color:#ffcd38;">${Math.round(state.musicVolume*100)}%</span>
              </div>
              <input type="range" class="r40-slider" data-setting="music" min="0" max="100" value="${Math.round(state.musicVolume*100)}" style="--val-pct:${Math.round(state.musicVolume*100)}%;width:100%;">
            </div>
            <div>
              <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:4px;font-size:13px;color:#cbd5e1;font-weight:700;">
                <span>${isEn ? 'SFX' : 'Hiệu ứng'}</span>
                <span class="sfx-val" style="color:#ffcd38;">${Math.round(state.sfxVolume*100)}%</span>
              </div>
              <input type="range" class="r40-slider" data-setting="sfx" min="0" max="100" value="${Math.round(state.sfxVolume*100)}" style="--val-pct:${Math.round(state.sfxVolume*100)}%;width:100%;">
            </div>
            <div style="display:flex;justify-content:space-between;align-items:center;padding:6px 0;">
              <button type="button" class="live-secondary" data-action="ftue-test-sound" style="font-size:12px;padding:6px 12px;">🔊 ${isEn ? 'Test Pop Sound' : 'Thử tiếng Bốp'}</button>
              <label class="r40-switch"><input type="checkbox" data-setting="haptics" ${state.haptics!==false?'checked':''}><span class="r40-switch-slider"></span></label>
            </div>
          </div>
          <div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;">
            <button type="button" class="live-secondary" data-action="ftue-step-3">${isEn ? 'BACK' : 'QUAY LẠI'}</button>
            <button type="button" class="live-primary" data-action="ftue-step-5">${isEn ? 'CONTINUE' : 'TIẾP TỤC'}</button>
          </div>
        </div>
      ` : `
        <div>
          <h3 style="color:#ffcd38;margin:0 0 6px;font-size:16px;font-weight:900;">${isEn ? 'QUICK GUIDE (3 STEPS)' : 'HƯỚNG DẪN CHƠI (3 BƯỚC)'}</h3>
          <div style="display:flex;flex-direction:column;gap:8px;margin-bottom:14px;">
            <div style="background:rgba(255,255,255,0.06);padding:8px 12px;border-radius:8px;">
              <b style="color:#ffcd38;font-size:13px;display:block;">1. Đặt vị trí & Kéo ngắm</b>
              <small style="color:#cbd5e1;font-size:11.5px;">Chạm vào sân để chọn vị trí đứng, kéo thả để ngắm hướng ném trong 5 giây.</small>
            </div>
            <div style="background:rgba(255,255,255,0.06);padding:8px 12px;border-radius:8px;">
              <b style="color:#38bdf8;font-size:13px;display:block;">2. Ném đồng loạt & Né tránh</b>
              <small style="color:#cbd5e1;font-size:11.5px;">Hết giờ, tất cả cùng ném! Người trúng đòn mất 1 Máu (3 Máu mỗi người).</small>
            </div>
            <div style="background:rgba(255,255,255,0.06);padding:8px 12px;border-radius:8px;">
              <b style="color:#4ade80;font-size:13px;display:block;">3. Tối đa 5 Vòng</b>
              <small style="color:#cbd5e1;font-size:11.5px;">Sống sót cuối cùng hoặc có nhiều Máu nhất sau 5 vòng sẽ chiến thắng!</small>
            </div>
          </div>
          <div style="background:rgba(25,213,216,0.1);border:1px solid #19d5d8;border-radius:8px;padding:8px 10px;margin-bottom:14px;font-size:11.5px;color:#e0f2fe;">
            💡 <b>Mẹo iOS:</b> Bấm Chia sẻ trên Safari → <i>"Thêm vào MH chính"</i> để chơi toàn màn hình không viền!
          </div>
          <button type="button" class="live-primary" data-action="ftue-finish" style="width:100%;min-height:48px;font-weight:900;font-size:15px;">${isEn ? 'ENTER GAME NOW' : 'VÀO GAME NGAY'}</button>
        </div>
      `}
    </div>` : '';

  const back = ['rank','quests','profile'].includes(state.overlay) ? `<button class="modal-back" data-action="close">${icon('chevron','reverse')}${t('back')}</button>` : '';
  return `<div class="modal-shade ${state.overlay==='settings'?'settings-shade':''}"><section class="modal ${state.overlay==='settings'?'settings-sheet':''}" role="dialog" aria-modal="true" aria-label="${title}"><header><h2>${title}</h2><button data-action="close" aria-label="${t('close')}">${icon('close')}</button></header>${list}${join}${settings}${scenarioSettings}${confirmation}${emotes}${rankContent}${questsContent}${shopContent}${profileContent}${ftueContent}${['character','weapon','join','settings','exit-confirm','emote','rank','quests','shop','profile','ftue'].includes(state.overlay)?'':`<p>${state.overlay==='loading'?`${t('loadingGame')} ${state.loadingPercent}%`:t('preparing')}</p>`}${back}</section></div>`;
}
function render() {
  if (legacy) return;
  root.innerHTML = state.screen==='home' ? home()+modal() : state.screen==='match' ? renderLiveMatch(state,catalog)+modal() : state.screen==='result' ? renderLiveResult(state,catalog)+modal() : state.screen==='bot' ? prototype('bot',state,catalog)+modal() : mock ? prototype(state.screen,state,catalog)+modal() : home()+modal();
  if(params.has('debug')&&state.contractErrors.length)root.insertAdjacentHTML('beforeend',`<output class="contract-debug">BRIDGE: ${state.contractErrors.map(safeText).join(', ')}</output>`);
  if (!performance.getEntriesByName('hitme-home-interactive').length) performance.mark('hitme-home-interactive');
  root.classList.toggle('reduced-motion',state.reducedMotion);
  if (params.get('e2e') === '1') {
    window.__hitmeState = {build:state.build,version:state.version,screen:state.screen,selectedCharacterId:state.characterId,selectedWeaponId:state.weaponId,botCount:state.botCount,phase:state.match?.phase,round:state.match?.round,sessionMatches:state.sessionMatches,contractErrors:state.contractErrors,roster:state.roster,weapons:state.weapons};
    window.__hitmeUiDump = () => [...root.querySelectorAll('[data-name]')].map(el => {const r=el.getBoundingClientRect();return {name:el.dataset.name,text:el.textContent.trim(),rect:{x:r.x,y:r.y,w:r.width,h:r.height},interactable:el.tagName==='BUTTON',visible:r.width>0&&r.height>0};});
  }
}
function changeCharacter(direction) {
  const enabled = catalog.characters.filter(c=>c.enabled);
  if (!enabled.length) return;
  const at = Math.max(0,enabled.findIndex(c=>c.id===state.characterId));
  const chosen = enabled[(at+direction+enabled.length)%enabled.length];
  state.characterId=chosen.id;saveSelection();state.slideDirection=direction>0?'next':'previous';
  send('SetCharacter',{id:chosen.id}); render();
  setTimeout(()=>{state.slideDirection='';root.querySelector('.hero')?.classList.remove('slide-next','slide-previous');},250);
}
const confirmActions = new Set(['quick','create','join-submit','practice']);
const backActions = new Set(['close','previous']);
function playButton(action) {
  if (!audio.music) audio.playMusic('bgm_menu');
  const id = action === 'locked' ? 'ui_denied' :
    ['next','previous','select-character'].includes(action) ? 'ui_select_char' :
    ['weapon','select-weapon'].includes(action) ? 'ui_select_weapon' :
    ['rank','quests','home'].includes(action) ? 'ui_tab' :
    backActions.has(action) ? 'ui_back' : confirmActions.has(action) ? 'ui_confirm' : 'ui_tap';
  audio.playSfx(id);
  if (['character','weapon','settings','profile','rank','quests','join','shop'].includes(action)) audio.playSfx('ui_sheet_open');
  if (['close','select-character'].includes(action)) audio.playSfx('ui_sheet_close');
  if (navigator.vibrate) navigator.vibrate(10);
}
let mockClock = null;
function startMockClock() {
  if (mockClock) clearInterval(mockClock);
  const deadline = performance.now() + 5000;
  state.mockTimeLeft=5;state.mockRound=1;state.mockHp=3;state.mockEvent='';state.mockPhase='placing';
  mockClock=setInterval(()=>{
    if (state.screen!=='match'||state.mockPhase!=='placing'){clearInterval(mockClock);mockClock=null;return;}
    state.mockTimeLeft=Math.max(0,(deadline-performance.now())/1000);
    if (state.mockTimeLeft===0){clearInterval(mockClock);mockClock=null;}
    if (!state.overlay) render();
  },250);
}
function seedMatchState() {
  const w = catalog.weapons.find(x => x.id === state.weaponId);
  const wName = w ? catalogTitle(w) : 'Dép Tổ Ong';
  const botPool = ['nu_nang_dong', 'co_ba', 'anh_teen', 'quay_boy', 'cho_vang', 'meo_mun'].filter(id => id !== state.characterId);
  const botList = [];
  const bCount = Math.max(1, Math.min(5, state.botCount || 3));
  for (let i = 0; i < bCount; i++) {
    const bid = botPool[i % botPool.length];
    const bChar = catalog.characters.find(c => c.id === bid);
    const bName = bChar ? catalogTitle(bChar) : `Bot ${i+1}`;
    botList.push({ id: `bot-${i+1}`, name: bName, characterId: bid, slot: i+1, hp: 3, maxHp: 3, alive: true, isLocal: false });
  }
  state.match = {
    type: 'match',
    round: 1,
    maxRounds: 5,
    roundLabel: (document.documentElement.lang==='en'?'ROUND':'VÒNG') + ' 1',
    phase: 'placing',
    timeLeft: 5.0,
    aliveCount: botList.length + 1,
    weaponName: wName,
    toast: '',
    players: [
      { id: 'player', name: (document.documentElement.lang==='en'?'YOU':'BẠN'), characterId: state.characterId, slot: 0, hp: 3, maxHp: 3, alive: true, isLocal: true },
      ...botList
    ]
  };
}
root.addEventListener('click', event => {
  const button=event.target.closest('[data-action]'); if (!button) return;
  const action=button.dataset.action;
  playButton(action);
  if(action==='online-locked'){state.toasts=[{message:t('comingSoon')}];render();return;}
  if(action==='start-scenario' && params.get('dev')==='1' && !mock){
    if(!state.ready){state.overlay='loading';return render();}
    if(send('StartScenario',{name:button.dataset.scenario})){
      state.overlay=null;state.screen='match';state.practiceActive=true;state.result=null;state.tutorialStep=-1;
      return render();
    }
    return;
  }
  if(action==='factory-reset'){try{localStorage.clear();sessionStorage.clear();}catch(e){}window.location.reload();return;}
  if(action==='tutorial-next'||action==='tutorial-skip') {state.tutorialStep=action==='tutorial-skip'||state.tutorialStep>=2?-1:state.tutorialStep+1;if(state.tutorialStep<0)remember('hitme.tutorialSeen','1');return render();}
  if(action==='tutorial-reopen'){state.tutorialStep=0;state.overlay=null;return render();}
  if(!mock&&action==='pause'){send('Pause');state.paused=true;return render();}
  if(!mock&&action==='resume'){send('Resume');state.paused=false;state.overlay=null;return render();}
  if(!mock&&action==='exit-confirm'){state.overlay='exit-confirm';return render();}
  if(!mock&&action==='exit-match'){send('ExitMatch');state.overlay=null;state.screen='home';state.paused=false;state.practiceActive=false;return render();}
  if(!mock&&action==='rematch'){seedMatchState();send('Rematch');state.result=null;state.screen='match';state.tutorialStep=-1;return render();}
  if(!mock&&action==='ready'){if(state.match?.phase?.toLowerCase()==='placing'&&!state.readyLocked){state.readyLocked=send('Ready');render();}return;}
  if(!mock&&action==='emote'){if(Date.now()<state.emoteCooldownUntil)return;state.overlay='emote';return render();}
  if(!mock&&action==='emote-tab'){state.emoteTab=button.dataset.tab;return render();}
  if(!mock&&action==='send-live-emote'){if(Date.now()<state.emoteCooldownUntil)return;const id=button.dataset.id,kind=button.dataset.kind;state.emoteCooldownUntil=Date.now()+3000;showEmoteBubble('player',kind==='phrase'?(document.documentElement.lang==='en'?['Hello everyone!','Can you dodge?','Well done!','Play again!','Oops!','GG']:['Chào cả nhà!','Né được không?','Giỏi lắm!','Chơi lại nhé!','Ối!','GG'])[Number(id.slice(-2))-1]:'Sticker');send('SendEmote',{kind,id});state.overlay=null;return render();}
  if(!mock&&action==='practice'){state.screen='bot';return render();}
  if(!mock&&action==='start-practice'){if(!state.ready){state.overlay='loading';return render();}seedMatchState();send('StartPractice',{characterId:state.characterId,weaponId:state.weaponId,botCount:state.botCount});state.practiceActive=true;state.screen='match';state.result=null;if(remembered('hitme.tutorialSeen')!=='1')state.tutorialStep=0;return render();}
  if (action==='set-lang-vi') {document.documentElement.lang='vi';try{localStorage.setItem('hitme_lang','vi')}catch(e){}return render();}
  if (action==='set-lang-en') {document.documentElement.lang='en';try{localStorage.setItem('hitme_lang','en')}catch(e){}return render();}
  if (action==='copy-ver') {try{navigator.clipboard?.writeText('0.1.0-beta.12 · 5ef2718');button.textContent=(document.documentElement.lang==='en'?'Copied!':'Đã chép!');setTimeout(()=>{button.textContent=(document.documentElement.lang==='en'?'Copy':'Sao chép')},1500)}catch(e){}return;}
  if (action==='open-ftue') {state.overlay='ftue';state.ftueStep=1;return render();}
  if (action==='ftue-step-1') {state.ftueStep=1;return render();}
  if (action==='ftue-step-2') {audio.playSfx('ui_confirm');state.ftueStep=2;return render();}
  if (action==='ftue-random-name') {
    const pool=['Nam Siêu Tốc','Bé Bự','Cao Thủ Dép','Chảo Thần Sầu','Ninja Làng','Vô Địch Sân Đình','Dép Thần Công','Bàn Tay Vàng'];
    const rName=pool[Math.floor(Math.random()*pool.length)];
    const inp=document.getElementById('r44-ftue-name-input');
    if(inp)inp.value=rName;
    return;
  }
  if (action==='ftue-save-name') {
    const inp=document.getElementById('r44-ftue-name-input');
    const name=inp?.value?.trim()||'Nam Siêu Tốc';
    remember('hitme.playerName',name);
    try{const prof=JSON.parse(localStorage.getItem('hitme.localProfile')||'{}');prof.playerName=name;localStorage.setItem('hitme.localProfile',JSON.stringify(prof));}catch(e){}
    state.ftueStep=3;
    return render();
  }
  if (action==='ftue-select-char') {LoadoutStore.setCharacter(button.dataset.id);return render();}
  if (action==='ftue-step-4') {state.ftueStep=4;return render();}
  if (action==='ftue-test-sound') {audio.playSfx('sfx_hit');return;}
  if (action==='ftue-step-5') {state.ftueStep=5;return render();}
  if (action==='ftue-finish') {
    remember('hitme.ftueCompleted','1');
    try {
      const p = {
        name: remembered('hitme.playerName') || 'Nam Siêu Tốc',
        characterId: state.characterId || 'nam_lang',
        weaponId: state.weaponId || 'dep_to_ong',
        customized: true
      };
      localStorage.setItem('hitme_user_profile', JSON.stringify(p));
      localStorage.setItem('hitme.localProfile', JSON.stringify(p));
    } catch(e) {}
    state.overlay=null;
    return render();
  }
  if (action==='locked') {navigator.vibrate?.(30);return;}
  if (action.startsWith('go-')) {if(mock&&state.screen==='lobby'&&action==='go-home')send('LeaveRoom',{});return go(action.slice(3));}
  if (action==='settings-done') return go(state.previousScreen);
  if (action==='bot-minus'||action==='bot-plus') {state.botCount=Math.min(5,Math.max(1,state.botCount+(action==='bot-plus'?1:-1)));return render();}
  if (action==='lobby-ready') {state.lobbyReady=!state.lobbyReady;send('Ready',{ready:state.lobbyReady});return render();}
  if (mock&&(action==='lobby-add'||action==='lobby-remove')) {window.__hitmeMock?.setLobbySize((state.lobby?.players?.length||1)+(action==='lobby-add'?1:-1));return;}
  if (action==='lobby-practice') return go('bot');
  if (action==='mock-ready') {state.mockEvent='Đã khóa hướng ngắm';return render();}
  if (action==='send-emote') {state.mockEvent=button.dataset.id;go('match');return;}
  if (action==='start-practice') {if(mock){seedMatchState();state.screen='match';send('StartPractice',{characterId:state.characterId,weaponId:state.weaponId,botCount:state.botCount});startMockClock();return render();} }
  if (action==='previous'||action==='next') return changeCharacter(action==='next'?1:-1);
  if (action==='select-character') {LoadoutStore.setCharacter(button.dataset.id);state.overlay=null;return render();}
  if (action==='select-weapon') {LoadoutStore.setWeapon(button.dataset.id);state.overlay=null;return render();}
  if (action==='close') {state.overlay=null;return render();}
  if (mock && action==='settings' && state.screen!=='home') {state.overlay='settings';return render();}
  if (mock && action==='emote') return go('emote');
  if (mock && action==='practice') return go('bot');
  if (mock && action==='quick') {go('search');setTimeout(()=>{if(state.screen==='search')send('QuickPlay',{characterId:state.characterId,weaponId:state.weaponId})},650);return;}
  if (mock && action==='create') {send('CreateRoom',{characterId:state.characterId,weaponId:state.weaponId});return;}
  if (mock && action==='join') {state.overlay='join';return render();}
  if (mock && action==='join-submit') {const code=document.getElementById('room-code')?.value.trim().toUpperCase();if(!/^HM[A-Z0-9]{4}$/.test(code||'')){state.overlay=null;return go('error')}state.overlay=null;send('JoinRoom',{code,characterId:state.characterId,weaponId:state.weaponId});return;}
  if (['character','weapon','settings','profile','rank','quests','join','shop'].includes(action)) {state.overlay=action;return render();}
  if (['quick','create','practice'].includes(action) && !state.ready) {state.overlay='loading';return render();}
  if (action==='quick') send('QuickPlay',{characterId:state.characterId,weaponId:state.weaponId});
  if (action==='create') send('CreateRoom',{characterId:state.characterId,weaponId:state.weaponId});
  if (action==='practice') send('StartPractice',{characterId:state.characterId,weaponId:state.weaponId,botCount:state.botCount});
  if (action==='join-submit') send('JoinRoom',{code:document.getElementById('room-code').value.trim().toUpperCase(),characterId:state.characterId,weaponId:state.weaponId});
});
root.addEventListener('input', event => {
  const setting = event.target.dataset?.setting;
  if (!['music','sfx'].includes(setting)) return;
  const value = Math.max(0,Math.min(1,Number(event.target.value)/100));
  if (setting==='music') state.musicVolume=value; else state.sfxVolume=value;
  event.target.style.setProperty('--val-pct', `${Math.round(value*100)}%`);
  const valSpan = event.target.parentElement?.querySelector(`.${setting}-val`);
  if (valSpan) valSpan.textContent = `${Math.round(value*100)}%`;
  audio.setVolumes({music:state.musicVolume,sfx:state.sfxVolume});
  if(!mock)send('SetSettings',{musicVolume:state.musicVolume,sfxVolume:state.sfxVolume,reducedMotion:state.reducedMotion});
});
root.addEventListener('change', event => {
  const setting = event.target.dataset?.setting;
  if (setting === 'fps') {
    const checked = event.target.checked;
    try { localStorage.setItem('hitme.showFps', checked ? '1' : '0'); } catch(e) {}
    const fpsEl = document.getElementById('r38-fps-overlay');
    if (fpsEl) fpsEl.style.display = checked ? 'block' : 'none';
  }
  if (setting === 'haptics') {
    state.haptics = event.target.checked;
    try { localStorage.setItem('hitme_haptics', event.target.checked ? '1' : '0'); } catch(e) {}
  }
  if (setting === 'reduced-motion') {
    state.reducedMotion = event.target.checked;
    document.body.classList.toggle('r38-reduced-motion', state.reducedMotion);
    if(!mock)send('SetSettings',{musicVolume:state.musicVolume,sfxVolume:state.sfxVolume,reducedMotion:state.reducedMotion});
  }
});
onUnity(event => {
  const invalid=validateLiveEvent(event);if(invalid){contractError(invalid);return;}
  if (event.type==='sfx') {audio.playSfx(event.id,event.vol ?? 1,event.pan ?? 0);return;}
  if (event.type==='music') {audio.playMusic(event.id,event.action);return;}
  if (event.type==='ready') {state.roster=event.roster||[];state.weapons=event.weapons||[];state.version=event.version||'';state.build=event.build||'';state.ready=true;if(state.overlay==='loading')state.overlay=null;if(!mock)sendLayout();}
  if (event.type==='loading') {state.loadingPercent=Math.max(0,Math.min(100,Number(event.percent)||0));}
  if (event.type==='screen' && !(mock && params.get('scene') && event.name==='home' && state.screen===params.get('scene'))) {state.screen=event.name;if(!mock&&event.name==='result'){state.sessionMatches++;state.overlay=null;state.tutorialStep=-1;clearPhaseToasts();}if(!mock&&event.name==='match'){state.roundEvents=[];state.currentRound=0;}}
  if (event.type==='selection' && event.characterId) state.characterId=assetId(event.characterId);
  if (event.type==='selection' && event.weaponId) state.weaponId=assetId(event.weaponId);
  if (event.type==='profile' || event.type==='balance') state.balance=Number.isFinite(Number(event.balance))?Number(event.balance):null;
  if (event.type==='lobby') {state.lobby=event;state.lobbyReady=!!event.players?.find(p=>p.id==='local')?.ready;}
  if (event.type==='match') {const endingPhase=['resolve','roundend','round_result','matchend','match_end'].includes(String(event.phase).toLowerCase());if(!mock && state.match?.phase!==event.phase && endingPhase)clearPhaseToasts();state.match=event;if(mock){state.mockPhase=event.phase;state.mockRound=event.round||1;state.mockEvent=event.toast||'';}else{if(state.currentRound!==event.round){state.currentRound=event.round;state.roundEvents=[];state.readyLocked=false;}if(event.toast&&!endingPhase)queueToast(event.toast);}if(state.overlay)return;if(state.screen==='match'&&updateLiveMatchInPlace(root,event,state,catalog))return;}
  if (event.type==='result') {
    state.result=event;
    const local=event.rankings.find(player=>player.id==='player');
    audio.playSfx(local?.rank===1?'win':'lose');
  }
  if(event.type==='layout')state.layout=event;
  if(event.type==='error'){if(mock){state.screen='error';state.lobbyError=event.message||event.code||'Không thể vào phòng';}queueToast(event.message||event.code||'Lỗi');audio.playSfx('ui_error');}
  if(event.type==='emote'&&!mock){const phrases=document.documentElement.lang==='en'?['Hello everyone!','Can you dodge?','Well done!','Play again!','Oops!','GG']:['Chào cả nhà!','Né được không?','Giỏi lắm!','Chơi lại nhé!','Ối!','GG'];showEmoteBubble(event.playerId||'player',event.kind==='phrase'?(phrases[Number(String(event.id).slice(-2))-1]||'GG'):'Sticker');audio.playSfx('emo_pop');}
  if(event.type==='event'&&!mock){const names={hit:'Trúng đòn!',ko:'Bị loại!',win:'Chiến thắng!'};const message=names[event.kind];if(message){queueToast(message);if(['hit','ko'].includes(event.kind)){state.roundEvents.push(message);state.roundEvents=state.roundEvents.slice(-3);}}audio.playSfx({hit:'hit_dep',ko:'ko',win:'win',throw:'throw_dep',round_result:'round_result'}[event.kind]||'ui_tap');}
  if (event.type==='event' && mock) {state.mockEvent={hit:'Trúng đòn! Tim giảm',ko:'BOT 1 bị loại',win:'Chiến thắng!' }[event.kind]||event.kind;if(event.kind==='hit')state.mockHp=2;audio.playSfx({hit:'hit_dep',ko:'ko',win:'win'}[event.kind]||'ui_tap');}
  render();
});
if (mock) {state.balance=1250;installMockUnity(catalog);} else render();
