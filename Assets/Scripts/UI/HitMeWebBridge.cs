using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HitMe.UI
{
    public static class HitMeWebBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void MarkHomeShown();

        [DllImport("__Internal")]
        public static extern int IsE2EEnabled();

        [DllImport("__Internal")]
        public static extern void UpdateHitMeStateJson(string json);

        [DllImport("__Internal")]
        public static extern void UpdateHitMeUiDumpJson(string json);

        [DllImport("__Internal")]
        public static extern void EmitUnityToWebJson(string json);
#else
        public static void MarkHomeShown()
        {
            // Editor / standalone fallback
        }

        public static int IsE2EEnabled()
        {
            return 1; // Allow testing in Editor if needed
        }

        public static void UpdateHitMeStateJson(string json)
        {
            // Editor / standalone fallback
        }

        public static void UpdateHitMeUiDumpJson(string json)
        {
            // Editor / standalone fallback
        }

        public static void EmitUnityToWebJson(string json)
        {
            // Editor / standalone fallback
        }
#endif
    }
}
