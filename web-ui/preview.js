/* Static review scenarios. They are mock screens and never send gameplay commands. */
const links = ['home','bot','lobby','match','result','settings','error'];
const href = scene => `?mock=1&scene=${scene}`;
const navigation = () => `<nav class="preview-navigation" aria-label="Kịch bản xem trước">${links.map(name => `<a href="${href(name)}">${name}</a>`).join('')}</nav>`;
const header = title => `<header class="preview-header"><a href="${href('home')}" aria-label="Về Home">‹</a><h1>${title}</h1></header>`;
const roster = [
  ['nam_lang','Nam Làng'],['nu_nang_dong','Nữ Năng Động'],['co_ba','Cô Ba'],['anh_teen','Anh Teen'],
  ['quay_boy','Quậy Boy'],['cho_vang','Chó Vàng'],['meo_mun','Mèo Mun']
];
const bot = () => `<main class="stage preview" data-screen="bot">${header('LUYỆN VỚI BOT')}
  <div class="preview-scroll"><p>Chọn nhân vật</p><div class="preview-roster">${roster.map(([id,name],i) => `<div class="preview-character ${i>3?'locked':''}">${i>3?`<span class="preview-avatar-fallback" aria-hidden="true">${name[0]}</span>`:`<img src="./assets/${id}_head.png" alt="">`}<b>${name}</b><small>${i>3?'SẮP CÓ':'SẴN SÀNG'}</small></div>`).join('')}</div>
  <p>Vũ khí: Dép Tổ Ong</p><div class="preview-weapon"><img src="./assets/dep_to_ong_icon.png" alt="">Dép Tổ Ong</div>
  <p>4 người (1 Bạn + 3 Bot)</p></div><footer class="preview-footer"><a class="preview-primary" href="${href('match')}">BẮT ĐẦU</a></footer>${navigation()}</main>`;
const lobby = () => `<main class="stage preview" data-screen="lobby">${header('PHÒNG HM4826')}
  <div class="preview-scroll"><p>4 / 6 người • Sân làng</p><div class="preview-list">${['Bạn · Nam Làng','Người chơi 2 · Cô Ba','Người chơi 3 · Anh Teen','Người chơi 4 · Nữ Năng Động'].map((name,i)=>`<div><span>${name}</span><b>${i<3?'SẴN SÀNG':'CHƯA SẴN SÀNG'}</b></div>`).join('')}</div><p>Tất cả sẵn sàng thì trận tự bắt đầu.</p></div>
  <footer class="preview-footer"><a class="preview-primary" href="${href('match')}">SẴN SÀNG</a></footer>${navigation()}</main>`;
const match = () => `<main class="stage preview preview-match" data-screen="match"><div class="match-top"><span>VÒNG 1</span><strong>5 GIÂY</strong><span>ẨN KHI NGẮM</span></div>
  <div class="match-arena"><img src="./assets/nam_lang_portrait.png" alt="Nam Làng"></div>
  <div class="match-bottom"><span>VŨ KHÍ: DÉP TỔ ONG</span><span>☺ BIỂU CẢM</span></div>${navigation()}</main>`;
const result = () => `<main class="stage preview" data-screen="result">${header('KẾT QUẢ TRẬN')}<div class="preview-scroll"><h2>CHIẾN THẮNG!</h2><div class="preview-list">${['Nam Làng','Cô Ba','Anh Teen','Nữ Năng Động'].map((name,i)=>`<div><span>${i+1}. ${name}</span><b>${i===0?'HẠNG 1':'ĐÃ LOẠI'}</b></div>`).join('')}</div></div><footer class="preview-footer"><a class="preview-primary" href="${href('home')}">CHƠI LẠI</a></footer>${navigation()}</main>`;
const settings = () => `<main class="stage preview" data-screen="settings">${header('CÀI ĐẶT')}<div class="preview-scroll"><div class="preview-list"><div>Âm nhạc <b>BẬT</b></div><div>Hiệu ứng âm thanh <b>BẬT</b></div><div>Giảm chuyển động <b>TẮT</b></div></div></div>${navigation()}</main>`;
const error = () => `<main class="stage preview" data-screen="error">${header('LỖI KẾT NỐI')}<div class="preview-scroll"><h2>Không thể vào phòng</h2><p>Vui lòng kiểm tra mạng và thử lại.</p></div><footer class="preview-footer"><a class="preview-primary" href="${href('home')}">VỀ TRANG CHỦ</a></footer>${navigation()}</main>`;
const scenes = {bot,lobby,match,result,settings,error};
export function previewScene(name) { return (scenes[name] || scenes.error)(); }
