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
#if UNITY_5_3_OR_NEWER
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
#endif
                return false;
            }
        }
    }
}
