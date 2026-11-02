# HỢP ĐỒNG DỮ LIỆU HỒ SƠ & VÍ TRỰC TUYẾN (PROFILE & ECONOMY CONTRACT)

Tài liệu này xác lập quy chuẩn dữ liệu giữa Antigravity (Unity WebGL Client) và Codex (C# Server / Supabase Backend) cho Phase 4 & 5.

---

## 1. Trạng Thái & Trách Nhiệm Phân Chia

- **Antigravity (Client)**:
  - Cung cấp giao diện `IAccountService` với hai bộ hiện thực: `FakeAccountService` (dùng cho offline / bot practice) và `ServerAccountService` (kết nối máy chủ).
  - Không tự trừ vàng hay tự cấp vật phẩm khi ở chế độ Online; hiển thị trạng thái "Đang tải..." hoặc "Đang xử lý..." cho đến khi Server phản hồi `ack`/`result`.
- **Codex (Server & Database)**:
  - Lưu trữ quyền năng (Authority) đối với số dư Vàng, Kim Cương, Danh sách vật phẩm đã sở hữu và Điểm rank trong Supabase `profiles` & `inventory`.
  - Kiểm tra điều kiện số dư trước khi ghi nhận mua bán.

---

## 2. Giao Thức Trao Đổi (WebSocket Protocol v2 Proposal)

### 2.1 Yêu Cầu & Nhận Dữ Liệu Hồ Sơ (Profile Sync)

**Client gửi khi kết nối phòng hoặc mở trang chủ:**
```json
{
  "v": 2,
  "type": "profile_get",
  "token": "guest:HM-xxxx"
}
```

**Server phản hồi `profile_state`:**
```json
{
  "v": 2,
  "type": "profile_state",
  "playerId": "HM-9582",
  "displayName": "Khách_4826",
  "avatarId": "avatar_default",
  "gold": 500,
  "gems": 20,
  "rankPoints": 150,
  "rankTier": "Bạc",
  "matchesPlayed": 12,
  "winCount": 5,
  "inventory": ["dep_to_ong_trang", "dep_to_ong_bac"],
  "equippedWeapon": "dep_to_ong_bac",
  "quests": [
    { "id": "q_play_3_matches", "progress": 2, "target": 3, "claimed": false },
    { "id": "q_hit_5_times", "progress": 5, "target": 5, "claimed": false },
    { "id": "q_win_1_match", "progress": 1, "target": 1, "claimed": true }
  ]
}
```

---

### 2.2 Mua Vật Phẩm (Buy Item)

**Client gửi yêu cầu mua:**
```json
{
  "v": 2,
  "type": "buy_item",
  "itemId": "chao_chong_dinh"
}
```

**Server phản hồi kết quả:**
```json
{
  "v": 2,
  "type": "buy_item_result",
  "success": true,
  "itemId": "chao_chong_dinh",
  "newGold": 400,
  "error": null
}
```
*(Nếu thất bại: `"success": false, "error": "insufficient_gold" | "already_owned"`)*

---

### 2.3 Trang Bị Vật Phẩm (Equip Weapon)

**Client gửi:**
```json
{
  "v": 2,
  "type": "equip_item",
  "itemId": "chao_chong_dinh"
}
```

**Server phản hồi:**
```json
{
  "v": 2,
  "type": "equip_item_result",
  "success": true,
  "equippedWeapon": "chao_chong_dinh"
}
```

---

### 2.4 Thưởng Sau Trận Đấu (Match Rewards)

Khi trận đấu kết thúc (`match_end`), Server gửi kèm cấu trúc thưởng:
```json
{
  "v": 2,
  "type": "match_rewards",
  "goldEarned": 50,
  "rankPointsDelta": 25,
  "newGold": 550,
  "newRankPoints": 175,
  "newTier": "Bạc"
}
```

---

## 3. Các Trường Chờ Codex Xác Nhận (Pending Codex R5)

| Trường / Sự kiện | Đề xuất từ Antigravity | Trạng thái |
|---|---|---|
| Cấu trúc Quests trong `profile_state` | Danh sách đối tượng có `id`, `progress`, `claimed` | ⏳ Chờ Codex R5 xác nhận bảng Supabase `user_quests` |
| Tên message mua hàng | `buy_item` / `buy_item_result` | ⏳ Chờ Codex đối chiếu RPC Supabase |
| Định danh Avatar | `avatarId` chuỗi ký tự (vd: `avatar_01`) | ⏳ Chờ Codex |
| Tên message đồng bộ hồ sơ | `profile_get` vs `account_sync` | ⏳ Chờ Codex |

Sau khi Codex hoàn tất `hit-me-codex-luot-5.md` và bàn giao `MD/NET_PROTOCOL.md`, Antigravity sẽ chạy rà soát và ghi các điểm khác biệt vào `MD/PROTOCOL_MISMATCH.md`.
