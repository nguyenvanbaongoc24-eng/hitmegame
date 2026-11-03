using System;
using UnityEngine;

namespace HitMe.Data
{
    [Serializable]
    public sealed class CharacterDef
    {
        public string id;
        public string displayNameVi;
        public string displayNameEn;
        public Sprite menuPortrait;
        public Sprite menuHead;
        public Sprite gameplayAtlas;
        public Color defaultColor = Color.white;
        public bool isEnabled = true;
        public bool enabled { get => isEnabled; set => isEnabled = value; }
        public string displayName => displayNameVi;

        public CharacterDef() { }

        public CharacterDef(string id, string vi, string en, Color color, bool enabled = true)
        {
            this.id = id;
            this.displayNameVi = vi;
            this.displayNameEn = en;
            this.defaultColor = color;
            this.isEnabled = enabled;
        }

        public string GetLocalizedName(bool isVietnamese)
        {
            return isVietnamese ? displayNameVi : displayNameEn;
        }
    }
}
