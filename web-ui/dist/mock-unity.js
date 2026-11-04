/* Fake Unity bridge for /ui-preview/ only. No combat is simulated or scored. */
const emit = event => window.hitmeUi.onUnity(event);
export function installMockUnity(catalog) {
  let characterId = 'nam_lang', weaponId = 'dep_to_ong';
  let generation = 0;
  const timed = (delay,fn) => {const captured=generation;setTimeout(()=>{if(captured===generation)fn()},delay)};
  window.unityInstance = {SendMessage(object,method,json) {
    if(object!=='WebBridge') return;
    const p=JSON.parse(json);
    if(method==='SetCharacter') {if(catalog.characters.some(x=>x.id===p.id&&x.enabled)){characterId=p.id;emit({type:'selection',characterId})}else emit({type:'error',code:'character_locked'});}
    if(method==='SetWeapon') {if(catalog.weapons.some(x=>x.id===p.id&&x.enabled)){weaponId=p.id;emit({type:'selection',weaponId})}else emit({type:'error',code:'weapon_locked'});}
    if(method==='CreateRoom'||method==='JoinRoom') emit({type:'lobby',code:p.code||'HM4826',players:[{id:'local',characterId,ready:false,isHost:true}]});
    if(method==='Ready') emit({type:'lobby',code:'HM4826',players:[{id:'local',characterId,ready:!!p.ready,isHost:true}]});
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
    if(method==='ExitMatch'||method==='LeaveRoom'){generation++;emit({type:'screen',name:'home'})}
  }};
  queueMicrotask(()=>{emit({type:'ready',build:'mock',version:'0.1.0',catalogVersion:catalog.version,roster:catalog.characters.map(x=>({...x,name:x.name.vi})),weapons:catalog.weapons.map(x=>({...x,name:x.name.vi}))});emit({type:'screen',name:'home'})});
}
