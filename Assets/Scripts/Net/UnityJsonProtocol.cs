using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#else
using System.Text.Json;
#endif

namespace HitMe.NetClientV1
{
    public sealed class UnityJsonProtocol : IProtocolJson
    {
        public static readonly UnityJsonProtocol Instance = new UnityJsonProtocol();
#if !UNITY_5_3_OR_NEWER
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true
        };
#endif

        public string ToJson(object value)
        {
            if (value == null) return string.Empty;
#if UNITY_5_3_OR_NEWER
            return JsonUtility.ToJson(value);
#else
            return JsonSerializer.Serialize(value, value.GetType(), Options);
#endif
        }

        public T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
#if UNITY_5_3_OR_NEWER
            return JsonUtility.FromJson<T>(json);
#else
            return JsonSerializer.Deserialize<T>(json, Options);
#endif
        }
    }
}
