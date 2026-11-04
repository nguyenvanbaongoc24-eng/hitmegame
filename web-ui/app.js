import {onUnity, send} from './bridge.js';
import {installMockUnity} from './mock-unity.js';

const params = new URLSearchParams(location.search);
const legacy = params.get('ui') === 'unity';
const mock = params.get('mock') === '1';
const root = document.getElementById('ui-root');
const canvas = document.getElementById('unity-canvas');
if (legacy) root.hidden = true;

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
const state = {screen:'home',roster:[],weapons:[],characterId:'nam_lang',weaponId:'dep_to_ong',balance:null,version:'',build:'',lobby:null,match:null,result:null,overlay:null,reducedMotion:false};
const current = () => state.roster.find(c => c.id === state.characterId) || {id:'nam_lang',name:'Nam Làng',enabled:true};
const characterImage = kind => `./assets/${assetId(state.characterId)}_${kind}.png`;

function home() {
  const character = current();
  return `<main class="stage home" data-screen="home">
    <div class="backdrop" aria-hidden="true"></div>
    <header class="profile" data-name="profile">
      <button class="avatar round" data-action="profile" aria-label="Hồ sơ"><img src="${characterImage('head')}" alt=""></button>
      <button class="profile-name" data-action="profile"><strong>${safeText(character.name)}</strong><span>Hồ sơ ${icon('edit')}</span></button>
      ${state.balance !== null ? `<button class="gold" data-action="shop" aria-label="Vàng ${state.balance}">${icon('coin')}<b>${Number(state.balance).toLocaleString('vi-VN')}</b><span class="add">${icon('plus')}</span></button>` : ''}
    </header>
    <button class="settings icon-round" data-name="settings" data-action="settings" aria-label="Cài đặt">${icon('gear')}</button>
    <div class="logo" data-name="logo" aria-label="HIT ME"><span class="logo-leaf leaf-left"></span><span class="logo-board"><span class="logo-shoe"><img src="./assets/dep_to_ong_icon.png" alt=""></span><span class="logo-word">HIT ME</span></span><span class="logo-leaf leaf-right"></span></div>
    <div class="pedestal" aria-hidden="true"></div>
    <img class="hero" data-name="hero" src="${characterImage('portrait')}" alt="${safeText(character.name)}">
    <button class="hero-arrow left icon-round" data-name="arrow-left" data-action="previous" aria-label="Nhân vật trước">${icon('chevron')}</button>
    <button class="hero-arrow right icon-round" data-name="arrow-right" data-action="next" aria-label="Nhân vật tiếp theo">${icon('chevron')}</button>
    <button class="selection-card character-card" data-name="character-card" data-action="character"><img src="${characterImage('head')}" alt=""><span><small>NHÂN VẬT</small><b>${safeText(character.name)}</b></span>${icon('chevron')}</button>
    <button class="selection-card weapon-card" data-name="weapon-card" data-action="weapon">${icon('chevron','reverse')}<img src="./assets/dep_to_ong_icon.png" alt=""><span><small>VŨ KHÍ</small><b>Dép</b></span>${icon('chevron')}</button>
    <button class="cta quick" data-name="quick-play" data-action="quick">${icon('play')}<span><strong>CHƠI NHANH</strong><small>Online • Tối đa 6 người</small></span></button>
    <button class="cta blue" data-name="create-room" data-action="create">${icon('people')}<strong>TẠO PHÒNG</strong></button>
    <button class="cta green" data-name="join-room" data-action="join">${icon('people')}<strong>VÀO PHÒNG</strong></button>
    <button class="cta navy practice" data-name="practice" data-action="practice">${icon('robot')}<strong>LUYỆN VỚI BOT</strong>${icon('chevron')}</button>
    <nav class="tabs" data-name="tabs" aria-label="Điều hướng chính">
      <button class="active" data-action="home">${icon('home')}<span>Trang chủ</span></button>
      <button data-action="rank">${icon('trophy')}<span>Xếp hạng</span></button>
      <button data-action="quests">${icon('gift')}<span>Nhiệm vụ</span></button>
    </nav>
  </main>`;
}
function modal() {
  if (!state.overlay) return '';
  const title = {character:'Chọn nhân vật',weapon:'Chọn vũ khí',settings:'Cài đặt',profile:'Hồ sơ',rank:'Sắp ra mắt',quests:'Sắp ra mắt',join:'Vào phòng',shop:'Sắp ra mắt'}[state.overlay] || 'HIT ME';
  const list = state.overlay === 'character' ? `<div class="choice-grid">${state.roster.map(item => `<button data-action="select-character" data-id="${safeText(item.id)}" ${item.enabled?'':'disabled'}><img src="./assets/${assetId(item.id)}_head.png" alt=""><span>${safeText(item.name)}</span>${item.enabled?'':icon('lock')}</button>`).join('')}</div>` : '';
  const join = state.overlay === 'join' ? `<label>Mã phòng <input id="room-code" maxlength="6" autocomplete="off" placeholder="HM4826"></label><button class="cta quick dialog-action" data-action="join-submit">VÀO PHÒNG</button>` : '';
  return `<div class="modal-shade"><section class="modal" role="dialog" aria-modal="true" aria-label="${title}"><header><h2>${title}</h2><button data-action="close" aria-label="Đóng">${icon('close')}</button></header>${list}${join}${state.overlay==='character'?'':state.overlay==='join'?'':'<p>Đang chuẩn bị cho bản thử.</p>'}</section></div>`;
}
function render() {
  if (legacy) return;
  root.innerHTML = home() + modal();
  root.classList.toggle('reduced-motion',state.reducedMotion);
  if (params.get('e2e') === '1') {
    window.__hitmeState = {build:state.build,version:state.version,screen:state.screen,selectedCharacterId:state.characterId,selectedWeaponId:state.weaponId,roster:state.roster,weapons:state.weapons};
    window.__hitmeUiDump = () => [...root.querySelectorAll('[data-name]')].map(el => {const r=el.getBoundingClientRect();return {name:el.dataset.name,text:el.textContent.trim(),rect:{x:r.x,y:r.y,w:r.width,h:r.height},interactable:el.tagName==='BUTTON',visible:r.width>0&&r.height>0};});
  }
}
function changeCharacter(direction) {
  const enabled = state.roster.filter(c=>c.enabled && ['nam_lang','nu_nang_dong','co_ba','anh_teen'].includes(c.id));
  if (!enabled.length) return;
  const at = Math.max(0,enabled.findIndex(c=>c.id===state.characterId));
  const chosen = enabled[(at+direction+enabled.length)%enabled.length];
  state.characterId=chosen.id; send('SetCharacter',{id:chosen.id}); render();
}
root.addEventListener('click', event => {
  const button=event.target.closest('[data-action]'); if (!button) return;
  const action=button.dataset.action;
  if (action==='previous'||action==='next') return changeCharacter(action==='next'?1:-1);
  if (action==='select-character') {state.characterId=button.dataset.id;send('SetCharacter',{id:state.characterId});state.overlay=null;return render();}
  if (action==='close') {state.overlay=null;return render();}
  if (['character','weapon','settings','profile','rank','quests','join','shop'].includes(action)) {state.overlay=action;return render();}
  if (action==='quick') send('QuickPlay',{characterId:state.characterId,weaponId:state.weaponId});
  if (action==='create') send('CreateRoom',{characterId:state.characterId,weaponId:state.weaponId});
  if (action==='practice') send('StartPractice',{characterId:state.characterId,weaponId:state.weaponId,botCount:5});
  if (action==='join-submit') send('JoinRoom',{code:document.getElementById('room-code').value.trim().toUpperCase(),characterId:state.characterId,weaponId:state.weaponId});
});
onUnity(event => {
  if (event.type==='ready') {state.roster=event.roster||[];state.weapons=event.weapons||[];state.version=event.version||'';state.build=event.build||'';}
  if (event.type==='screen') state.screen=event.name;
  if (event.type==='selection' && event.characterId) state.characterId=assetId(event.characterId);
  if (event.type==='profile' || event.type==='balance') state.balance=Number.isFinite(Number(event.balance))?Number(event.balance):null;
  if (event.type==='lobby') state.lobby=event;
  if (event.type==='match') state.match=event;
  if (event.type==='result') state.result=event;
  render();
});
if (mock) {state.balance=1250;installMockUnity();} else render();
