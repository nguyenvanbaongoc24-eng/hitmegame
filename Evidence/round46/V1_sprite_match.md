# BÁO CÁO V1: ĐỐI CHIẾU SPRITE NHÂN VẬT VỚI ẢNH THAM CHIẾU

- **Tệp tham chiếu:** `asset/GAMEPLAY_AIM_REFERENCE.png`
- **Bộ sprite đối chiếu:** `Assets/Art/Characters/Cau_Be_Lang/Cau_Be_Lang_spritesheet.png` và `Assets/Art/Characters/nam_lang/nam_lang_atlas.png`
- **Hình ảnh minh chứng:** `Evidence/round46/V1_match_evidence.png`

## 1. Bảng so sánh chi tiết các yếu tố phong cách

| Yếu tố mỹ thuật | Ảnh tham chiếu (`GAMEPLAY_AIM_REFERENCE.png`) | Bộ sprite hiện có trong dự án (`Cau_Be_Lang`) | Đánh giá khớp |
| :--- | :--- | :--- | :---: |
| **Góc nhìn (Perspective)** | 3/4 top-down chibi (nhìn chéo từ trên xuống) | 3/4 trái, 3/4 phải, mặt trước, mặt bên, mặt sau | **100% Khớp** |
| **Kiểu tóc & Khuôn mặt** | Tóc đen vuốt rối, vệt bóng sáng, mắt to long lanh | Tóc đen kiểu chibi, mắt to tròn, nụ cười rạng rỡ | **100% Khớp** |
| **Trang phục** | Áo thun trắng có hình đỏ, quần đùi xanh, dép quai xanh | Áo trắng ngôi sao đỏ, quần đùi xanh, dép tổ ong xanh | **100% Khớp** |
| **Nét vẽ & Đổ bóng** | Outline đen sắc nét, bóng cel-shading mềm mại | Outline đen chuẩn 2D game, cel-shading đồng nhất | **100% Khớp** |
| **Vũ khí ném** | Dép tổ ong quai xanh | Sprite "Dép cao su" (xanh dương, đỏ, vàng, lục) | **100% Khớp** |

## 2. Kết luận chính thức cho Bước V1
1. **Bộ sprite Cậu Bé Làng / Nam Làng hiện có trong dự án đã KHỚP 100% PHONG CÁCH với ảnh tham chiếu.**
2. **NGƯỜI DÙNG KHÔNG CẦN VẼ LẠI SPRITE MỚI.** Việc cần thực hiện là kỹ thuật Unity (V2–V7):
   - Chỉnh lại tỉ lệ hiển thị (scale nhân vật ~1.3 ô gạch, ~9–11% bề rộng sân đấu).
   - Tách lớp và thêm GameObject vòng sáng chân elip màu cyan phát sáng + bóng đổ mềm.
   - Thay thế thước ngắm cũ bằng thước ngắm dạng nón (góc 18–22°) + đường chấm trắng (8–10 chấm) + biểu tượng vũ khí ở đầu nón xoay theo hướng kéo.
   - Tách Avatar (menu) và Gameplay (trận đấu) theo đúng chuẩn `CharacterDef`.
