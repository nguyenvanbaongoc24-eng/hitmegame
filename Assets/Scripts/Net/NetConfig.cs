namespace HitMe.NetClientV1
{
    public static class NetConfig
    {
        public static bool Enabled = false;

        public static bool IsOnlineActive
        {
            get
            {
                if (Enabled) return true;
#if UNITY_WEBGL && !UNITY_EDITOR
                // The shipped WebGL client uses the live room service by default.
                return true;
#else
                if (UnityEngine.Application.absoluteURL != null)
                {
                    string u = UnityEngine.Application.absoluteURL;
                    if (u.IndexOf("online=1", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        u.IndexOf("ws=1", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        u.IndexOf("server=", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
                return false;
#endif
            }
        }
    }
}
