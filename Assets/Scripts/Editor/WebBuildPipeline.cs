using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HitMe.Editor
{
    public static class WebBuildPipeline
    {
        [MenuItem("Tools/HIT ME/Build WebGL (Localhost)")]
        public static void BuildWeb()
        {
            Debug.Log("Starting HIT ME WebGL Build...");

            // 1. Build Sample Scene first to ensure fresh scene and assets
            SceneSetupEditor.BuildSampleScene();

            // 2. Configure WebGL settings
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            PlayerSettings.productName = "HIT ME VN";
            PlayerSettings.companyName = "HitMeStudio";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;

            // Set compression format to Disabled for instant, reliable loading on local HTTP server
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;

            string buildDir = "D:/HIT ME VN GAME/Builds/WebLocal";
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            string[] scenes = new string[] { "Assets/Scenes/MainMatch.unity" };

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
                Debug.Log($"WebGL Build succeeded: {summary.totalSize} bytes at {buildDir}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"WebGL Build failed! Errors: {summary.totalErrors}");
                throw new Exception($"WebGL Build failed with {summary.totalErrors} errors!");
            }
        }
    }
}
