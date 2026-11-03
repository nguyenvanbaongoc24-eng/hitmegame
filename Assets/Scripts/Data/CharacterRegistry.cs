using System;
using UnityEngine;

namespace HitMe.Data
{
    public static class CharacterRegistry
    {
        public static readonly CharacterDef[] CanonicalCharacters = new CharacterDef[]
        {
            new CharacterDef("nam_lang", "Nam Làng", "Nam Lang", new Color(0f, 0.898f, 1f), enabled: true),
            new CharacterDef("nu_nang_dong", "Nữ Năng Động", "Active Girl", new Color(1f, 0.843f, 0f), enabled: true),
            new CharacterDef("co_ba", "Cô Ba", "Aunt Ba", new Color(0f, 1f, 0.533f), enabled: true),
            new CharacterDef("anh_teen", "Anh Teen", "Teen Guy", new Color(0.702f, 0.4f, 1f), enabled: true),
            new CharacterDef("quay_boy", "Quậy Boy", "Rebel Boy", new Color(1f, 0.2f, 0.4f), enabled: false), // Sắp ra mắt
            new CharacterDef("cho_vang", "Chó Vàng", "Golden Dog", new Color(1f, 0.6f, 0f), enabled: false), // Sắp ra mắt
            new CharacterDef("meo_mun", "Mèo Mun", "Black Cat", new Color(0.3f, 0.3f, 0.35f), enabled: false) // Sắp ra mắt
        };

        public static CharacterDef GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return CanonicalCharacters[0];
            for (int i = 0; i < CanonicalCharacters.Length; i++)
            {
                if (string.Equals(CanonicalCharacters[i].id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return CanonicalCharacters[i];
                }
            }
            return CanonicalCharacters[0];
        }

        public static CharacterDef GetByIndex(int index)
        {
            int safeIdx = ((index % CanonicalCharacters.Length) + CanonicalCharacters.Length) % CanonicalCharacters.Length;
            return CanonicalCharacters[safeIdx];
        }
    }
}
