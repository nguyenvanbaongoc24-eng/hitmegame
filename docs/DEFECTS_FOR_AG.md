# Phiếu AG từ selftest đợt 34

| ID | Ảnh WebKit / bằng chứng | Tái hiện | Nghi ngờ nguyên nhân | Tiêu chí chấp nhận |
|---|---|---|---|---|
| R34-02 | [Ảnh trận 2 người](../Evidence/codex_selftest/practice_2_result.png) chỉ làm mốc build; bằng chứng mã: `954257d:Assets/Scripts/UI/WebBridge.cs` (`_lastWeaponId`) và `.../Game/MatchController.cs` (`PlayThrow("dep_to_ong")`). | Chọn Chảo hoặc Vợt trên Home/Cửa hàng, bắt đầu luyện, quan sát vũ khí bay/âm thanh/trúng. | ID vũ khí mới đổi nhãn, chưa truyền xuống đối tượng bay và âm thanh; `ProductionUIController.SelectedWeaponId` vẫn cố định Dép. | Cả 3 lựa chọn cho đúng sprite và SFX trong trận, kết quả trúng vẫn theo Core; test Unity và video WebKit cho từng lựa chọn. |

Các vấn đề tải 0%, `insertBefore` trên Safari, FPS đè hồ sơ, nhiệm vụ trùng và vùng trống online trong prompt chưa tái hiện ở beta.15. AG nên giữ trong backlog kiểm tra; chưa coi đã hết lỗi.
