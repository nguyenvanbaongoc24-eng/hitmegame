import {onUnity, send} from './bridge.js';
import {installMockUnity} from './mock-unity.js';
import {audio} from './audio.js';
import {prototype, title as catalogTitle} from './prototype.js';

const copy = await (await fetch('./i18n.json')).json();
const catalog = await (await fetch('./catalog.json')).json();
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
syncViewport();
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
const initialCharacter = remembered('hitme.characterId');
const initialWeapon = remembered('hitme.weaponId');
const state = {screen:mock?(params.get('scene')||'home'):'home',previousScreen:'home',catalog,roster:[],weapons:[],characterId:catalog.characters.some(x=>x.id===initialCharacter&&x.enabled)?initialCharacter:'nam_lang',weaponId:catalog.weapons.some(x=>x.id===initialWeapon&&x.enabled)?initialWeapon:'dep_to_ong',balance:null,version:'',build:'',lobby:null,match:null,result:null,overlay:null,reducedMotion:false,ready:false,loadingPercent:0,slideDirection:'',botCount:3,lobbyReady:false,musicVolume:audio.musicVolume,sfxVolume:audio.sfxVolume,mockRound:1,mockPhase:'placing',mockTimeLeft:5,mockHp:3,mockEvent:''};
const current = () => catalog.characters.find(c => c.id === state.characterId) || catalog.characters[0];
const currentWeapon = () => catalog.weapons.find(w=>w.id===state.weaponId) || catalog.weapons[0];
const characterImage = kind => current().assets[kind];
const saveSelection = () => {remember('hitme.characterId',state.characterId);remember('hitme.weaponId',state.weaponId)};
const go = screen => {state.screen=screen;state.overlay=null;render()};

function home() {
  const character = current();
  return `<main class="stage home" data-screen="home">
    <div class="backdrop" aria-hidden="true"></div>
    <div class="home-bottom-art" aria-hidden="true"></div>
    <header class="profile" data-name="profile">
      <button class="avatar round" data-action="profile" aria-label="${t('profile')}"><img src="${characterImage('head')}" alt=""></button>
      <button class="profile-name" data-action="profile"><strong>${safeText(catalogTitle(character))}</strong><span>${t('profile')} ${icon('edit')}</span></button>
      ${state.balance !== null ? `<button class="gold" data-action="shop" aria-label="Vàng ${state.balance}">${icon('coin')}<b>${Number(state.balance).toLocaleString('vi-VN')}</b><span class="add">${icon('plus')}</span></button>` : ''}
    </header>
    <button class="settings icon-round" data-name="settings" data-action="settings" aria-label="${t('settings')}">${icon('gear')}</button>
    <div class="logo" data-name="logo" aria-label="HIT ME"><img class="logo-image" src="./assets/logo_hit_me.png" alt="HIT ME"></div>
    <div class="pedestal" aria-hidden="true"></div>
    <img class="hero ${state.slideDirection ? `slide-${state.slideDirection}` : ''}" data-name="hero" src="${characterImage('portrait')}" alt="${safeText(catalogTitle(character))}">
    <button class="hero-arrow left icon-round" data-name="arrow-left" data-action="previous" aria-label="Nhân vật trước">${icon('chevron')}</button>
    <button class="hero-arrow right icon-round" data-name="arrow-right" data-action="next" aria-label="Nhân vật tiếp theo">${icon('chevron')}</button>
    <button class="selection-card character-card" data-name="character-card" data-action="character"><img src="${characterImage('head')}" alt=""><span><small>${t('character')}</small><b>${safeText(catalogTitle(character))}</b></span>${icon('chevron')}</button>
    <button class="selection-card weapon-card" data-name="weapon-card" data-action="weapon">${icon('chevron','reverse')}<img src="${safeText(currentWeapon().icon)}" alt=""><span><small>${t('weapon')}</small><b>${safeText(catalogTitle(currentWeapon()))}</b></span>${icon('chevron')}</button>
    <button class="cta quick" data-name="quick-play" data-action="quick">${icon('play')}<span><strong>${t('quickPlay')}</strong><small>${t('quickPlayDetail')}</small></span></button>
    <button class="cta blue" data-name="create-room" data-action="create">${icon('people')}<strong>${t('createRoom')}</strong></button>
    <button class="cta green" data-name="join-room" data-action="join">${icon('people')}<strong>${t('joinRoom')}</strong></button>
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
  const title = {character:t('chooseCharacter'),weapon:t('chooseWeapon'),settings:t('settings'),profile:t('profile'),rank:t('comingSoon'),quests:t('comingSoon'),join:t('joinRoom'),shop:t('comingSoon'),loading:t('loadingGame')}[state.overlay] || 'HIT ME';
  const choices = state.overlay === 'character' ? catalog.characters : state.overlay === 'weapon' ? catalog.weapons : [];
  const list = choices.length ? `<div class="choice-grid">${choices.map(item => `<button data-action="${item.enabled?`select-${state.overlay}`:'locked'}" data-id="${safeText(item.id)}" ${item.enabled?'':'aria-disabled="true" class="locked"'}><img src="${safeText(state.overlay==='character'?item.assets.head:item.icon)}" alt=""><span>${safeText(catalogTitle(item))}</span>${item.enabled?'':`${icon('lock')}<small>${t('comingSoon')}</small>`}</button>`).join('')}</div>` : '';
  const join = state.overlay === 'join' ? `<label>${t('roomCode')} <input id="room-code" maxlength="6" autocomplete="off" placeholder="HM4826"></label><button class="cta quick dialog-action" data-action="join-submit">${t('joinRoom')}</button>` : '';
  const settings = state.overlay === 'settings' ? `<label class="sheet-setting">Nhạc nền <input type="range" data-setting="music" min="0" max="100" value="${Math.round(state.musicVolume*100)}"></label><label class="sheet-setting">Hiệu ứng <input type="range" data-setting="sfx" min="0" max="100" value="${Math.round(state.sfxVolume*100)}"></label>` : '';
  const back = ['rank','quests'].includes(state.overlay) ? `<button class="modal-back" data-action="close">${icon('chevron','reverse')}${t('back')}</button>` : '';
  return `<div class="modal-shade ${state.overlay==='settings'?'settings-shade':''}"><section class="modal ${state.overlay==='settings'?'settings-sheet':''}" role="dialog" aria-modal="true" aria-label="${title}"><header><h2>${title}</h2><button data-action="close" aria-label="${t('close')}">${icon('close')}</button></header>${list}${join}${settings}${['character','weapon','join','settings'].includes(state.overlay)?'':`<p>${state.overlay==='loading'?`${t('loadingGame')} ${state.loadingPercent}%`:t('preparing')}</p>`}${back}</section></div>`;
}
function render() {
  if (legacy) return;
  root.innerHTML = mock && state.screen !== 'home' ? prototype(state.screen,state,catalog) + modal() : home() + modal();
  if (!performance.getEntriesByName('hitme-home-interactive').length) performance.mark('hitme-home-interactive');
  root.classList.toggle('reduced-motion',state.reducedMotion);
  if (params.get('e2e') === '1') {
    window.__hitmeState = {build:state.build,version:state.version,screen:state.screen,selectedCharacterId:state.characterId,selectedWeaponId:state.weaponId,botCount:state.botCount,mockPhase:state.mockPhase,roster:state.roster,weapons:state.weapons};
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
root.addEventListener('click', event => {
  const button=event.target.closest('[data-action]'); if (!button) return;
  const action=button.dataset.action;
  playButton(action);
  if (action==='locked') {navigator.vibrate?.(30);return;}
  if (action.startsWith('go-')) return go(action.slice(3));
  if (action==='settings-done') return go(state.previousScreen);
  if (action==='bot-minus'||action==='bot-plus') {state.botCount=Math.min(5,Math.max(1,state.botCount+(action==='bot-plus'?1:-1)));return render();}
  if (action==='lobby-ready') {state.lobbyReady=!state.lobbyReady;send('Ready',{ready:state.lobbyReady});return render();}
  if (action==='lobby-practice') return go('bot');
  if (action==='mock-ready') {state.mockEvent='Đã khóa hướng ngắm';return render();}
  if (action==='send-emote') {state.mockEvent=button.dataset.id;go('match');return;}
  if (action==='start-practice') {if(mock){send('StartPractice',{characterId:state.characterId,weaponId:state.weaponId,botCount:state.botCount});startMockClock();return;} }
  if (action==='previous'||action==='next') return changeCharacter(action==='next'?1:-1);
  if (action==='select-character') {state.characterId=button.dataset.id;saveSelection();send('SetCharacter',{id:state.characterId});state.overlay=null;return render();}
  if (action==='select-weapon') {state.weaponId=button.dataset.id;saveSelection();send('SetWeapon',{id:state.weaponId});state.overlay=null;return render();}
  if (action==='close') {state.overlay=null;return render();}
  if (mock && action==='settings' && state.screen!=='home') {state.overlay='settings';return render();}
  if (mock && action==='emote') return go('emote');
  if (mock && action==='practice') return go('bot');
  if (mock && action==='quick') {go('search');setTimeout(()=>{if(state.screen==='search'){send('CreateRoom',{characterId:state.characterId,weaponId:state.weaponId});go('lobby')}},650);return;}
  if (mock && action==='create') {send('CreateRoom',{characterId:state.characterId,weaponId:state.weaponId});return go('lobby');}
  if (mock && action==='join') {state.overlay='join';return render();}
  if (mock && action==='join-submit') {const code=document.getElementById('room-code')?.value.trim().toUpperCase();if(!/^HM[A-Z0-9]{4}$/.test(code||'')){state.overlay=null;return go('error')}send('JoinRoom',{code,characterId:state.characterId,weaponId:state.weaponId});return go('lobby');}
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
  audio.setVolumes({music:state.musicVolume,sfx:state.sfxVolume});
});
onUnity(event => {
  if (event.type==='sfx') {audio.playSfx(event.id,event.vol ?? 1,event.pan ?? 0);return;}
  if (event.type==='music') {audio.playMusic(event.id,event.action);return;}
  if (event.type==='ready') {state.roster=event.roster||[];state.weapons=event.weapons||[];state.version=event.version||'';state.build=event.build||'';state.ready=true;if(state.overlay==='loading')state.overlay=null;}
  if (event.type==='loading') {state.loadingPercent=Math.max(0,Math.min(100,Number(event.percent)||0));}
  if (event.type==='screen' && !(mock && params.get('scene') && event.name==='home' && state.screen===params.get('scene'))) state.screen=event.name;
  if (event.type==='selection' && event.characterId) state.characterId=assetId(event.characterId);
  if (event.type==='selection' && event.weaponId) state.weaponId=assetId(event.weaponId);
  if (event.type==='profile' || event.type==='balance') state.balance=Number.isFinite(Number(event.balance))?Number(event.balance):null;
  if (event.type==='lobby') state.lobby=event;
  if (event.type==='match') {state.match=event;if(mock){state.mockPhase=event.phase;state.mockRound=event.round||1;state.mockEvent=event.toast||'';}}
  if (event.type==='result') state.result=event;
  if (event.type==='event' && mock) {state.mockEvent={hit:'Trúng đòn! ♥ giảm',ko:'BOT 1 bị loại',win:'Chiến thắng!' }[event.kind]||event.kind;if(event.kind==='hit')state.mockHp=2;audio.playSfx({hit:'hit_dep',ko:'ko',win:'win'}[event.kind]||'ui_tap');}
  render();
});
if (mock) {state.balance=1250;installMockUnity(catalog);} else render();
