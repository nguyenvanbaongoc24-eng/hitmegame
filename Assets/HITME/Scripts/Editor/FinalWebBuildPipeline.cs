using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HitMe.Editor
{
    /// <summary>
    /// Builds the already-authored MainMatch scene. This deliberately does not invoke
    /// legacy scene setup helpers, so the visual foundation and core gameplay stay intact.
    /// </summary>
    public static class FinalWebBuildPipeline
    {
        private const string MainMatchScene = "Assets/Scenes/MainMatch.unity";
        private const string OutputPath = "Builds/WebGL";

        [MenuItem("Tools/HIT ME/FINAL/Build WebGL")]
        public static void BuildWebGL()
        {
            if (!File.Exists(MainMatchScene))
                throw new BuildFailedException("MainMatch scene was not found: " + MainMatchScene);

            PlayerSettings.defaultWebScreenWidth = 390;
            PlayerSettings.defaultWebScreenHeight = 844;
            PlayerSettings.runInBackground = false;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MainMatchScene },
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"WebGL build failed: {report.summary.result}. See {OutputPath} and the Editor log.");

            Debug.Log($"HIT ME FINAL WebGL build succeeded: {report.summary.totalSize} bytes in {report.summary.totalTime.TotalSeconds:F1}s at {OutputPath}.");
        }
    }
}
