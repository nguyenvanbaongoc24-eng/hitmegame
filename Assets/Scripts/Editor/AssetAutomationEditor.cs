using System.IO;
using UnityEngine;
using UnityEditor;
using HitMe.Core;

namespace HitMe.Editor
{
    public static class AssetAutomationEditor
    {
        [MenuItem("Tools/HIT ME/Generate Character & Weapon ScriptableObjects")]
        public static void GenerateScriptableObjects()
        {
            // 1. Ensure folders exist
            EnsureDirectory("Assets/ScriptableObjects");
            EnsureDirectory("Assets/ScriptableObjects/Characters");
            EnsureDirectory("Assets/ScriptableObjects/Weapons");

            // Configure all sprites first
            SpriteImportSetup.ConfigureAllSprites();
            AssetDatabase.Refresh();

            // 2. Create CharacterData: Nam Làng
            CharacterData namLang = ScriptableObject.CreateInstance<CharacterData>();
            namLang.characterId = "nam_lang";
            namLang.characterName = "Nam Làng";
            namLang.description = "Cậu bé làng nhanh nhẹn, trang phục cờ đỏ sao vàng. Lối chơi linh hoạt, tốc độ cơ bản cao.";
            namLang.baseHp = 3;
            namLang.moveSpeed = 5.2f;
            namLang.avatarIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/avatar_round.png");
            namLang.spriteIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
            namLang.spriteMove = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_move.png");
            namLang.spriteAim = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_aim.png");
            namLang.spriteThrowWindup = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_throw_windup.png");
            namLang.spriteThrowRelease = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_throw_release.png");
            namLang.spriteHit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_hit.png");
            namLang.spriteVictory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_victory.png");
            namLang.spriteDefeat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_defeat.png");

            AssetDatabase.CreateAsset(namLang, "Assets/ScriptableObjects/Characters/NamLang.asset");

            // 3. Create CharacterData: Anh Nón Lá
            CharacterData anhNonLa = ScriptableObject.CreateInstance<CharacterData>();
            anhNonLa.characterId = "anh_non_la";
            anhNonLa.characterName = "Anh Nón Lá";
            anhNonLa.description = "Anh chàng đội nón lá truyền thống, vững vàng và kiên định. Phòng thủ tốt, ném đầm tay.";
            anhNonLa.baseHp = 4;
            anhNonLa.moveSpeed = 4.8f;
            anhNonLa.avatarIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_idle.png");
            anhNonLa.spriteIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_idle.png");
            anhNonLa.spriteMove = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_move.png");
            anhNonLa.spriteAim = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_aim.png");
            anhNonLa.spriteThrowWindup = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_throw_windup.png");
            anhNonLa.spriteThrowRelease = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_throw_release.png");
            anhNonLa.spriteHit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_hit.png");
            anhNonLa.spriteVictory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_victory.png");
            anhNonLa.spriteDefeat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_defeat.png");

            AssetDatabase.CreateAsset(anhNonLa, "Assets/ScriptableObjects/Characters/AnhNonLa.asset");

            // 4. Create WeaponData: Dép Tổ Ong
            WeaponData dep = ScriptableObject.CreateInstance<WeaponData>();
            dep.weaponId = "dep_to_ong";
            dep.weaponName = "Dép Tổ Ong";
            dep.description = "Vũ khí huyền thoại quốc dân. Độ cân bằng tuyệt đối, đường đạn thẳng và uy lực.";
            dep.damage = 1;
            dep.speed = 14.0f;
            dep.hitRadius = 0.5f;
            dep.trajectoryType = WeaponTrajectoryType.Straight;
            dep.icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_icon.png");
            dep.projectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_projectile.png");
            dep.sfxThrow = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_throw.wav");
            dep.sfxImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit_bop.wav");

            AssetDatabase.CreateAsset(dep, "Assets/ScriptableObjects/Weapons/DepToOng.asset");

            // 5. Create WeaponData: Chảo Chống Dính
            WeaponData chao = ScriptableObject.CreateInstance<WeaponData>();
            chao.weaponId = "chao_chong_dinh";
            chao.weaponName = "Chảo Chống Dính";
            chao.description = "Chảo gang gia truyền, diện tích trúng đòn lớn hơn. Tiếng boong giòn tan khi trúng đích.";
            chao.damage = 1;
            chao.speed = 12.5f;
            chao.hitRadius = 0.65f;
            chao.trajectoryType = WeaponTrajectoryType.Straight;
            chao.icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/chao_icon.png");
            chao.projectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/chao_projectile.png");
            chao.sfxThrow = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_throw.wav");
            chao.sfxImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit_bop.wav");

            AssetDatabase.CreateAsset(chao, "Assets/ScriptableObjects/Weapons/ChaoChongDinh.asset");

            // 6. Create WeaponData: Vợt Pickleball
            WeaponData pickle = ScriptableObject.CreateInstance<WeaponData>();
            pickle.weaponId = "vot_pickleball";
            pickle.weaponName = "Vợt Pickleball";
            pickle.description = "Vũ khí thời thượng tốc độ cao. Bay nhanh và bất ngờ khiến đối thủ khó phản xạ.";
            pickle.damage = 1;
            pickle.speed = 16.0f;
            pickle.hitRadius = 0.45f;
            pickle.trajectoryType = WeaponTrajectoryType.Straight;
            pickle.icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pickleball_icon.png");
            pickle.projectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pickleball_projectile.png");
            pickle.sfxThrow = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_throw.wav");
            pickle.sfxImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit_bop.wav");

            AssetDatabase.CreateAsset(pickle, "Assets/ScriptableObjects/Weapons/VotPickleball.asset");

            AssetDatabase.SaveAssets();
            Debug.Log("Successfully created ScriptableObjects for Characters (Nam Làng, Anh Nón Lá) and Weapons (Dép, Chảo, Vợt Pickleball).");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
