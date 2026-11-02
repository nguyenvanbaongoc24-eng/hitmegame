using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HitMe.UI
{
    public static class RingBufferLogger
    {
        private const int MaxEntries = 50;
        private static readonly List<string> _entries = new List<string>(MaxEntries);
        private static readonly object _sync = new object();
        private static bool _isHooked = false;

        public static void Initialize()
        {
            if (_isHooked) return;
            Application.logMessageReceived += HandleUnityLog;
            _isHooked = true;
        }

        private static void HandleUnityLog(string condition, string stackTrace, LogType type)
        {
            string level = type == LogType.Error || type == LogType.Exception ? "ERROR"
                : (type == LogType.Warning ? "WARN" : "INFO");
            Log(level, condition);
        }

        public static void Log(string level, string rawMessage)
        {
            if (string.IsNullOrEmpty(rawMessage)) return;

            string sanitized = SanitizeMessage(rawMessage);
            string timeStr = DateTime.UtcNow.ToString("HH:mm:ss");
            string entry = $"[{timeStr}][{level}] {sanitized}";

            lock (_sync)
            {
                if (_entries.Count >= MaxEntries)
                {
                    _entries.RemoveAt(0);
                }
                _entries.Add(entry);
            }
        }

        public static string SanitizeMessage(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // 1. Scrub token/password/secret/authorization/reconnectToken key-values
            string cleaned = Regex.Replace(
                input,
                @"(?i)(token|bearer|password|passwd|secret|reconnectToken|authorization)[""':=\s]+([a-zA-Z0-9_\-\.]{6,})",
                "$1: [REDACTED]");

            // 2. Scrub guest tokens (e.g. guest:Player_1234, g_b5a4c5cca600ac060d874f5530ac267a)
            cleaned = Regex.Replace(
                cleaned,
                @"guest:[a-zA-Z0-9_\-]+",
                "guest:[REDACTED]");

            cleaned = Regex.Replace(
                cleaned,
                @"\bg_[a-f0-9]{20,}\b",
                "[REDACTED_ID]");

            return cleaned;
        }

        public static IReadOnlyList<string> GetRecentLogs()
        {
            lock (_sync)
            {
                return new List<string>(_entries);
            }
        }

        public static void Clear()
        {
            lock (_sync)
            {
                _entries.Clear();
            }
        }

        public static string BuildBugReport(
            string buildVersion,
            string language,
            string currentScreen,
            string roomCode,
            string operatingSystem = null)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("### HIT ME Bug Report");
            sb.AppendLine($"- **Build Version / Commit**: {buildVersion ?? "Unknown"}");
            sb.AppendLine($"- **Language**: {language ?? "Vietnamese"}");
            sb.AppendLine($"- **Screen**: {currentScreen ?? "Home"}");
            sb.AppendLine($"- **Room Code**: {(!string.IsNullOrEmpty(roomCode) ? roomCode : "N/A")}");
            sb.AppendLine($"- **Platform / OS**: {operatingSystem ?? SystemInfo.operatingSystem + " (" + Application.platform + ")"}");
            sb.AppendLine($"- **Timestamp (UTC)**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
            sb.AppendLine("#### Recent Logs (Last 50, Sanitized):");
            sb.AppendLine("```");

            var logs = GetRecentLogs();
            if (logs.Count == 0)
            {
                sb.AppendLine("(No log entries recorded)");
            }
            else
            {
                foreach (var line in logs)
                {
                    sb.AppendLine(line);
                }
            }
            sb.AppendLine("```");
            return sb.ToString();
        }
    }
}
