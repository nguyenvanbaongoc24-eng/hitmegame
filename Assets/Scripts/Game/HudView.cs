using UnityEngine;

namespace HitMe.Game
{
    /// <summary>
    /// Legacy HUD bridge retained only for serialized reference compatibility.
    /// Production HUD rendering is handled exclusively by ProductionUIController (uGUI/TextMeshPro) with zero per-frame allocations.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        public bool ReadyPressed { get; private set; }
        public bool ReplayPressed { get; private set; }

        public void RequestReady() => ReadyPressed = true;
        public void RequestReplay() => ReplayPressed = true;
        public void ResetButtons()
        {
            ReadyPressed = false;
            ReplayPressed = false;
        }
    }
}
