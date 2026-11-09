// Local UI preview only. Excluded when the Unity loader supplies unityInstance.
const roster = [
  {id:'nam_lang',name:'Nam Làng',enabled:true},
  {id:'nu_nang_dong',name:'Nữ Năng Động',enabled:true},
  {id:'co_ba',name:'Cô Ba',enabled:true},
  {id:'anh_teen',name:'Anh Teen',enabled:true},
  {id:'quay_boy',name:'Quậy Boy',enabled:false},
  {id:'cho_vang',name:'Chó Vàng',enabled:false},
  {id:'meo_mun',name:'Mèo Mun',enabled:false}
];
const weapons = [{id:'dep_to_ong',name:'Dép Tổ Ong',enabled:true}];
const emit = event => window.hitmeUi.onUnity(event);
export function installMockUnity() {
  let screen = 'home';
  let selectedCharacterId = 'nam_lang';
  const players = [{id:'local',name:'Nam Làng',characterId:'nam_lang',slot:0,hp:3,maxHp:3,alive:true,isLocal:true}];
  window.unityInstance = { SendMessage(object, method, json) {
    if (object !== 'WebBridge') return;
    const payload = JSON.parse(json);
    if (method === 'SetCharacter') {
      const choice = roster.find(item => item.id === payload.id);
      if (choice?.enabled) { selectedCharacterId = choice.id; emit({type:'selection',characterId:choice.id}); }
      else emit({type:'error',code:'character_locked',message:'Nhân vật sắp ra mắt'});
    }
    if (method === 'StartPractice') {
      screen='match'; emit({type:'screen',name:screen});
      emit({type:'match',round:1,maxRounds:5,phase:'placing',timeLeft:5,players,weaponName:'Dép Tổ Ong',playableRect:{x:28,y:230,w:337,h:500},toast:''});
      setTimeout(() => emit({type:'event',kind:'hit',sourceId:'local',targetId:'bot_1'}),1200);
      setTimeout(() => emit({type:'event',kind:'ko',sourceId:'local',targetId:'bot_1'}),1800);
      setTimeout(() => emit({type:'event',kind:'win',sourceId:'local'}),2400);
      setTimeout(() => {screen='result';emit({type:'result',rankings:[{id:'local',name:'Nam Làng',rank:1,hits:1,survived:true}],round:1});},3000);
    }
    if (method === 'CreateRoom' || method === 'JoinRoom') { screen='lobby';emit({type:'screen',name:screen});emit({type:'lobby',code:'HM4826',players:[{id:'local',name:'Nam Làng',characterId:selectedCharacterId,slot:0,ready:false,isHost:true,isLocal:true}],canStart:false,hint:'Cần ít nhất 2 người'}); }
    if (method === 'QuickPlay') emit({type:'loading',percent:25,status:'Đang tìm phòng'});
    if (method === 'LeaveRoom' || method === 'ExitMatch') {screen='home';emit({type:'screen',name:screen});}
    if (method === 'Ready') emit({type:'lobby',code:'HM4826',players:[{id:'local',name:'Nam Làng',characterId:selectedCharacterId,slot:0,ready:!!payload.ready,isHost:true,isLocal:true}],canStart:false,hint:'Cần ít nhất 2 người'});
  }};
  queueMicrotask(() => {emit({type:'ready',build:'mock',version:'0.1.0',roster,weapons});emit({type:'screen',name:screen});});
}
