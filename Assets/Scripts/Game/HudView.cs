using System.Collections.Generic;
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
            int y = 84; for (int i = 0; i < state.Players.Count; i++) { var p = state.Players[i]; GUI.Label(new Rect(20, y, 220, 22), p.Name + ": " + p.Hp + " HP" + (p.IsAlive ? "" : "  (đã loại)")); y += 22; }
            if (state.Phase == Phase.Placing && GUI.Button(new Rect(Screen.width - 150, 24, 120, 44), "SẴN SÀNG")) ReadyPressed = true;
            if (state.Phase == Phase.MatchEnd)
            {
                GUI.Box(new Rect(Screen.width / 2 - 175, Screen.height / 2 - 85, 350, 170), "KẾT QUẢ\n" + string.Join(", ", state.WinnerIds) + " thắng");
                if (GUI.Button(new Rect(Screen.width / 2 - 70, Screen.height / 2 + 30, 140, 35), "CHƠI LẠI")) ReplayPressed = true;
            }
        }
    }
}
