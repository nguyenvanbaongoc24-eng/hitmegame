const $ = id => document.getElementById(id);
const query = new URLSearchParams(location.search);
const cfg = await fetch('/config.json').then(r => r.json());
const wsUrl = query.get('server') || `${location.protocol === 'https:' ? 'wss:' : 'ws:'}//${location.host}/ws`;
const state = { ws: null, token: '', reconnectToken: '', id: '', room: '', host: '', players: [], config: null,
  phase: 'entry', round: 0, deadline: 0, placed: {x:0,y:0}, aim: {x:0,y:1}, hasAim: false,
  pointer: null, origin: null, gesture: null, revealed: [], hp: new Map(), hits: new Map(), emotes: new Map(), winners: [],
  ready: false, countdownAt: 0, lastCommitAt: 0, lastPing: 0, rtts: [], autoReconnect: false };
const errorVi = { not_enough_players:'Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot.',
  auth_invalid:'Tên/phiên khách không hợp lệ.', protocol_mismatch:'Phiên bản không khớp máy chủ.', room_not_found:'Không tìm thấy phòng.',
  room_full:'Phòng đã đủ người.', room_started:'Trận đã bắt đầu.', banned:'Bạn đang bị cấm thi đấu tạm thời.',
  reconnect_invalid:'Phiên vào lại đã hết hạn.', wrong_phase:'Thao tác sai pha.', round_locked:'Vòng đã khóa.',
  rate_limited:'Bạn thao tác quá nhanh.', invalid_commit:'Vị trí hoặc hướng không hợp lệ.' };
function notice(text) { $('notice').textContent = text; }
function show(name) { for (const id of ['entry','lobby','match','result']) $(id).classList.toggle('hidden', id !== name); }
function safeLog(direction, packet) {
  const copy = JSON.parse(JSON.stringify(packet));
  if (copy.token) copy.token = '[ẨN]';
  if (copy.reconnectToken) copy.reconnectToken = '[ẨN]';
  $('log').textContent += `${new Date().toLocaleTimeString()} ${direction} ${JSON.stringify(copy)}\n`;
  $('log').scrollTop = $('log').scrollHeight;
}
function send(packet) {
  packet.v = 1;
  safeLog('→', packet);
  state.ws?.send(JSON.stringify(packet));
}
function connect(afterOpen) {
  if (state.ws && state.ws.readyState <= 1) state.ws.close();
  const ws = new WebSocket(wsUrl);
  state.ws = ws;
  $('connection').textContent = 'Đang kết nối…';
  ws.onopen = () => { $('connection').textContent = 'Đã kết nối'; afterOpen?.(); };
  ws.onerror = () => notice('Không nối được server. Kiểm tra địa chỉ và Wi-Fi.');
  ws.onclose = e => { if (state.ws !== ws) return; $('connection').textContent = 'Mất kết nối'; notice(e.reason || 'Mất kết nối'); };
  ws.onmessage = e => { let packet; try { packet = JSON.parse(e.data); } catch { return; } safeLog('←', packet); receive(packet); };
}
function identity() {
  const name = ($('name').value.trim() || 'Khách').slice(0,24);
  $('name').value = name;
  state.token = `guest:${name}`;
  sessionStorage.setItem('hitme_guest_token', state.token);
}
function startEntry(kind) {
  identity();
  const code = $('roomCode').value.trim().toUpperCase();
  if (kind === 'join' && !/^[A-HJ-KM-NP-Z2-9]{6}$/.test(code)) return notice('Mã phòng gồm 6 ký tự A–Z/2–9, không có I, L, O.');
  connect(() => send(kind === 'quick' ? {type:'quick_match',token:state.token} :
    {type:'join',token:state.token,roomCode:kind==='join'?code:''}));
}
function receive(packet) {
  if (packet.type === 'error') return notice((errorVi[packet.code] || `Lỗi: ${packet.code}`) + (packet.remainingSeconds ? ` (${packet.remainingSeconds} giây)` : ''));
  if (packet.type === 'quick_match_waiting') { show('entry'); notice('Đang ghép nhanh với người thật…'); return; }
  if (packet.type === 'quick_match_hint') { notice('Chưa tìm thấy đối thủ. Có thể sang Luyện với bot; hệ thống không tự thêm bot.'); return; }
  if (packet.type === 'quick_match_left') { show('entry'); notice('Đã rời hàng chờ.'); return; }
  if (packet.type === 'room_state') {
    state.room = packet.roomCode;
    state.host = packet.hostId;
    state.config = packet.config;
    state.players = packet.players || [];
    if (packet.reconnectToken) { state.reconnectToken = packet.reconnectToken; sessionStorage.setItem('hitme_reconnect', packet.reconnectToken); }
    const me = state.players.find(p => p.name === state.token.slice(6));
    if (me) state.id = me.id;
    if (!packet.started && state.phase !== 'placing') { state.phase='lobby'; show('lobby'); renderLobby(); }
    renderCards(); return;
  }
  if (packet.type === 'round_start') {
    state.phase='placing'; state.round=packet.roundId; state.deadline=packet.deadline;
    state.placed={x:0,y:0}; state.aim={x:0,y:1}; state.hasAim=false; state.revealed=[]; state.emotes.clear(); state.pointer=null; state.lastCommitAt=0;
    show('match'); $('phase').textContent=`Vòng ${state.round} · Đặt vị trí và ngắm`; $('roundSummary').textContent='';
    sendCommit(); renderCards(); draw(); return;
  }
  if (packet.type === 'reveal') { state.phase='reveal'; state.revealed=packet.players||[]; $('phase').textContent='Lộ diện · Ném đồng thời'; draw(); return; }
  if (packet.type === 'round_result') {
    state.phase='result_round';
    for (const p of packet.players||[]) state.hp.set(p.id,p.hp);
    for (const hit of packet.hits||[]) state.hits.set(hit.shooterId,(state.hits.get(hit.shooterId)||0)+1);
    $('phase').textContent=`Kết quả vòng ${packet.roundId}`;
    $('roundSummary').textContent=`${(packet.hits||[]).length} lần trúng · HP đã cập nhật`;
    renderCards(); draw(); return;
  }
  if (packet.type === 'match_end') {
    state.phase='match_end'; state.winners=packet.winners||[]; show('result'); renderRanking(); return;
  }
  if (packet.type === 'emote') {
    state.emotes.set(packet.playerId,packet.emoteId);
    renderCards();
    if (state.phase !== 'placing') draw();
  }
}
function renderLobby() {
  $('lobbyCode').textContent=state.room;
  $('rosterCount').textContent=`${state.players.length}/${state.config?.maxPlayers||6} người thật`;
  $('lobbyPlayers').replaceChildren(...Array.from({length:state.config?.maxPlayers||6},(_,i)=>{
    const el=document.createElement('div'); el.className='card';
    el.textContent=state.players[i] ? `${state.players[i].name} · ${state.players[i].ready?'Sẵn sàng':'Chưa sẵn sàng'}` : 'Đang chờ…'; return el;
  }));
  const enough=state.players.length >= (state.config?.minPlayersToStart||2);
  $('lobbyHint').textContent=enough?'Tất cả Sẵn sàng sẽ bắt đầu sau 3 giây; chủ phòng có thể bắt đầu sớm.':errorVi.not_enough_players;
  $('hostStart').disabled=!enough||state.host!==state.id;
  $('roomReady').textContent=state.players.find(p=>p.id===state.id)?.ready?'Bỏ sẵn sàng':'Sẵn sàng';
  if (enough && state.players.every(p=>p.ready)) { if (!state.countdownAt) state.countdownAt=Date.now()+3000; }
  else { state.countdownAt=0; $('countdown').textContent=''; }
}
function renderCards() {
  $('hpCards').replaceChildren(...state.players.map(p=>{
    const el=document.createElement('div'); el.className='card'+(p.id===state.id?' me':'');
    const hp=state.hp.has(p.id)?state.hp.get(p.id):state.config?.startHp||3;
    el.textContent=`${p.name}${p.id===state.id?' (Bạn)':''}`;
    const small=document.createElement('small'); small.textContent=`HP ${hp} · ${state.emotes.get(p.id)||''}`; el.append(small); return el;
  }));
}
function rankRows() {
  const rows=state.players.map(p=>({id:p.id,name:p.name,hp:state.hp.get(p.id)??state.config?.startHp??3,hits:state.hits.get(p.id)||0}));
  rows.sort((a,b)=>b.hp-a.hp||b.hits-a.hits||a.name.localeCompare(b.name));
  let previous=null, rank=0;
  return rows.map((p,i)=>{ const key=`${p.hp}:${p.hits}`; if(key!==previous) rank=i+1; previous=key; return {...p,rank}; });
}
function renderRanking() {
  const winners=state.winners.map(id=>state.players.find(p=>p.id===id)?.name||id);
  $('winner').textContent=`Người thắng: ${winners.join(', ')||'Hòa'}`;
  $('ranking').replaceChildren(...rankRows().map(p=>{
    const row=document.createElement('tr'); for(const value of [p.rank,p.name,p.hp,p.hits]){
      const cell=document.createElement('td'); cell.textContent=String(value); row.append(cell);
    } return row;
  }));
}
function screenPoint(event) {
  const rect=$('arena').getBoundingClientRect();
  return {px:event.clientX-rect.left,py:event.clientY-rect.top,
    x:((event.clientX-rect.left)/rect.width-.5)*(state.config?.arenaWidth||8),
    y:(.5-(event.clientY-rect.top)/rect.height)*(state.config?.arenaHeight||10)};
}
function clamp(point) {
  const x=(state.config?.arenaWidth||8)/2-(state.config?.playerRadius||.4);
  const y=(state.config?.arenaHeight||10)/2-(state.config?.playerRadius||.4);
  return {x:Math.max(-x,Math.min(x,point.x)),y:Math.max(-y,Math.min(y,point.y))};
}
function sendCommit() {
  if(state.phase!=='placing'||!state.ws||state.ws.readyState!==1) return;
  const t=Date.now(); if(t-state.lastCommitAt<70)return; state.lastCommitAt=t;
  send({type:'commit',roundId:state.round,positionX:state.placed.x,positionY:state.placed.y,
    hasAim:state.hasAim,aimX:state.aim.x,aimY:state.aim.y});
}
const arena=$('arena');
arena.addEventListener('pointerdown',event=>{
  if(state.phase!=='placing'||state.pointer!==null)return;
  const point=screenPoint(event); state.pointer=event.pointerId; state.origin=point;
  const centerX=arena.clientWidth*(.5+state.placed.x/(state.config?.arenaWidth||8));
  const centerY=arena.clientHeight*(.5-state.placed.y/(state.config?.arenaHeight||10));
  state.gesture=Math.hypot(point.px-centerX,point.py-centerY)<=28?'aim':'place';
  if(state.gesture==='place'){state.placed=clamp(point);state.aim={x:0,y:1};state.hasAim=false;}
  arena.setPointerCapture(event.pointerId); sendCommit(); draw(); event.preventDefault();
});
arena.addEventListener('pointermove',event=>{
  if(state.phase!=='placing'||event.pointerId!==state.pointer||state.gesture!=='aim')return;
  const point=screenPoint(event);
  const centerX=arena.clientWidth*(.5+state.placed.x/(state.config?.arenaWidth||8));
  const centerY=arena.clientHeight*(.5-state.placed.y/(state.config?.arenaHeight||10));
  const distance=Math.hypot(point.px-centerX,point.py-centerY);
  // Approximately 4 mm at 96 CSS px/inch. No throw force: direction only.
  if(distance<15){state.aim={x:0,y:1};state.hasAim=false;}
  else {const dx=point.x-state.placed.x,dy=point.y-state.placed.y,len=Math.hypot(dx,dy);if(len>0){state.aim={x:dx/len,y:dy/len};state.hasAim=true;}}
  sendCommit(); draw(); event.preventDefault();
});
function pointerEnd(event){if(event.pointerId!==state.pointer)return;state.pointer=null;state.gesture=null;state.lastCommitAt=0;sendCommit();draw();event.preventDefault();}
arena.addEventListener('pointerup',pointerEnd);arena.addEventListener('pointercancel',pointerEnd);
function draw(){
  const ctx=arena.getContext('2d'),w=arena.width,h=arena.height;
  ctx.fillStyle='#d99f67';ctx.fillRect(0,0,w,h);ctx.strokeStyle='#fff8dc';ctx.lineWidth=5;ctx.strokeRect(24,24,w-48,h-48);
  ctx.strokeStyle='#ffffff30';ctx.lineWidth=1;for(let x=60;x<w;x+=60){ctx.beginPath();ctx.moveTo(x,24);ctx.lineTo(x,h-24);ctx.stroke();}for(let y=60;y<h;y+=60){ctx.beginPath();ctx.moveTo(24,y);ctx.lineTo(w-24,y);ctx.stroke();}
  const mark=(p,color,label,aim)=>{const x=w*(.5+p.x/(state.config?.arenaWidth||8)),y=h*(.5-p.y/(state.config?.arenaHeight||10));ctx.fillStyle=color;ctx.beginPath();ctx.arc(x,y,19,0,Math.PI*2);ctx.fill();ctx.fillStyle='#0f172a';ctx.font='bold 13px system-ui';ctx.fillText(label,x-20,y-26);if(aim){ctx.strokeStyle='#00d7e5';ctx.lineWidth=5;ctx.beginPath();ctx.moveTo(x,y);ctx.lineTo(x+aim.x*95,y-aim.y*95);ctx.stroke();}};
  if(state.phase==='placing')mark(state.placed,'#00dce8','BẠN',state.hasAim?state.aim:{x:0,y:1});
  else for(const p of state.revealed){const name=state.players.find(item=>item.id===p.id)?.name||p.id;mark(p.position,p.id===state.id?'#00dce8':'#f8b920',name,p.aim);if(state.emotes.has(p.id)){ctx.fillText(state.emotes.get(p.id),w*(.5+p.position.x/(state.config?.arenaWidth||8)),h*(.5-p.position.y/(state.config?.arenaHeight||10))-45);}}
}
setInterval(()=>{
  if(state.phase==='placing'){
    const left=Math.max(0,Math.ceil((state.deadline-Date.now())/1000));$('timer').textContent=`${left} giây`;
    if(left===0){state.lastCommitAt=0;sendCommit();}
  } else $('timer').textContent='';
  if(state.phase==='lobby'&&state.countdownAt)$('countdown').textContent=`Bắt đầu trong ${Math.max(0,Math.ceil((state.countdownAt-Date.now())/1000))} giây`;
},200);
$('create').onclick=()=>startEntry('create');$('join').onclick=()=>startEntry('join');$('quick').onclick=()=>startEntry('quick');
$('roomReady').onclick=()=>{const me=state.players.find(p=>p.id===state.id);send({type:'room_ready',ready:!me?.ready});};
$('hostStart').onclick=()=>send({type:'start'});
$('leave').onclick=()=>{send({type:'leave'});state.ws.close();state.phase='entry';show('entry');};
$('lockReady').onclick=()=>{if(state.phase!=='placing')return;state.lastCommitAt=0;sendCommit();send({type:'lock_ready',ready:true});notice('Đã khóa sớm; chờ người khác hoặc hết giờ.');};
$('emote').onchange=event=>{if(event.target.value)send({type:'emote',emoteId:event.target.value});event.target.value='';};
$('disconnect').onclick=()=>{const old=state.ws;old?.close();notice('Đang vào lại bằng mã phiên riêng…');setTimeout(()=>connect(()=>send({type:'join',token:state.token,reconnectToken:state.reconnectToken})),250);};
$('toggleLog').onclick=()=>$('log').classList.toggle('hidden');
$('playAgain').onclick=()=>{state.ws?.close();state.phase='entry';state.room='';state.hp.clear();state.hits.clear();show('entry');notice('Tạo phòng mới để đấu lại và chia sẻ mã mới.');};
if(query.get('name'))$('name').value=query.get('name');
if(query.get('room'))$('roomCode').value=query.get('room').toUpperCase();
draw();
