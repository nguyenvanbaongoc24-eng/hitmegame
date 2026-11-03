# BÁO CÁO G1: PHÂN TÍCH TỶ LỆ CƠ THỂ VÀ CHE CHÂN

- **Tệp tham chiếu:** `asset/GAMEPLAY_AIM_REFERENCE.png`
- **Ảnh phóng to vùng chân:** `Evidence/round46/G1_feet_zoom.png`
- **Ảnh xếp cạnh so sánh tỷ lệ:** `Evidence/round46/G1_proportions_comparison.png`
- **Chuẩn đo lường:** Dung sai $\pm 12\%$ relative so với tỷ lệ đầu/chiều cao tham chiếu ($56.4\% \pm 6.8\% \rightarrow [49.6\%, 63.2\%]$).

---

## 1. Số đo chính xác trên ảnh tham chiếu (`GAMEPLAY_AIM_REFERENCE.png`)

| Chỉ số | Giá trị pixel | Đơn vị ô gạch (Tile = 52.5 px) | Tỷ lệ % so với chiều cao | Ghi chú |
| :--- | :---: | :---: | :---: | :--- |
| **Tổng chiều cao nhân vật ($H_{total}$)** | **78.0 px** | **1.48 tiles** | 100.0% | $\approx 10\%$ bề rộng sân đấu (8 ô gạch) |
| **Chiều rộng thân ($W$)** | **60.0 px** | **1.14 tiles** | 76.9% | Dáng chibi bè ngang, đầu to |
| **Chiều cao đầu ($H_{head}$)** | **44.0 px** | **0.84 tiles** | **56.4%** | Đầu to chiếm hơn một nửa cơ thể |
| **Thân (áo + quần)** | 26.0 px | 0.50 tiles | 33.3% | Ngực và bụng rất ngắn |
| **Chân nhìn thấy ($H_{legs}$)** | **8.0 px** | **0.15 tiles** | **10.3%** | Chân rất ngắn, chỉ thấy phần mũi giày/dép |
| **Đường kính vòng sáng elip** | 69 x 48 px | 1.31 x 0.91 tiles | — | Tỉ lệ elip 3/4 phối cảnh |
| **Độ chìm của bàn chân** | 7.0 px | 0.13 tiles | 9.0% | Bàn chân nằm lọt trong vòng, mũi chân chạm sát đáy trong |

### Phân tích cơ chế che chân từ `G1_feet_zoom.png`:
1. Vòng sáng là hình elip nằm trên sàn. Bàn chân đứng ở tâm elip.
2. Cung sau (nửa trên elip) nằm sau gót chân.
3. Bàn chân rất ngắn (chỉ 8 px). Cung trước (nửa dưới elip) vòng lên phía trước mũi giày/dép và bóng đổ, tạo cảm giác nhân vật đứng lọt thỏm và chìm nhẹ vào trong vòng phát sáng.

---

## 2. Bảng đối chiếu tỷ lệ 7 nhân vật hiện có trong dự án

| STT | Nhân vật | Tỷ lệ đầu hiện có | Lệch so với tham chiếu (56.4%) | Đánh giá (±12%) | Hướng xử lý |
| :---: | :--- | :---: | :---: | :---: | :--- |
| 1 | **Cậu Bé Làng / Nam Làng** | **44.0%** | **-22.0%** | **KHÔNG ĐẠT** | Cần ảnh chibi đầu to (Mục 6 spec) hoặc áp dụng hạ chìm sprite + nửa vòng trước (Cách B). |
| 2 | **Nữ Năng Động (Cô Nàng Thể Thao)** | **43.0%** | **-23.8%** | **KHÔNG ĐẠT** | Lập Asset Request theo Mục 6 spec. |
| 3 | **Quậy Boy (Ngô Bá Khí)** | **43.0%** | **-23.8%** | **KHÔNG ĐẠT** | Lập Asset Request theo Mục 6 spec. |
| 4 | **Cô Ba (Anh Nón Lá)** | **43.0%** | **-23.8%** | **KHÔNG ĐẠT** | Lập Asset Request theo Mục 6 spec. |
| 5 | **Anh Teen (Anh Núi)** | **43.0%** | **-23.8%** | **KHÔNG ĐẠT** | Lập Asset Request theo Mục 6 spec. |
| 6 | **Chú Chó Vàng** | **52.0%** | **-7.8%** | **ĐẠT** | Nằm trong dung sai $\pm 12\%$. Đóng gói khung 160x160 dùng luôn. |
| 7 | **Mèo Mun** | **52.0%** | **-7.8%** | **ĐẠT** | Nằm trong dung sai $\pm 12\%$. Đóng gói khung 160x160 dùng luôn. |

---

## 3. Cập nhật `Assets/Data/GameplayVisualSpec.json`

Đã ghi các thông số tỷ lệ chính thức:
* `character.heightInTiles`: `1.48`
* `character.headRatio`: `0.564`
* `character.legRatioVisible`: `0.103`
* `character.feetSinkRatio`: `0.09`
* `groundRing.frontHalfSortingOffset`: `+1` (phủ trước chân)
* `groundRing.backHalfSortingOffset`: `-1` (nằm sau gót)
