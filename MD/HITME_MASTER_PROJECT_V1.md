# HIT ME — MASTER PROJECT / REBUILD V1.0

Ngày: 27/10/2026 • Trạng thái: master điều phối của bản rebuild, đã đối chiếu cấu trúc local ngày 27/10/2026

## 1. Quyết định nền tảng

- Nguồn chính duy nhất của bản làm lại: `D:\HITME GAME`.
- Tài liệu điều phối chính đặt tại `D:\HITME GAME\MD\HITME_MASTER_PROJECT_V1.md`. Các báo cáo trong thư mục `docs` của repository/worktree chỉ là tài liệu chuyên đề hoặc ảnh chụp trạng thái tại thời điểm viết.
- Project Unity chính đã tồn tại tại `D:\HITME GAME\unity-client`; không tạo thêm project hoặc sao chép toàn bộ project cũ vào đây. Giữ gameplay đã có, chỉ di chuyển/tái sử dụng tài nguyên sau khi kiểm kê và bảo toàn GUID/reference.
- Không xóa bản cũ, không di chuyển hoặc ghi đè dữ liệu chưa được kiểm kê.
- Mobile là thiết bị chính, màn hình dọc; PC dùng cho phát triển và kiểm tra bổ trợ.
- Unity đã xác minh: `6000.6.4f1` (revision `12bfff696524`). Blender đã cài tại `D:\App\BLENDER\blender.exe`; chỉ dùng khi có nhu cầu xử lý tài nguyên thực tế.
- Hướng mỹ thuật: game 2D/2.5D vui nhộn, nhân vật Việt Nam, arena có chiều sâu nhẹ, UI mang cảm giác game mobile hoàn thiện.
- Tham chiếu Clash Royale ở cách phân cấp màn hình, nút chơi nổi bật, thẻ nhân vật/vũ khí và phản hồi tương tác. Đây là định hướng thiết kế, chưa phải bản phân tích ảnh tham chiếu. Không sao chép logo, icon, nhân vật hoặc tài nguyên của game đó; không đưa cơ chế thẻ bài, tháp hay lane vào HIT ME.

## 2. Mục tiêu và nguyên nhân cần làm lại

Bản mới phải giải quyết ba vấn đề trước khi mở rộng: giao diện không khớp mẫu, tài nguyên đưa vào chưa sạch, điều khiển và hướng ném không thống nhất. Thành công được chứng minh bằng bản chạy được, ảnh chụp runtime và kết quả kiểm tra; việc có đủ asset hoặc build không lỗi chưa đủ.

Thứ tự ưu tiên: một màn chơi đẹp và điều khiển đúng → một trận offline hoàn chỉnh → menu và lựa chọn nhân vật/vũ khí → online.

### Phạm vi đầu tiên: vertical slice

Một arena, Nam Làng, một weapon có tài nguyên sạch nhất, một bot, đầy đủ chạm–ngắm–ném–trúng đòn–kết thúc trận. Main Menu và Result đủ để vào chơi lại. Chưa làm tài khoản, cửa hàng, nhiệm vụ, QR, xếp hạng hay multiplayer trong giai đoạn này.

### Phạm vi MVP offline

Bổ sung Mai Hồng, Anh Nón Lá, Anh Núi và các weapon có asset đạt chuẩn; chọn nhân vật/vũ khí, cài đặt, hướng dẫn, luyện tập và trận với bot. Danh sách cuối cùng dựa trên kiểm kê, không cam kết mọi ảnh nguồn đều dùng được.

## 3. Gameplay chuẩn

### 3.1 Cơ chế một ngón tay

1. Chạm tại A trong arena hợp lệ: chân nhân vật đặt ngay tại A.
2. Giữ ngón tay, trượt tới B: nhân vật đứng yên tại A; hướng ném lấy từ A đến B.
3. Thước ngắm cập nhật liên tục, hỗ trợ 360 độ.
4. Thả tay sau khi có hướng hợp lệ: xác nhận ném đúng một lần, không cần nút ném.

Không dùng joystick. Không kéo nhân vật theo ngón tay khi đang ngắm. Không đảo hướng kiểu kéo ná. Khoảng cách trượt không điều chỉnh lực ném trong MVP.

Chạm ngắn chưa vượt ngưỡng chỉ đổi vị trí, không ném; người chơi có thể chạm vị trí mới khi chưa khóa lượt. Nếu kéo trở lại vùng chết quanh A, hướng trở thành không hợp lệ, thước ngắm mờ đi; thả trong vùng này không ném. Ngưỡng khởi điểm đề xuất 12 đơn vị giao diện tham chiếu, phải tinh chỉnh bằng thử nghiệm thiết bị, không dùng số pixel vật lý cố định cho mọi màn hình.

Chỉ ngón tay đầu tiên được sở hữu thao tác. Ngón thứ hai không thay đổi hướng. Trượt ra khỏi arena vẫn tiếp tục ngắm nếu còn nhận được cảm ứng. Touch cancel, mất focus hoặc mở menu hủy thao tác chưa khóa, không tự ném.

Chạm bắt đầu trên UI không được xuyên vào arena. Vùng trang trí ngoài arena không được giả làm vùng chơi. Không cho đặt chân xuyên vật cản; hiển thị điểm không hợp lệ và giữ vị trí trước đó.

### 3.2 Chốt cách xử lý lượt để tránh hai cách hiểu

Bản master chọn **lượt đồng thời, vị trí bí mật**:

- Mỗi lượt có 5 giây chọn vị trí và ngắm, là giá trị cấu hình.
- Trong pha này chỉ thấy nhân vật, đường ngắm và vị trí của chính mình. Không hiển thị dấu vết, bóng hoặc hiệu ứng làm lộ vị trí mới của đối thủ.
- Thả tay hợp lệ chốt lệnh ném và khóa lựa chọn ngay. Hiển thị “Đã sẵn sàng”.
- Khi tất cả đã khóa hoặc hết giờ, reveal vị trí và cho các nhân vật ném đồng thời.
- Vì vậy “thả để ném” là gửi lệnh ném ngay; projectile không được bay trước pha reveal. Đây là lựa chọn thiết kế để bảo toàn luật bí mật. Nếu muốn projectile bay ngay khi thả, phải đổi sang chế độ khác và cập nhật master trước khi code.
- Hết giờ khi đang ngắm hợp lệ: tự khóa hướng đang ngắm. Nếu không có hướng hợp lệ: bỏ lượt, giữ vị trí hợp lệ cuối.
- Mở cài đặt ở MVP offline tạm dừng toàn bộ trận, kể cả bot và đồng hồ. Online sau này không tạm dừng thời gian server.

### 3.3 Vũ khí, sát thương và kết quả

Thông số thử nghiệm ban đầu: 3 HP/người; trúng đòn mất 1 HP; không crit, không tăng chỉ số theo skin. Đưa thông số vào data asset để điều chỉnh, không rải hằng số trong script.

Projectile bay theo hướng đã chốt, dừng khi trúng mục tiêu đầu tiên hoặc vật cản chặn đạn. Không trúng chủ sở hữu lúc sinh. Hết tầm hoặc quá thời gian sống phải được thu hồi. Không xuyên nhiều người trong MVP.

Đánh giá kết quả sau khi toàn bộ projectile của lượt đã kết thúc; không kết thúc sớm khiến cú ném đồng thời còn lại mất hiệu lực. Một người còn sống: thắng. Tất cả bị hạ cùng lượt: hòa. Nhiều người còn sống: lượt mới. Giới hạn thử nghiệm 20 lượt; hết giới hạn so HP, bằng HP thì hòa giữa những người dẫn đầu.

Bot chọn lệnh trước khi biết lệnh người chơi; không đọc hướng/vị trí bí mật để phản ứng. Dùng seed lưu được để tái hiện lỗi.

## 4. Trạng thái và dữ liệu trận

| Trạng thái | Điều khiển | Hiển thị | Điều kiện tiếp theo |
|---|---|---|---|
| Loading | Khóa | Tiến trình hợp lý | Tài nguyên sẵn sàng |
| Countdown | Khóa | Đếm bắt đầu | Hết đếm |
| Planning | Chạm–trượt–thả | Nhân vật mình, thước ngắm, 5s | Đã khóa hết hoặc timeout |
| Reveal | Khóa | Hiện vị trí các bên | Bắt đầu ném |
| Resolving | Khóa | Animation, projectile, hit | Projectile kết thúc hết |
| RoundEnd | Khóa | HP và kết quả lượt | Lượt mới hoặc MatchEnd |
| MatchEnd | Khóa gameplay | Thắng/thua/hòa | Chơi lại hoặc về menu |

Lệnh một người: matchId, roundId, playerId, actionId, vị trí hợp lệ, hướng chuẩn hóa, trạng thái throw/skip. Core offline dùng chung mô hình lệnh với online tương lai. Không đưa dữ liệu trình bày như Sprite hoặc Animator vào luật trận.

Định nghĩa dự kiến: CharacterDefinition, WeaponDefinition, ArenaDefinition, MatchConfig, PlayerLoadout, RoundCommand, MatchResult. ID ổn định, độc lập tên file hiển thị.

## 5. Hướng ném và tọa độ — một nguồn dữ liệu

- Chọn một mặt phẳng gameplay và ghi rõ trong tài liệu kỹ thuật sau khi prototype. Dùng camera đang render để chiếu screen → mặt arena; không trộn tọa độ Canvas với world.
- Hướng gameplay lấy từ hai điểm đã chiếu A và B rồi chuẩn hóa. Đường ngắm render từ cùng hướng này, để hướng hiển thị khớp cử chỉ dù camera có góc nghiêng.
- Input, aim preview, animation facing và projectile nhận cùng một lệnh đã chốt.
- Mỗi weapon có visualAngleOffset riêng cho góc gốc của ảnh; không xoay vector vận tốc để sửa sprite.
- Lật visual bằng renderer hoặc node con. Không dùng scale âm ở node vật lý/projectile.
- Điểm phóng nằm trên đường ngắm và không xuất hiện trong vật cản. Projectile spawn đúng một lần qua cơ chế do gameplay sở hữu.
- Animation chỉ báo thời điểm release qua một giao diện duy nhất; có cơ chế fallback nếu clip thiếu event, nhưng không tạo hai projectile.
- Các frame weapon xoay chỉ thể hiện chuyển động hình ảnh; collider và vận tốc không bị animation điều khiển tùy tiện.
- Kiểm tra liên tục đường đi hoặc cơ chế va chạm tương đương để projectile nhanh không xuyên mục tiêu giữa hai frame.

## 6. Định hướng giao diện

### 6.1 Nguyên tắc mỹ thuật

Bố cục game mobile rõ, nút có khối nhẹ, viền và bóng nhất quán; tránh panel giống dashboard web, gradient tràn lan hoặc nhiều lớp hộp chồng nhau. Bảng màu kế thừa bộ asset final, không tự đặt palette xung đột khi chưa xem nguồn.

Ảnh tham chiếu được dùng để đo bố cục và phong cách; ảnh màn hình hoàn chỉnh không được dán thành nền rồi phủ nút lệch lên trên. Thành phần cần tương tác phải có layout và hit area độc lập. Text dùng chữ thật, không kéo giãn chữ đã dính trong ảnh.

### 6.2 Cấu trúc màn hình

| Màn hình | Trọng tâm | Thành phần chính |
|---|---|---|
| Main Menu | Nhân vật và nút CHƠI | Header gọn, nhân vật trung tâm, chọn chế độ, CTA, điều hướng dưới |
| Bộ sưu tập | Chọn nhân vật/weapon | Thẻ có ảnh sạch, tên, trạng thái chọn; preview lớn |
| Luyện tập | Hiểu điều khiển | Arena, bot, hướng dẫn một câu, thử lại |
| Gameplay | Đọc trận và ngắm | Arena lớn, timer, HP, menu nhỏ, một weapon đang dùng |
| Result | Hiểu kết quả | Thắng/thua/hòa, nhân vật, chơi lại, về menu |
| Settings | Điều chỉnh nhanh | Âm thanh, nhạc, rung, chất lượng; đóng rõ ràng |

Navigation đề xuất ngoài trận: Bộ sưu tập — Chơi — Hồ sơ. Tab chưa có chức năng không xuất hiện ở vertical slice; không dựng tính năng giả. Vào trận ẩn toàn bộ navigation dưới của menu.

### 6.3 Bố cục gameplay

- Header gọn ở trên safe area: người chơi/HP, lượt và timer trung tâm, nút menu ở góc.
- Arena liền mạch, mục tiêu chiếm ít nhất 70% chiều cao usable viewport; đo thực tế ở từng tỉ lệ, không thu nhỏ nhân vật để đạt chỉ tiêu giả.
- Dưới cùng chỉ một ô weapon và trạng thái/hướng dẫn ngắn. Không thêm deck, joystick hoặc nút ném.
- Thước ngắm có độ tương phản tốt, nét chính và đầu chỉ hướng; không dùng hiệu ứng quá sáng che mục tiêu.
- Vùng chạm nút tối thiểu mục tiêu 44–48 đơn vị UI; font chính hỗ trợ đầy đủ tiếng Việt; không dùng chữ quá nhỏ để nhét nội dung.
- Bảo vệ Dynamic Island, notch và home indicator bằng safe area thực tế. Không hard-code riêng iPhone 15.
- Mốc layout đề xuất 390 × 844 đơn vị, sau đó kiểm tra 9:16, 9:19.5, màn hình hẹp và tablet dọc. Đây là khung thiết kế, không phải độ phân giải thiết bị cố định.

### 6.4 Chốt mẫu trước khi nhân rộng

AG hoàn thiện hai màn runtime: Main Menu và Gameplay. Chụp ảnh ở cùng viewport với mẫu, đặt cạnh nhau và ghi chênh lệch về vị trí, tỉ lệ, khoảng trống, màu, font, cropping. Chỉ mở rộng sang các màn khác sau khi hai màn này đạt tiêu chí hình ảnh và được người dùng xem xét. Codex vẫn làm core độc lập trong thời gian đó.

## 7. Asset pipeline

Nguồn final đã xác minh tồn tại tại `D:\HITME GAME\ASSET HITME FINAL`. Trạng thái tồn tại không đồng nghĩa mọi file đã được REVIEWED/INTEGRATED/VERIFIED; agent vẫn phải dùng manifest và QA trước khi đưa vào runtime. Không âm thầm quay sang bản cũ hoặc dùng asset placeholder thay bản final.

Manifest mỗi asset: assetId, sourcePath, loại, nhân vật/weapon liên quan, kích thước, alpha, grid/frameRects, pivot, PPU, hướng gốc, animation mapping, trạng thái QA, outputPath.

Trạng thái: SOURCE → REVIEWED → PROCESSED → INTEGRATED → VERIFIED; lỗi chuyển REJECTED kèm lý do. Một file đã import chưa đồng nghĩa đã VERIFIED.

### Nhân vật

- Nam Làng, Mai Hồng, Anh Nón Lá, Anh Núi là nhóm dự kiến.
- Sheet 20 frame không mặc định là clip liên tục 20 frame: phân loại pose và thứ tự thực tế bằng mắt.
- Frame cùng nhân vật có pivot chân, kích thước và hướng sáng thống nhất; không dính text, ô bên cạnh hoặc nền giả ô caro.
- Idle/aim/throw/hit/defeat chỉ được dựng khi nguồn đủ. Thiếu pose phải ghi rõ, không trộn nhân vật khác.
- Nếu weapon đã vẽ dính trong frame nhân vật, không chồng thêm weapon rời. Cần chiến lược clip kết thúc không còn weapon hoặc lớp tách được; không tự xóa gây hỏng tay.

### Vũ khí

Dép tổ ong, chảo, vợt pickleball ưu tiên theo chất lượng nguồn. Sheet 25 góc cần manifest thứ tự thực tế; không giả định là một vòng xoay đều chỉ vì có lưới 5 × 5. Collider đơn giản, kích thước đọc được trên mobile, cùng định nghĩa điểm phóng.

### Arena và gió

Tách nền tĩnh, mặt đất gameplay, vật cản, trang trí và foreground. Cây/cờ/đèn lồng chỉ chuyển động ở lớp phù hợp. Nếu nền đã chứa cây/cờ, phải xử lý phần nền khi thêm lớp động để tránh nhân đôi.

Chọn vertex shader cho lá/cờ có mesh và mask phù hợp, hoặc pre-render từ Blender khi nguồn cho phép. Vertex phần gốc được khóa, biên độ nhỏ, pha lệch giữa vật thể; collider không đung đưa theo hình. Không làm cả ảnh arena gợn sóng. Tạo Quality Low tắt chuyển động phụ. MVP chỉ cần 2–3 chi tiết chuyển động đẹp và nhẹ.

## 8. Kiến trúc và cấu trúc thư mục thực tế

| Đường dẫn | Vai trò / trạng thái đã xác minh |
|---|---|
| `D:\HITME GAME\MD` | Master, mục lục migration và báo cáo migration |
| `D:\HITME GAME\ASSET HITME FINAL` | Nguồn asset final nguyên bản; không sửa đè |
| `D:\HITME GAME\unity-client` | Repository Unity chính, nhánh `main` |
| `D:\HITME GAME\worktrees\codex-unity-core` | Worktree gameplay/tích hợp của Codex |
| `D:\HITME GAME\worktrees\ag-visual-ui` | Worktree visual/UI của AG |
| `<worktree>\coordination` | Ownership và hợp đồng dữ liệu theo branch |
| `<worktree>\docs` | Báo cáo chuyên đề theo snapshot; không thay thế master này |
| `<worktree>\Builds\WebGL` | Output WebGL cục bộ, không coi là source và không commit output nặng |

Các thư mục root `reference`, `source-assets`, `art-work`, `builds` và `coordination` trong đề xuất cũ chưa tồn tại tại thời điểm migration — **CẦN XÁC MINH** trước khi dùng; không tự tạo chỉ để khớp tài liệu.

Trong Unity: `Assets/HITME/Core`, `Gameplay`, `Presentation`, `UI`, `Data`, `Art`, `Audio`, `Scenes`, `Tests`, `Editor`. Bảo toàn `.meta`; không tạo GUID mới mỗi lần chạy importer. Script Editor không nằm trong runtime assembly.

Tách luật trận thuần dữ liệu khỏi presentation; dùng assembly definition khi có ích. Input gửi lệnh, match xử lý luật, presentation nghe sự kiện. UI không tự tính sát thương. ScriptableObject giữ cấu hình, runtime state không ghi ngược vào asset gốc.

Hiện trạng đã xác minh: Build Settings chỉ bật `Assets/Scenes/MainMatch.unity`; scene này đang là điểm khởi chạy cho menu và gameplay. `ArenaVisual_Staging.unity` và `Assets/HITME/Scenes/VisualPreview.unity` chỉ là staging/preview trong feature worktree, không phải entry scene. Kiến trúc tách Boot/MainMenu/Battle là đề xuất tương lai — **CẦN XÁC MINH** trước khi đổi build order. Project hiện dùng uGUI và Built-in Render Pipeline; chưa có URP package hoặc SRP asset được gán.

## 9. Codex và AG làm song song

Hai agent dùng hai Git worktree riêng. Đường dẫn đã xác minh: Codex ở `D:\HITME GAME\worktrees\codex-unity-core` trên `feature/codex-unity-core`; AG ở `D:\HITME GAME\worktrees\ag-visual-ui` trên `feature/ag-visual-ui`. Không mở cùng một thư mục Unity bằng hai Editor và không cho Unity import worktree còn lại. Commit nền/HEAD có thể thay đổi theo bàn giao; luôn kiểm tra Git trước khi làm và không giả định hai branch vẫn đang cùng HEAD.

| Hạng mục | Chủ sở hữu |
|---|---|
| Gameplay/input/combat/bot/match state | Codex |
| Hợp đồng sự kiện và dữ liệu | Codex, AG đọc và góp yêu cầu |
| Làm sạch asset, animation, visual prefab, UI | AG |
| Scene Battle tích hợp, Boot và build | Codex |
| VisualPreview, shader gió, material | AG |
| Packages và ProjectSettings | Codex; AG gửi yêu cầu thay đổi |
| QA hình ảnh | AG |
| QA logic và kiểm tra sau tích hợp | Codex |

Hợp đồng tối thiểu: PlanningStarted, AimChanged, CommandLocked, RevealStarted, ThrowReleased, DamageApplied, MatchEnded. Mỗi sự kiện mang roundId/playerId cần thiết. AG không tự tạo hệ input/projectile thứ hai. UI presenter được AG sở hữu và chỉ gọi API đã chốt.

Bàn giao theo commit nhỏ: mã commit, file thay đổi, cách tái hiện, ảnh runtime, dependency, giới hạn. Không merge chồng các scene YAML bằng tay khi chưa hiểu thay đổi; chủ scene ghép prefab. Không tự nâng version Unity/package giữa chừng.

## 10. Roadmap và cổng chất lượng

| Mốc | Đầu ra | Điều kiện qua |
|---|---|---|
| M0 — Kiểm kê | Trạng thái repo được ghi nhận, phiên bản công cụ, manifest, worktree, hợp đồng | Biết nguồn nào dùng, bảo toàn thay đổi chưa commit, không có đường dẫn giả |
| M1 — Hai tuyến song song | Codex prototype input; AG Main Menu/Battle preview | Hướng đúng 8 hướng; hình runtime có thay đổi rõ |
| M2 — Vertical slice | Một nhân vật, một weapon, một bot, một trận | Chơi liên tục từ menu đến result, không lỗi chặn |
| M3 — Mobile polish | Safe area, UI final, sprite sạch, gió nhẹ | QA hình ảnh và thử cảm ứng thiết bị đạt |
| M4 — MVP offline | Bộ sưu tập, loadout lưu local, settings, tutorial | Khởi động lại vẫn giữ lựa chọn; tất cả luồng có đường về |
| M5 — Online | Phòng và trận nhiều người | Chỉ bắt đầu sau MVP; có đặc tả server riêng |

Không triển khai đồng thời tất cả màn hình và multiplayer. Mỗi cổng có build, ảnh và checklist kết quả. Nếu thiếu asset, giải quyết hoặc ghi blocker trước khi nhân rộng.

## 11. Online tương lai và lưu dữ liệu

Vertical slice chạy offline không phụ thuộc đăng nhập hoặc mạng. Lưu local settings/loadout bằng cấu trúc có schema version và fallback khi dữ liệu hỏng.

Online cần authority cho thời gian, lệnh, vị trí hợp lệ, sát thương và kết quả. Client không được nhận tọa độ bí mật trước reveal; chỉ ẩn SpriteRenderer là không đủ. Máy chủ kiểm tra command trùng, trễ, sai lượt và ngoài arena. Ngắt mạng/rejoin phải có đặc tả riêng.

Chưa chốt nhà cung cấp backend, Supabase hay WSS ở giai đoạn rebuild. Không coi dịch vụ đồng bộ dữ liệu là tự động giải quyết simulation authoritative. Chọn sau khi MVP offline ổn định và đã kiểm tra khả năng triển khai thực tế.

## 12. Hiệu năng và kiểm tra

Mục tiêu thiết kế: 60 FPS trên thiết bị mobile tham chiếu; mức Low hướng tới ít nhất 30 FPS. Đây là mục tiêu, không phải kết quả đã đo. Profile bản build trên thiết bị thật, ghi model, OS, trình duyệt/runtime và chất lượng.

Dùng pooling projectile/VFX khi tần suất cần; atlas theo nhóm màn hình; tránh tải mọi nhân vật và mọi effect ở Boot. Đo texture memory, frame time, overdraw và thời gian tải; không tối ưu dựa trên phỏng đoán. WebGL hiện build từ `MainMatch` qua menu `Tools/HIT ME/FINAL/Build WebGL` và xuất vào `Builds/WebGL` trong worktree Codex. Các báo cáo cũ dùng `C:\Python314\python.exe -m http.server 8015` tại thư mục output để phục vụ `http://localhost:8015/`; trạng thái runtime phải được kiểm tra lại sau mỗi build — **CẦN XÁC MINH**, không kế thừa PASS từ báo cáo cũ. Vẫn phải test trình duyệt mobile thật, không chỉ giả lập viewport. Native là kênh đóng gói sau, không dựng thêm một giao diện HTML gameplay thứ hai.

### Test chức năng bắt buộc

- 8 hướng chính/chéo, đổi hướng nhiều lần, trở về vùng chết, sát biên và chạm vật cản.
- UI không xuyên touch; ngón thứ hai bị bỏ qua; cancel/mất focus không ném.
- Timeout khi giữ ngón tay; release đúng thời điểm timeout chỉ tạo một command.
- Đường ngắm trùng đường bay; lật sprite không đảo hướng; không spawn hai projectile.
- Không lộ vị trí bí mật bằng bóng, VFX hoặc dữ liệu online.
- Hai người cùng bị hạ vẫn giải quyết hết projectile và cho hòa.
- Bot không phản ứng theo dữ liệu bí mật.
- Chơi lại khởi tạo sạch HP, timer, lệnh và projectile cũ.
- Main Menu → chọn loadout → chơi → result → chơi lại/về menu hoạt động.

### Test hình ảnh bắt buộc

- Không nền caro giả, chữ nguồn, viền thừa, mất tay/chân hoặc dính frame cạnh.
- Không stretching, sai font tiếng Việt, text tràn hoặc nút bị notch che.
- Nhân vật đứng đúng mặt đất; bóng không trôi; foreground che đúng thứ tự.
- Đường ngắm rõ; arena không bị menu bó hẹp.
- Ảnh runtime Main Menu/Battle/Result được đối chiếu với reference cùng viewport.

## 13. Định nghĩa hoàn thành

Một tính năng chỉ hoàn thành khi: có code/asset tích hợp vào đường chạy chính, đã chạy đúng scenario, không lỗi Console liên quan, có bằng chứng phù hợp và nêu rõ giới hạn. “Đã tạo prefab”, “đã sửa script” hoặc “đã compile” không đồng nghĩa người chơi đã thấy thay đổi.

Hình thức báo cáo mỗi mốc: mục tiêu → kết quả thực tế → ảnh/build → kiểm tra → lỗi còn lại → commit. Không khai đã test iPhone nếu chỉ dùng Editor. Không dùng mockup thay ảnh runtime.

## 14. Prompt khởi động cho Codex

> Đọc master này, lấy D:\HITME GAME làm nguồn chính. Kiểm kê dữ liệu hiện có, trạng thái Git và công cụ Unity/Blender; không xóa hoặc ghi đè dữ liệu cũ. Dùng project `unity-client` và đúng worktree đã cấp; không tạo project Unity mới. Ghi phiên bản thực tế, nguồn asset, quyền sở hữu file và hợp đồng sự kiện vào coordination. Thực hiện phần gameplay của M1: một arena, một nhân vật, chạm đặt vị trí, giữ trượt ngắm, thả khóa lệnh; giữ quy tắc planning bí mật và reveal đồng thời trong master. Bạn sở hữu core, bot, scene tích hợp và build. Không tự thiết kế lại toàn bộ mỹ thuật trong lúc AG làm visual. Hoàn thành vertical slice trước khi mở rộng tính năng. Bàn giao bằng build chạy được, test và commit.

## 15. Prompt khởi động cho AG

> Đọc master và tài liệu coordination do Codex tạo. Chỉ làm trong worktree visual được cấp. Kiểm kê bộ ASSET HITME FINAL/HITME FINAL thực có trong nguồn D:\HITME GAME; không giả định đường dẫn hoặc thay bằng asset cũ. Dựng Main Menu và Battle trong VisualPreview bằng đúng asset đã qua QA, lấy tổ chức UI kiểu Clash Royale làm tham chiếu nhưng giữ bản sắc HIT ME. Ưu tiên bố cục và tỉ lệ trước hiệu ứng. Làm sạch sprite, pivot, animation và weapon; tạo một ít chuyển động cây/cờ hợp lý bằng shader hoặc Blender nếu nguồn cho phép. Nhận hướng ngắm và trạng thái từ core Codex, không dựng input/projectile thứ hai. Bàn giao prefab, ảnh runtime đối chiếu, commit và hướng dẫn ghép. Chưa mở rộng các màn còn lại trước khi hai màn mẫu đạt cổng chất lượng.

## 16. Nhật ký quyết định và các điểm cần xác minh

| ID | Quyết định/kiểm tra | Trạng thái |
|---|---|---|
| D01 | Root D:\HITME GAME thay mọi nguồn triển khai trước đó | Đã chốt theo yêu cầu người dùng |
| D02 | Mobile dọc, Unity, phong cách UI tham chiếu Clash Royale | Định hướng đã chốt |
| D03 | Planning bí mật; release chốt lệnh; reveal ném đồng thời | Baseline của master, cần giữ nhất quán |
| D04 | 3 HP, 1 damage, 20 lượt tối đa | Giá trị prototype, tinh chỉnh sau playtest |
| V01 | Unity `6000.6.4f1`; Blender tại `D:\App\BLENDER\blender.exe`; Built-in Render Pipeline, uGUI | Đã xác minh cấu hình local; phiên bản Blender chi tiết kiểm tra lại khi dùng |
| V02 | Asset final tại `D:\HITME GAME\ASSET HITME FINAL` | Đã xác minh đường dẫn; chất lượng/tình trạng tích hợp tiếp tục theo manifest |
| V03 | Hai màn mẫu và bảng màu/font cuối cùng | Xác lập bằng preview runtime |
| V04 | Thiết bị/profile hiệu năng và kênh build đầu | Xác minh ở M2–M3 |
| V05 | `MainMatch.unity` là scene build/entry duy nhất hiện tại; Boot/MainMenu/Battle tách scene | Hiện trạng đã xác minh; phương án tách scene CẦN XÁC MINH |

Tài liệu này kết hợp đặc tả rebuild với các dữ kiện local đã xác minh và ghi rõ ngày kiểm tra. Các báo cáo cũ không tự động chứng minh tính năng vẫn hoàn thành ở HEAD hiện tại. Mọi thay đổi luật, entry scene và ranh giới sở hữu cần cập nhật vào master và nhật ký trước khi hai agent triển khai khác nhau.
