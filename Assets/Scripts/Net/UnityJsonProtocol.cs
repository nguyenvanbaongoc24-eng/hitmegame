using System;
using UnityEngine;

namespace HitMe.NetClientV1
{
    public sealed class UnityJsonProtocol : IProtocolJson
    {
        public static readonly UnityJsonProtocol Instance = new UnityJsonProtocol();

        public string ToJson(object value)
        {
            if (value == null) return string.Empty;
            return JsonUtility.ToJson(value);
        }

        public T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            return JsonUtility.FromJson<T>(json);
        }
    }
}
