using System.Runtime.InteropServices;
using UnityEngine;

namespace HitMe.UI
{
    public static class HitMeWebBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void MarkHomeShown();
#else
        public static void MarkHomeShown()
        {
            // Editor / standalone fallback
        }
#endif
    }
}
