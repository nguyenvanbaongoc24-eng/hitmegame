#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HitMe.Editor
{
    public static class Phase2Step1Pipeline
    {
        public static void RunAll()
        {
            Debug.Log("--- Step 1: Configure All Sprites ---");
            SpriteImportSetup.ConfigureAllSprites();

            Debug.Log("--- Step 2: Build Atlases ---");
            Phase2SpriteAtlasBuilder.BuildAtlases();

            Debug.Log("--- Step 3: Build Prefabs and Test Scene ---");
            Phase2PrefabBuilder.BuildAll();

            Debug.Log("--- Step 4: Capture Evidence ---");
            Phase2PrefabBuilder.CaptureEvidence();

            Debug.Log("--- Phase 2 Step 1 Pipeline Completed Successfully ---");
        }
    }
}
#endif
