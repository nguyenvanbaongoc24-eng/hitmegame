/* Fake Unity bridge for /ui-preview/ only. No combat is simulated or scored. */
const emit = event => window.hitmeUi.onUnity(event);
export function installMockUnity(catalog) {
  let characterId = 'nam_lang', weaponId = 'dep_to_ong';
  let generation = 0;
  let roomCode = 'HM4826';
  let players = [];
  const sampleCharacters = catalog.characters.filter(x => x.enabled).map(x => x.id);
  const publishLobby = () => emit({type:'lobby',code:roomCode,maxPlayers:6,players:players.map(x => ({...x}))});
  window.__hitmeMock = {setLobbySize(size) {
    const count = Math.max(1,Math.min(6,Number(size)||1));
    if (!players.length) players=[{id:'local',characterId,ready:false,isHost:true}];
    players = [players[0],...Array.from({length:count-1},(_,i) => players[i+1] || {id:`guest_${i+1}`,name:`Người chơi ${i+2}`,characterId:sampleCharacters[(i+1)%sampleCharacters.length] || characterId,ready:i%2===0,isHost:false})];
    publishLobby();
  }};
  const timed = (delay,fn) => {const captured=generation;setTimeout(()=>{if(captured===generation)fn()},delay)};
  window.unityInstance = {SendMessage(object,method,json) {
    if(object!=='WebBridge') return;
    const p=JSON.parse(json);
    if(method==='SetCharacter') {if(catalog.characters.some(x=>x.id===p.id&&x.enabled)){characterId=p.id;emit({type:'selection',characterId})}else emit({type:'error',code:'character_locked'});}
    if(method==='SetWeapon') {if(catalog.weapons.some(x=>x.id===p.id&&x.enabled)){weaponId=p.id;emit({type:'selection',weaponId})}else emit({type:'error',code:'weapon_locked'});}
    if(method==='CreateRoom'||method==='JoinRoom'||method==='QuickPlay') {
      if(method==='JoinRoom'&&p.code==='HM0000'){emit({type:'error',code:'room_not_found',message:'Không tìm thấy phòng'});return;}
      roomCode=p.code||'HM4826';players=[{id:'local',characterId,ready:false,isHost:method!=='JoinRoom'}];
      window.__hitmeMock.setLobbySize(method==='JoinRoom'?4:1);emit({type:'screen',name:'lobby'});
    }
    if(method==='Ready'&&players.length){players[0].ready=!!p.ready;publishLobby();}
    if(method==='StartPractice') {
      generation++;characterId=p.characterId||characterId;weaponId=p.weaponId||weaponId;
      emit({type:'screen',name:'match'});
      emit({type:'match',round:1,maxRounds:5,phase:'placing',timeLeft:5,players:[{id:'local',characterId,hp:3}]});
      timed(5000,()=>emit({type:'match',round:1,maxRounds:5,phase:'reveal',timeLeft:0,toast:'Tự ném theo hướng cuối'}));
      timed(5750,()=>emit({type:'match',round:1,maxRounds:5,phase:'throwing',timeLeft:0,toast:'Dép đang bay'}));
      timed(6450,()=>emit({type:'event',kind:'hit',targetId:'bot_1'}));
      timed(7050,()=>emit({type:'event',kind:'ko',targetId:'bot_1'}));
      timed(7450,()=>emit({type:'match',round:1,maxRounds:5,phase:'round_result',timeLeft:0,toast:'KẾT QUẢ VÒNG 1 • BOT 1 BỊ LOẠI'}));
      timed(7850,()=>emit({type:'event',kind:'win',sourceId:'local'}));
      timed(8300,()=>{emit({type:'screen',name:'result'});emit({type:'result',round:1,rankings:[{id:'local',rank:1}]})});
    }
    if(method==='ExitMatch'||method==='LeaveRoom'){generation++;players=[];emit({type:'screen',name:'home'})}
  }};
  queueMicrotask(()=>{emit({type:'ready',build:'mock',version:'0.1.0',catalogVersion:catalog.version,roster:catalog.characters.map(x=>({...x,name:x.name.vi})),weapons:catalog.weapons.map(x=>({...x,name:x.name.vi}))});emit({type:'screen',name:'home'})});
}
