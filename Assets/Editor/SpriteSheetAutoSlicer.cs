using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace HitMe.Editor
{
    /// <summary>
    /// HIT ME - 2.5D Sprite Sheet Auto Slicer & Animation Clip Generator.
    /// Pipeline: Tự động import -> Khử viền/Cấu hình Texture -> Slice Multi-Sprite với chuẩn Pivot (Chân: (0.5, 0.0), Khớp vai: (0.2, 0.8))
    /// -> Tạo Animation Clips (Idle, Move, Aim, Throw, Hit, Elimination) -> Khởi tạo Animator Controller.
    /// </summary>
    public class SpriteSheetAutoSlicer : EditorWindow
    {
        private string sourceFolder = "ASSET/CHARACTERS";
        private string destinationFolder = "Assets/Art/Characters";
        private string animationOutputFolder = "Assets/Art/Animations";
        private string selectedCharacter = "Anh Nón Lá";
        private bool generateAnimatorController = true;
        private float frameRate = 12f; // 12 FPS chuẩn 2.5D hand-drawn chibi

        // Định nghĩa số lượng frame cho từng Animation Clip theo đặc tả
        public static readonly Dictionary<string, (int frameCount, bool loop)> AnimationSpecs = new Dictionary<string, (int, bool)>
        {
            { "Idle", (8, true) },
            { "Move", (8, true) },
            { "Aim", (8, false) },
            { "Throw", (12, false) },
            { "Hit", (6, false) },
            { "Elimination", (10, false) }
        };

        [MenuItem("Tools/HIT ME/2.5D Pipeline/Sprite Sheet Slicer Window", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteSheetAutoSlicer>("2.5D Sprite Slicer");
            window.minSize = new Vector2(460, 420);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("HIT ME - 2.5D Asset Slicer & Animation Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox("Công cụ tự động cắt Sprite Sheet nhiều frame/layer và tạo Animation Clips theo chuẩn Pivot 2.5D:\n- Chân (Bottom Center): (0.5, 0.0)\n- Cánh tay ném (Vai/Shoulder Joint): (0.2, 0.8)", MessageType.Info);

            EditorGUILayout.Space(5);
            selectedCharacter = EditorGUILayout.TextField("Tên nhân vật:", selectedCharacter);
            sourceFolder = EditorGUILayout.TextField("Thư mục Asset gốc:", sourceFolder);
            destinationFolder = EditorGUILayout.TextField("Thư mục Import Unity:", destinationFolder);
            animationOutputFolder = EditorGUILayout.TextField("Thư mục xuất Animation:", animationOutputFolder);
            frameRate = EditorGUILayout.FloatField("Frame Rate (FPS):", frameRate);
            generateAnimatorController = EditorGUILayout.Toggle("Tạo Animator Controller", generateAnimatorController);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("1. Xử lý & Cắt Sprite Nhân Vật Này", GUILayout.Height(35)))
            {
                ProcessSingleCharacter(selectedCharacter);
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("2. Batch Process Toàn Bộ Nhân Vật trong ASSET/CHARACTERS", GUILayout.Height(35)))
            {
                BatchProcessAll();
            }
        }

        [MenuItem("Tools/HIT ME/2.5D Pipeline/Slice & Generate - Anh Nón Lá", false, 20)]
        public static void SliceAnhNonLa()
        {
            ProcessSingleCharacter("Anh Nón Lá");
        }

        [MenuItem("Tools/HIT ME/2.5D Pipeline/Slice & Generate - Cậu Bé Làng (Nam Làng)", false, 21)]
        public static void SliceCauBeLang()
        {
            ProcessSingleCharacter("Cậu Bé Làng");
        }

        public static string GetAssetCharactersDirectory()
        {
            string[] candidates = new string[]
            {
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "ASSET", "CHARACTERS")),
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ASSET", "CHARACTERS")),
                @"D:\HIT ME VN GAME\ASSET\CHARACTERS"
            };

            foreach (var path in candidates)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            return candidates[0];
        }

        [MenuItem("Tools/HIT ME/2.5D Pipeline/Batch Process All Characters", false, 30)]
        public static void BatchProcessAll()
        {
            string assetCharsDir = GetAssetCharactersDirectory();

            if (!Directory.Exists(assetCharsDir))
            {
                Debug.LogError($"[SpriteSheetAutoSlicer] Không tìm thấy thư mục: {assetCharsDir}");
                return;
            }

            string[] pngFiles = Directory.GetFiles(assetCharsDir, "*.png");
            Debug.Log($"[SpriteSheetAutoSlicer] Bắt đầu xử lý {pngFiles.Length} file trong {assetCharsDir}");

            foreach (var filePath in pngFiles)
            {
                string charName = Path.GetFileNameWithoutExtension(filePath);
                ProcessCharacterAsset(filePath, charName);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SpriteSheetAutoSlicer] Hoàn tất Batch Processing toàn bộ nhân vật!");
        }

        public static void ProcessSingleCharacter(string characterName)
        {
            string assetCharsDir = GetAssetCharactersDirectory();

            // Tìm file khớp tên (hỗ trợ cả có dấu và không dấu)
            string targetFile = null;
            if (Directory.Exists(assetCharsDir))
            {
                string[] files = Directory.GetFiles(assetCharsDir, "*.png");
                targetFile = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).IndexOf(characterName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (targetFile == null && characterName.Contains("Nón Lá"))
                {
                    targetFile = files.FirstOrDefault(f => f.IndexOf("NON LA", StringComparison.OrdinalIgnoreCase) >= 0 || f.IndexOf("NÓN LÁ", StringComparison.OrdinalIgnoreCase) >= 0);
                }
                if (targetFile == null && (characterName.Contains("Cậu Bé") || characterName.Contains("Nam Làng")))
                {
                    targetFile = files.FirstOrDefault(f => f.IndexOf("CẬU BÉ LÀNG", StringComparison.OrdinalIgnoreCase) >= 0 || f.IndexOf("CAU BE LANG", StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            if (targetFile == null)
            {
                Debug.LogWarning($"[SpriteSheetAutoSlicer] Không tìm thấy file cho '{characterName}'. Đang tạo template procedural sprite sheet...");
                CreateProceduralCharacterSpriteSheet(characterName);
                return;
            }

            ProcessCharacterAsset(targetFile, characterName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ProcessCharacterAsset(string sourceFilePath, string characterName)
        {
            string cleanCharName = CleanFileName(characterName);
            string destDir = "Assets/Art/Characters/" + cleanCharName;
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            string destFileName = $"{cleanCharName}_spritesheet.png";
            string destAssetPath = $"{destDir}/{destFileName}";
            string fullDestPath = Path.Combine(Application.dataPath, "..", destAssetPath);

            // Copy file nguồn sang thư mục Unity Assets nếu chưa có hoặc có thay đổi
            File.Copy(sourceFilePath, fullDestPath, true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(destAssetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

            // Cấu hình TextureImporter & Slice Sprites
            TextureImporter importer = AssetImporter.GetAtPath(destAssetPath) as TextureImporter;
            if (importer == null)
            {
                // Thử lại sau khi refresh
                AssetDatabase.Refresh();
                importer = AssetImporter.GetAtPath(destAssetPath) as TextureImporter;
            }

            if (importer == null)
            {
                Debug.LogError($"[SpriteSheetAutoSlicer] Không tải được TextureImporter cho: {destAssetPath}");
                return;
            }

            // Đọc kích thước thật của ảnh
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(destAssetPath);
            int texWidth = texture != null ? texture.width : 1536;
            int texHeight = texture != null ? texture.height : 1024;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = Math.Max(texWidth, texHeight);

            // Thực hiện tính toán các Sprite Rects & Pivots
            List<SpriteMetaData> metaList = GenerateSpriteMetaData(cleanCharName, texWidth, texHeight);
            importer.spritesheet = metaList.ToArray();

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log($"[SpriteSheetAutoSlicer] Đã slice {metaList.Count} sprite frames cho '{cleanCharName}' tại {destAssetPath}");

            // Tạo Animation Clips từ các sub-sprite vừa slice
            GenerateAnimationClips(destAssetPath, cleanCharName, metaList);
        }

        /// <summary>
        /// Tạo Metadata cho Multi-Sprite với chuẩn Pivot:
        /// - Chân tại (0.5, 0.0)
        /// - Cánh tay ném tại khớp vai (0.2, 0.8)
        /// - Các layer chi tiết cơ thể: Nón lá, Tóc, Mặt, Thân, Quần, Cánh tay, Vũ khí
        /// </summary>
        private static List<SpriteMetaData> GenerateSpriteMetaData(string characterName, int width, int height)
        {
            List<SpriteMetaData> metas = new List<SpriteMetaData>();

            // Phân bổ lưới Animation: 6 hàng ứng với 6 động tác
            // Idle (8 frames), Move (8 frames), Aim (8 frames), Throw (12 frames), Hit (6 frames), Elimination (10 frames)
            int rows = 6;
            float rowHeight = (float)height / rows;

            int rowIndex = 0;
            foreach (var kvp in AnimationSpecs)
            {
                string animName = kvp.Key;
                int frameCount = kvp.Value.frameCount;
                float colWidth = (float)width / frameCount;

                // Unity texture coordinate: Y = 0 nằm ở cạnh đáy, nên rowIndex 0 nằm ở trên cùng tương ứng với Y = height - (rowIndex+1)*rowHeight
                float y = height - (rowIndex + 1) * rowHeight;

                for (int f = 0; f < frameCount; f++)
                {
                    float x = f * colWidth;
                    var meta = new SpriteMetaData
                    {
                        name = $"{characterName}_{animName}_{f:D2}",
                        rect = new Rect(x, y, colWidth, rowHeight),
                        alignment = (int)SpriteAlignment.Custom,
                        pivot = new Vector2(0.5f, 0.0f) // Chân đặt tại tâm đáy (0.5, 0.0) chuẩn 2.5D
                    };
                    metas.Add(meta);
                }
                rowIndex++;
            }

            // Bổ sung các Layer chi tiết cơ thể (Phục vụ 2.5D Bone Rigging / Paperdoll)
            // Layers: Hat, Hair, Face, Torso, Pants, Feet, ArmThrow (khớp vai), Weapon
            float layerCellWidth = width / 8f;
            float layerCellHeight = rowHeight * 0.9f;
            float layerY = 0f; // Hàng đáy

            string[] layerNames = new string[] { "Hat", "Hair", "Face", "Torso", "Pants", "Feet", "ArmThrow", "Weapon" };
            for (int i = 0; i < layerNames.Length; i++)
            {
                Vector2 pivot = new Vector2(0.5f, 0.0f);
                if (layerNames[i] == "ArmThrow")
                {
                    pivot = new Vector2(0.2f, 0.8f); // Tâm xoay khớp vai
                }
                else if (layerNames[i] == "Weapon")
                {
                    pivot = new Vector2(0.5f, 0.2f); // Điểm cầm vũ khí
                }
                else if (layerNames[i] == "Hat" || layerNames[i] == "Hair" || layerNames[i] == "Face")
                {
                    pivot = new Vector2(0.5f, 0.5f);
                }

                var meta = new SpriteMetaData
                {
                    name = $"{characterName}_Layer_{layerNames[i]}",
                    rect = new Rect(i * layerCellWidth, layerY, layerCellWidth, layerCellHeight),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = pivot
                };
                metas.Add(meta);
            }

            return metas;
        }

        /// <summary>
        /// Tạo các Animation Clips tương ứng: Idle (8), Move (8), Aim (8), Throw (12), Hit (6), Elimination (10).
        /// </summary>
        private static void GenerateAnimationClips(string textureAssetPath, string characterName, List<SpriteMetaData> metas)
        {
            string animDir = $"Assets/Art/Animations/{characterName}";
            if (!Directory.Exists(animDir))
            {
                Directory.CreateDirectory(animDir);
            }

            AssetDatabase.ImportAsset(textureAssetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();

            // Tải toàn bộ sub-sprites vừa được slice
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(textureAssetPath);
            Dictionary<string, Sprite> spriteMap = assets.OfType<Sprite>().ToDictionary(s => s.name, s => s);

            Dictionary<string, AnimationClip> createdClips = new Dictionary<string, AnimationClip>();

            foreach (var kvp in AnimationSpecs)
            {
                string animName = kvp.Key;
                int frameCount = kvp.Value.frameCount;
                bool loop = kvp.Value.loop;

                List<Sprite> clipSprites = new List<Sprite>();
                for (int f = 0; f < frameCount; f++)
                {
                    string spriteName = $"{characterName}_{animName}_{f:D2}";
                    if (spriteMap.TryGetValue(spriteName, out Sprite sprite))
                    {
                        clipSprites.Add(sprite);
                    }
                }

                // Nếu chưa khớp tên tuyệt đối, tìm các sprite chứa tên animation hoặc fallback hợp lý
                if (clipSprites.Count == 0)
                {
                    var matching = spriteMap.Values
                        .Where(s => s.name.IndexOf(animName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();

                    if (matching.Count > 0)
                    {
                        clipSprites.AddRange(matching);
                    }
                    else if (spriteMap.Count > 0)
                    {
                        // Fallback sử dụng sprite đầu tiên có sẵn để đảm bảo luôn tạo đủ clip
                        clipSprites.Add(spriteMap.Values.First());
                    }
                }

                if (clipSprites.Count == 0)
                {
                    Debug.LogWarning($"[SpriteSheetAutoSlicer] Không tìm thấy sprite nào cho animation {animName}");
                    continue;
                }

                // Đảm bảo số lượng frame đúng chuẩn đặc tả (8, 8, 8, 12, 6, 10 frames)
                int initialCount = clipSprites.Count;
                while (clipSprites.Count < frameCount)
                {
                    clipSprites.Add(clipSprites[clipSprites.Count % initialCount]);
                }
                if (clipSprites.Count > frameCount)
                {
                    clipSprites = clipSprites.Take(frameCount).ToList();
                }

                string clipPath = $"{animDir}/{characterName}_{animName}.anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    clip = new AnimationClip();
                    AssetDatabase.CreateAsset(clip, clipPath);
                }
                else
                {
                    clip.ClearCurves();
                }

                clip.frameRate = 12f;

                // Cấu hình Loop
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                // Gán keyframe Sprite curve
                EditorCurveBinding curveBinding = new EditorCurveBinding
                {
                    type = typeof(SpriteRenderer),
                    path = "",
                    propertyName = "m_Sprite"
                };

                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[clipSprites.Count];
                for (int i = 0; i < clipSprites.Count; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe
                    {
                        time = i / 12f,
                        value = clipSprites[i]
                    };
                }

                AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);
                EditorUtility.SetDirty(clip);
                createdClips[animName] = clip;

                Debug.Log($"[SpriteSheetAutoSlicer] Tạo thành công Clip: {clipPath} ({clipSprites.Count} frames, loop: {loop})");
            }

            // Tạo Animator Controller
            CreateOrUpdateAnimatorController(animDir, characterName, createdClips);
        }

        private static void CreateOrUpdateAnimatorController(string animDir, string characterName, Dictionary<string, AnimationClip> clips)
        {
            string controllerPath = $"{animDir}/{characterName}_Controller.controller";
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            // Thêm các tham số điều khiển
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsAiming", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Throw", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsEliminated", AnimatorControllerParameterType.Bool);

            var rootStateMachine = controller.layers[0].stateMachine;

            AnimatorState idleState = null;
            AnimatorState moveState = null;
            AnimatorState aimState = null;
            AnimatorState throwState = null;
            AnimatorState hitState = null;
            AnimatorState elimState = null;

            if (clips.TryGetValue("Idle", out var idleClip))
            {
                idleState = rootStateMachine.AddState("Idle");
                idleState.motion = idleClip;
                rootStateMachine.defaultState = idleState;
            }
            if (clips.TryGetValue("Move", out var moveClip))
            {
                moveState = rootStateMachine.AddState("Move");
                moveState.motion = moveClip;
            }
            if (clips.TryGetValue("Aim", out var aimClip))
            {
                aimState = rootStateMachine.AddState("Aim");
                aimState.motion = aimClip;
            }
            if (clips.TryGetValue("Throw", out var throwClip))
            {
                throwState = rootStateMachine.AddState("Throw");
                throwState.motion = throwClip;
            }
            if (clips.TryGetValue("Hit", out var hitClip))
            {
                hitState = rootStateMachine.AddState("Hit");
                hitState.motion = hitClip;
            }
            if (clips.TryGetValue("Elimination", out var elimClip))
            {
                elimState = rootStateMachine.AddState("Elimination");
                elimState.motion = elimClip;
            }

            // Thiết lập Transitions
            if (idleState != null && moveState != null)
            {
                var toMove = idleState.AddTransition(moveState);
                toMove.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                toMove.hasExitTime = false;
                toMove.duration = 0.1f;

                var toIdle = moveState.AddTransition(idleState);
                toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
                toIdle.hasExitTime = false;
                toIdle.duration = 0.1f;
            }

            if (aimState != null)
            {
                var toAim = rootStateMachine.AddAnyStateTransition(aimState);
                toAim.AddCondition(AnimatorConditionMode.If, 0, "IsAiming");
                toAim.hasExitTime = false;
                toAim.duration = 0.05f;

                if (throwState != null)
                {
                    var toThrow = aimState.AddTransition(throwState);
                    toThrow.AddCondition(AnimatorConditionMode.If, 0, "Throw");
                    toThrow.hasExitTime = false;
                    toThrow.duration = 0.05f;
                }
            }

            if (throwState != null && idleState != null)
            {
                var backIdle = throwState.AddTransition(idleState);
                backIdle.hasExitTime = true;
                backIdle.exitTime = 0.95f;
                backIdle.duration = 0.1f;
            }

            if (hitState != null && idleState != null)
            {
                var toHit = rootStateMachine.AddAnyStateTransition(hitState);
                toHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
                toHit.hasExitTime = false;
                toHit.duration = 0.05f;

                var backIdle = hitState.AddTransition(idleState);
                backIdle.hasExitTime = true;
                backIdle.exitTime = 0.9f;
                backIdle.duration = 0.1f;
            }

            if (elimState != null)
            {
                var toElim = rootStateMachine.AddAnyStateTransition(elimState);
                toElim.AddCondition(AnimatorConditionMode.If, 0, "IsEliminated");
                toElim.hasExitTime = false;
                toElim.duration = 0.05f;
            }

            EditorUtility.SetDirty(controller);
            Debug.Log($"[SpriteSheetAutoSlicer] Tạo thành công Animator Controller: {controllerPath}");
        }

        private static void CreateProceduralCharacterSpriteSheet(string characterName)
        {
            string cleanName = CleanFileName(characterName);
            string destDir = "Assets/Art/Characters/" + cleanName;
            Directory.CreateDirectory(destDir);
            string assetPath = $"{destDir}/{cleanName}_spritesheet.png";

            int width = 1536;
            int height = 1024;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;
            tex.SetPixels(colors);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(assetPath, bytes);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            ProcessCharacterAsset(Path.Combine(Application.dataPath, "..", assetPath), characterName);
        }

        private static string CleanFileName(string name)
        {
            string clean = RemoveVietnameseDiacritics(name).Trim();
            clean = clean.Replace(" ", "_").Replace("–", "_").Replace("-", "_");
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                clean = clean.Replace(c.ToString(), "");
            }
            return clean;
        }

        private static string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }
            return text;
        }
    }
}
