#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HitMe.Editor
{
    public static class Phase2Step1Pipeline
    {
        public static void RunAll()
        {
            Debug.Log("--- Step 1: Generate Procedural Foot Elements ---");
            ProceduralFootElements.GenerateAll();

            Debug.Log("--- Step 2: Configure All Sprites ---");
            SpriteImportSetup.ConfigureAllSprites();

            Debug.Log("--- Step 3: Build Atlases ---");
            Phase2SpriteAtlasBuilder.BuildAtlases();

            Debug.Log("--- Step 4: Build Prefabs and Test Scenes ---");
            Phase2PrefabBuilder.BuildAll();

            Debug.Log("--- Step 5: Capture Evidence ---");
            Phase2PrefabBuilder.CaptureEvidence();

            Debug.Log("--- Step 6: Run 1000-Match Bot Simulation v4 ---");
            Phase1GrayboxSceneBuilder.GenerateSimulationEvidenceV4();

            Debug.Log("--- Step 7: Run AutoPlay Evidence ---");
            Phase1GrayboxSceneBuilder.GenerateAutoPlayEvidence();

            Debug.Log("--- Phase 2 Step 1 Pipeline Completed Successfully ---");
        }
    }
}
#endif
