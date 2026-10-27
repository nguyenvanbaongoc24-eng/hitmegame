using System.IO;
using UnityEngine;
using UnityEditor;
using HitMe.Core;

namespace HitMe.Editor
{
    /// <summary>
    /// HIT ME - Tự động hóa tạo WeaponData ScriptableObjects cho toàn bộ 9 vũ khí chuẩn MD (03_WEAPON_SYSTEM.md)
    /// </summary>
    public static class WeaponAutomationEditor
    {
        private struct WeaponDefinition
        {
            public string id;
            public string name;
            public string desc;
            public float speed;
            public float hitRadius;
            public int damage;
            public WeaponTrajectoryType trajectory;
            public string spritePrefix;
        }

        private static readonly WeaponDefinition[] AllWeapons = new WeaponDefinition[]
        {
            new WeaponDefinition
            {
                id = "slipper",
                name = "Dép Tổ Ong",
                desc = "Vũ khí huyền thoại quốc dân. Độ cân bằng tuyệt đối, đường đạn thẳng và uy lực.",
                speed = 14.0f,
                hitRadius = 0.5f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "dep_to_ong"
            },
            new WeaponDefinition
            {
                id = "frying_pan",
                name = "Chảo Chống Dính",
                desc = "Chảo gang gia truyền, diện tích trúng đòn lớn hơn. Tiếng boong giòn tan khi trúng đích.",
                speed = 12.5f,
                hitRadius = 0.65f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "chao_chong_dinh"
            },
            new WeaponDefinition
            {
                id = "pickleball_paddle",
                name = "Vợt Pickleball",
                desc = "Vũ khí thời thượng mặt đặc, tốc độ cao khiến đối thủ khó phản xạ.",
                speed = 16.0f,
                hitRadius = 0.45f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "vot_pickleball"
            },
            new WeaponDefinition
            {
                id = "beer_mug",
                name = "Cốc Bia Hơi",
                desc = "Cốc thủy tinh đầy bọt bia mát lạnh, ném đầm tay và bốc lửa.",
                speed = 13.0f,
                hitRadius = 0.55f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "coc_bia"
            },
            new WeaponDefinition
            {
                id = "television",
                name = "Tivi Cổ",
                desc = "Tivi CRT màn hình cong đỏ rực. Sức nặng ngàn cân áp đảo tâm lý đối thủ.",
                speed = 10.5f,
                hitRadius = 0.80f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "tivi_crt"
            },
            new WeaponDefinition
            {
                id = "refrigerator",
                name = "Tủ Lạnh Mint",
                desc = "Tủ lạnh 2 cánh bo góc phong cách retro. Kích thước khổng lồ chiếm trọn tầm nhìn.",
                speed = 9.5f,
                hitRadius = 0.85f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "tu_lanh"
            },
            new WeaponDefinition
            {
                id = "bamboo_pipe",
                name = "Điếu Cày",
                desc = "Điếu cày tre truyền thống. Tiếng rít xé gió và tốc độ bay chớp nhoáng.",
                speed = 15.5f,
                hitRadius = 0.45f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "dieu_cay"
            },
            new WeaponDefinition
            {
                id = "football",
                name = "Quả Bóng Đá",
                desc = "Trái bóng tròn thân thuộc trên sân làng, quỹ đạo bay linh hoạt và nảy lửa.",
                speed = 14.5f,
                hitRadius = 0.55f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "bong_da"
            },
            new WeaponDefinition
            {
                id = "claw_hammer",
                name = "Búa Đóng Đinh",
                desc = "Búa đầu thép móng nhổ đinh chắc nịch, đòn ném nặng trịch dứt khoát.",
                speed = 13.5f,
                hitRadius = 0.50f,
                damage = 1,
                trajectory = WeaponTrajectoryType.Straight,
                spritePrefix = "bua_dong_dinh"
            }
        };

        [MenuItem("Tools/HIT ME/2.5D Pipeline/Generate All 9 Weapons ScriptableObjects", false, 40)]
        public static void GenerateAllWeapons()
        {
            string outDir = "Assets/ScriptableObjects/Weapons";
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            AudioClip sfxThrow = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_throw.wav");
            AudioClip sfxImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit_bop.wav");

            foreach (var def in AllWeapons)
            {
                string assetPath = $"{outDir}/{def.id}.asset";
                WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<WeaponData>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }

                data.weaponId = def.id;
                data.weaponName = def.name;
                data.description = def.desc;
                data.speed = def.speed;
                data.hitRadius = def.hitRadius;
                data.damage = def.damage;
                data.trajectoryType = def.trajectory;
                data.sfxThrow = sfxThrow;
                data.sfxImpact = sfxImpact;

                // Load sprites from Assets/Art/Weapons
                string iconPath = $"Assets/Art/Weapons/{def.spritePrefix}_icon.png";
                string projPath = $"Assets/Art/Weapons/{def.spritePrefix}_projectile.png";

                data.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                data.projectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>(projPath);

                EditorUtility.SetDirty(data);
                Debug.Log($"[WeaponAutomation] Đã tạo ScriptableObject vũ khí: {def.name} ({assetPath})");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WeaponAutomation] Hoàn tất tạo toàn bộ 9 vũ khí ScriptableObjects!");
        }
    }
}
