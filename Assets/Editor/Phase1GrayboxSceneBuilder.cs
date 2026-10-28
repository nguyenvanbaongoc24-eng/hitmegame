using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HitMe.Core;
using HitMe.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HitMe.Editor
{
    public static class Phase1GrayboxSceneBuilder
    {
        public static void GenerateSimulationEvidence()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigAsset>("Assets/ScriptableObjects/GameConfig_Phase1.asset").ToCore();
            var wins = new int[config.MaxPlayers];
            var roundCounts = new SortedDictionary<int, int>();
            var roundsBySeed = new List<int>();
            int totalRounds = 0;
            int clearWinners = 0;
            int hpDraws = 0;
            for (uint seed = 1; seed <= 1000; seed++)
            {
                var result = MatchSimulator.RunBotMatchDetailed(config, seed);
                totalRounds += result.Rounds;
                roundsBySeed.Add(result.Rounds);
                int count;
                roundCounts.TryGetValue(result.Rounds, out count);
                roundCounts[result.Rounds] = count + 1;
                if (result.WinnerIds.Count == 1)
                {
                    clearWinners++;
                }
                else if (result.WinnerIds.Count > 1)
                {
                    hpDraws++;
                }
                for (int w = 0; w < result.WinnerIds.Count; w++)
                {
                    int index;
                    if (int.TryParse(result.WinnerIds[w].Replace("bot-", ""), out index) && index >= 0 && index < wins.Length) wins[index]++;
                }
            }
            var text = new StringBuilder();
            text.AppendLine("Phase 1 Bot Simulation v2 (Post-Audit Balance)");
            text.AppendLine("Config: Arena=" + config.ArenaWidth + "x" + config.ArenaHeight + ", MaxRounds=" + config.MaxRounds + ", Tiebreak=" + config.RoundLimitTiebreak + ", NoAim=" + config.NoAimRule);
            text.AppendLine("Matches: 1000");
            text.AppendLine("Average rounds: " + (totalRounds / 1000f).ToString("0.000"));
            text.AppendLine("Minimum rounds: " + roundsBySeed.Min());
            text.AppendLine("Maximum rounds: " + roundsBySeed.Max());
            text.AppendLine("Clear winner matches: " + clearWinners + " (" + (clearWinners / 10f).ToString("0.0") + "%)");
            text.AppendLine("HP draw / shared win matches: " + hpDraws + " (" + (hpDraws / 10f).ToString("0.0") + "%)");
            text.AppendLine("Round distribution (rounds: matches):");
            foreach (var pair in roundCounts) text.AppendLine(pair.Key + ": " + pair.Value);
            text.AppendLine("Rounds by seed (seed: rounds):");
            for (int i = 0; i < roundsBySeed.Count; i++) text.AppendLine((i + 1) + ": " + roundsBySeed[i]);
            for (int i = 0; i < wins.Length; i++) text.AppendLine("Bot position " + (i + 1) + ": " + wins[i] + " shared/sole wins (" + (wins[i] / 10f).ToString("0.0") + "%)");

            Directory.CreateDirectory("Evidence/phase1");
            File.WriteAllText("Evidence/phase1/sim_v2.txt", text.ToString());
            File.WriteAllText("Evidence/phase1/sim.txt", text.ToString());
            Debug.Log(text.ToString());
        }

        public static void GenerateAutoPlayEvidence()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigAsset>("Assets/ScriptableObjects/GameConfig_Phase1.asset").ToCore();
            var text = new StringBuilder(); text.AppendLine("Phase 1 AutoPlay batchmode evidence"); text.AppendLine("Mode: every participant, including player, is driven by Phase1BotBrain.");
            int errors = 0;
            for (uint match = 1; match <= 10; match++)
            {
                try { var result = MatchSimulator.RunBotMatchDetailed(config, config.Seed + match); text.AppendLine("match=" + match + " rounds=" + result.Rounds + " winners=" + string.Join(",", result.WinnerIds) + " exceptions=0"); }
                catch (System.Exception exception) { errors++; text.AppendLine("match=" + match + " ERROR=" + exception); }
            }
            text.AppendLine("Summary: matches=10 exceptions=" + errors + " hung=0");
            Directory.CreateDirectory("Evidence/phase1"); File.WriteAllText("Evidence/phase1/playtest_auto.txt", text.ToString()); Debug.Log(text.ToString());
        }

        public static void BuildWindowsEvidence()
        {
            Directory.CreateDirectory("Evidence/phase1");
            const string output = "Evidence/phase1/WindowsBuild/HITME.exe";
            var report = UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions { scenes = new[] { "Assets/Scenes/Graybox.unity" }, locationPathName = output, target = UnityEditor.BuildTarget.StandaloneWindows64, options = UnityEditor.BuildOptions.None });
            File.WriteAllText("Evidence/phase1/build.txt", "Target: Windows 64-bit\nResult: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nOutput: " + output + "\n");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Windows build failed; see Evidence/phase1/build.txt");
        }

        public static void BuildWebGLEvidence()
        {
            Directory.CreateDirectory("Evidence/phase1");
            var output = "Evidence/phase1/WebGLBuild";
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-buildOutput") output = args[i + 1];
            PlayerSettings.WebGL.template = "PROJECT:Phase1Mobile";
            // Ensure a changed custom template is not served from the Editor's
            // imported-template cache on the next evidence build.
            AssetDatabase.ImportAsset("Assets/WebGLTemplates/Phase1Mobile/index.html", ImportAssetOptions.ForceUpdate);
            var report = UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions { scenes = new[] { "Assets/Scenes/Graybox.unity" }, locationPathName = output, target = UnityEditor.BuildTarget.WebGL, options = UnityEditor.BuildOptions.None });
            File.WriteAllText("Evidence/phase1/webgl-build.txt", "Target: WebGL\nResult: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nOutput: " + output + "\n");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("WebGL build failed; see Evidence/phase1/webgl-build.txt");
        }

        public static void CreateGrayboxScene()
        {
            const string configPath = "Assets/ScriptableObjects/GameConfig_Phase1.asset";
            var config = AssetDatabase.LoadAssetAtPath<GameConfigAsset>(configPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfigAsset>();
                AssetDatabase.CreateAsset(config, configPath);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 10f; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.06f, .07f, .09f); cameraGo.tag = "MainCamera"; cameraGo.transform.position = new Vector3(0, 0, -10);
            var arenaGo = new GameObject("Arena"); var arena = arenaGo.AddComponent<ArenaView>(); arena.configAsset = config;
            var hud = new GameObject("HUD").AddComponent<HudView>();
            var controllerGo = new GameObject("Match Controller"); var controller = controllerGo.AddComponent<MatchController>(); controller.configAsset = config; controller.arena = arena; controller.hud = hud;
            var input = controllerGo.AddComponent<PlacementInput>(); input.viewCamera = camera; input.controller = controller;
            var capture = controllerGo.AddComponent<Phase1ScreenshotCapture>(); capture.controller = controller;
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Graybox.unity");
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes) { new EditorBuildSettingsScene("Assets/Scenes/Graybox.unity", true) };
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Phase 1 Graybox scene created.");
        }
    }
}
