using System.Collections.Generic;
using System.Linq;
using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class HudView : MonoBehaviour
    {
        public bool ReadyPressed { get; private set; }
        public bool ReplayPressed { get; private set; }
        public void ResetButtons() { ReadyPressed = false; ReplayPressed = false; }
        public void OnGUI()
        {
            var controller = FindFirstObjectByType<MatchController>(); if (controller == null || controller.State == null) return;
            var state = controller.State; GUI.color = Color.white;
            GUI.Label(new Rect(20, 20, 360, 32), "VÒNG " + state.RoundNumber + "  •  " + state.Phase + "  •  " + controller.SecondsRemaining.ToString("0.0") + "s");
            GUI.Label(new Rect(20, 50, 500, 26), "Chạm để đặt · Kéo từ vòng tròn để chọn hướng");
            int y = 84;
            for (int i = 0; i < state.Players.Count; i++)
            {
                var p = state.Players[i];
                if (state.Phase == Phase.Placing && p.IsBot) continue;
                GUI.Label(new Rect(20, y, 220, 22), p.Name + ": " + p.Hp + " HP" + (p.IsAlive ? "" : "  (đã loại)"));
                y += 22;
            }
            if (state.Phase == Phase.Placing && GUI.Button(new Rect(Screen.width - 150, 24, 120, 44), "SẴN SÀNG")) ReadyPressed = true;
            if (state.Phase == Phase.MatchEnd)
            {
                DrawResult(state);
            }
        }

        private void DrawResult(MatchState state)
        {
            const float panelWidth = 540f;
            const float rowHeight = 38f;
            var ranked = state.Players.OrderByDescending(p => p.IsAlive).ThenByDescending(p => p.Hp).ThenBy(p => p.Name).ToList();
            var height = 154f + ranked.Count * rowHeight;
            var x = (Screen.width - panelWidth) * .5f;
            var y = (Screen.height - height) * .5f;
            GUI.Box(new Rect(x, y, panelWidth, height), "KẾT QUẢ");
            GUI.Label(new Rect(x + 28, y + 42, panelWidth - 56, 30), "NGƯỜI THẮNG: " + string.Join(", ", state.WinnerIds));
            GUI.Label(new Rect(x + 28, y + 70, panelWidth - 56, 25), "XẾP HẠNG");
            for (int i = 0; i < ranked.Count; i++)
            {
                var player = ranked[i];
                var suffix = state.WinnerIds.Contains(player.Id) ? "  ★ THẮNG" : (player.IsAlive ? "  còn sống" : "  đã loại");
                GUI.Label(new Rect(x + 28, y + 98 + i * rowHeight, panelWidth - 56, rowHeight), (i + 1) + ". " + player.Name + " — " + player.Hp + " HP" + suffix);
            }
            if (GUI.Button(new Rect(x + panelWidth * .5f - 90f, y + height - 52f, 180f, 36f), "CHƠI LẠI")) ReplayPressed = true;
        }
    }
}
