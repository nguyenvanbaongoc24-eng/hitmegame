using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HitMe.Editor
{
    public static class WebBuildPipeline
    {
        public static string GetGitShortCommit()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --short HEAD",
                    WorkingDirectory = Directory.GetParent(Application.dataPath)?.FullName ?? ".",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc != null)
                    {
                        string output = proc.StandardOutput.ReadToEnd().Trim();
                        proc.WaitForExit(3000);
                        if (!string.IsNullOrEmpty(output))
                        {
                            return output;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WebBuildPipeline] Failed to read git commit: {ex.Message}");
            }
            return "unknown";
        }

        public static string WriteBuildFingerprintResource(string sceneName = "Graybox")
        {
            string commitShort = GetGitShortCommit();
            string dateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            string fingerprint = $"build {commitShort} · {dateTimeStr} · {sceneName}";

            Directory.CreateDirectory("Assets/Resources");
            string resourcePath = "Assets/Resources/BuildFingerprint.txt";
            File.WriteAllText(resourcePath, fingerprint);
            AssetDatabase.ImportAsset(resourcePath, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"[WebBuildPipeline] Wrote BuildFingerprint: {fingerprint}");
            return commitShort;
        }

        [MenuItem("Tools/HIT ME/Build WebGL (Fingerprinted)")]
        public static void BuildWeb()
        {
            string commitShort = WriteBuildFingerprintResource("Graybox");
            string dateTag = DateTime.Now.ToString("yyyyMMdd");

            Debug.Log($"Starting HIT ME Clean Fingerprinted WebGL Build (commit={commitShort}, date={dateTag})...");

            var configAsset = AssetDatabase.LoadAssetAtPath<HitMe.Game.GameConfigAsset>("Assets/ScriptableObjects/GameConfig_Phase1.asset");
            if (configAsset == null || configAsset.characterIdleSprites == null || configAsset.characterIdleSprites.Length < 7)
            {
                ProductionUISetup.BuildProductionUI();
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            PlayerSettings.productName = "HIT ME";
            PlayerSettings.companyName = "HitMeStudio";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;

            string folderName = $"WebGL_{dateTag}_{commitShort}";
            string buildDir = Path.GetFullPath(Path.Combine("Builds", folderName));
            if (Directory.Exists(buildDir))
            {
                string timeSuffix = DateTime.Now.ToString("HHmmss");
                folderName = $"WebGL_{dateTag}_{commitShort}_{timeSuffix}";
                buildDir = Path.GetFullPath(Path.Combine("Builds", folderName));
            }
            Directory.CreateDirectory(buildDir);

            string[] scenes = BuildScenes.Production;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string indexPath = Path.Combine(buildDir, "index.html");
                if (File.Exists(indexPath))
                {
                    string html = File.ReadAllText(indexPath);
                    string metaTag = $"<meta name=\"build-fingerprint\" content=\"build {commitShort} · {dateTag} · Graybox\">";
                    if (!html.Contains("build-fingerprint"))
                    {
                        html = html.Replace("<head>", "<head>\n    " + metaTag);
                    }
                    html = html.Replace("\"phase1-closed-20261028\"", $"\"{commitShort}\"");
                    File.WriteAllText(indexPath, html);
                }
                Debug.Log($"Build completed with a result of 'Succeeded' in {summary.totalTime.TotalSeconds:F2} seconds ({summary.totalSize} bytes)");
                Debug.Log($"WebGL Build succeeded: {summary.totalSize} bytes at {buildDir} (commit={commitShort})");
            }
            else
            {
                Debug.LogError($"WebGL Build failed! Result: {summary.result}, Errors: {summary.totalErrors}");
                throw new Exception($"WebGL Build failed with {summary.totalErrors} errors!");
            }
        }

        [MenuItem("Tools/HIT ME/Build WebGL (Release Profile - Brotli - Lo 10)")]
        public static void BuildWebRelease()
        {
            string commitShort = WriteBuildFingerprintResource("Graybox");
            string dateTag = DateTime.Now.ToString("yyyyMMdd");

            Debug.Log($"Starting HIT ME Release Profile WebGL Build (commit={commitShort}, date={dateTag}, compression=Brotli)...");

            var configAsset = AssetDatabase.LoadAssetAtPath<HitMe.Game.GameConfigAsset>("Assets/ScriptableObjects/GameConfig_Phase1.asset");
            if (configAsset == null || configAsset.characterIdleSprites == null || configAsset.characterIdleSprites.Length < 7)
            {
                ProductionUISetup.BuildProductionUI();
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            PlayerSettings.productName = "HIT ME";
            PlayerSettings.companyName = "HitMeStudio";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;

            // Release optimizations per Round 35 Lo 10 & Round 38 A7
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ManagedStrippingLevel.Medium);

            string folderName = $"WebGL_Release_{dateTag}_{commitShort}";
            string buildDir = Path.GetFullPath(Path.Combine("Builds", folderName));
            if (Directory.Exists(buildDir))
            {
                string timeSuffix = DateTime.Now.ToString("HHmmss");
                folderName = $"WebGL_Release_{dateTag}_{commitShort}_{timeSuffix}";
                buildDir = Path.GetFullPath(Path.Combine("Builds", folderName));
            }
            Directory.CreateDirectory(buildDir);

            string[] scenes = BuildScenes.Production;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string indexPath = Path.Combine(buildDir, "index.html");
                if (File.Exists(indexPath))
                {
                    string html = File.ReadAllText(indexPath);
                    string metaTag = $"<meta name=\"build-fingerprint\" content=\"build {commitShort} · {dateTag} · Graybox\">";
                    if (!html.Contains("build-fingerprint"))
                    {
                        html = html.Replace("<head>", "<head>\n    " + metaTag);
                    }
                    html = html.Replace("\"phase1-closed-20261028\"", $"\"{commitShort}\"");
                    File.WriteAllText(indexPath, html);
                }
                Debug.Log($"Release WebGL Build succeeded: {summary.totalSize} bytes at {buildDir} (commit={commitShort})");
            }
            else
            {
                Debug.LogError($"Release WebGL Build failed! Result: {summary.result}, Errors: {summary.totalErrors}");
                throw new Exception($"Release WebGL Build failed with {summary.totalErrors} errors!");
            }
        }
    }
}
