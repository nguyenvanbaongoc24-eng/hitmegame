using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.Game
{
    [Serializable]
    public sealed class MapConfig
    {
        public string id = "map_street_vietnam";
        public string nameVi = "Phố Cổ Nắng Gắt";
        public string nameEn = "Sunny Old Town Street";
        public string descriptionVi = "Đường gạch đá nẻ giữa phố phường thân quen";
        public string descriptionEn = "Cracked stone street in the bustling old quarter";
        public string spriteName = "ARENA";
        public string musicTrack = "bgm_match_loop";
        public float arenaWidth = 9.0f;
        public float arenaHeight = 11.4f;

        public string GetDisplayName(bool isEnglish)
        {
            return isEnglish ? nameEn : nameVi;
        }
    }

    [Serializable]
    internal sealed class MapConfigListWrapper
    {
        public List<MapConfig> maps;
    }

    public sealed class MapManager : MonoBehaviour
    {
        private static MapManager _instance;
        public static MapManager Instance => _instance;

        private readonly List<MapConfig> _maps = new List<MapConfig>();
        private int _selectedMapIndex = 0;

        public IReadOnlyList<MapConfig> Maps => _maps;
        public MapConfig CurrentMap => _maps.Count > 0 ? _maps[_selectedMapIndex] : null;
        public int SelectedMapIndex => _selectedMapIndex;

        public event Action<MapConfig> OnMapChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            LoadMaps();
        }

        public void LoadMaps()
        {
            _maps.Clear();
            var jsonAsset = Resources.Load<TextAsset>("Maps");
            if (jsonAsset != null && !string.IsNullOrEmpty(jsonAsset.text))
            {
                try
                {
                    string wrapped = "{\"maps\":" + jsonAsset.text + "}";
                    var wrapper = JsonUtility.FromJson<MapConfigListWrapper>(wrapped);
                    if (wrapper != null && wrapper.maps != null)
                    {
                        _maps.AddRange(wrapper.maps);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[MapManager] Error parsing Maps.json: " + ex.Message);
                }
            }

            if (_maps.Count == 0)
            {
                _maps.Add(new MapConfig());
            }

            // Restore saved map index from PlayerPrefs if available
            string savedId = PlayerPrefs.GetString("HitMe_SelectedMapId", "map_street_vietnam");
            int idx = _maps.FindIndex(m => m.id == savedId);
            _selectedMapIndex = idx >= 0 ? idx : 0;
        }

        public MapConfig CycleNextMap()
        {
            if (_maps.Count == 0) return null;
            _selectedMapIndex = (_selectedMapIndex + 1) % _maps.Count;
            SaveSelectedMap();
            OnMapChanged?.Invoke(CurrentMap);
            ApplyMapToScene(CurrentMap);
            return CurrentMap;
        }

        public void SetMapById(string mapId)
        {
            if (_maps.Count == 0) return;
            int idx = _maps.FindIndex(m => m.id == mapId);
            if (idx >= 0 && idx != _selectedMapIndex)
            {
                _selectedMapIndex = idx;
                SaveSelectedMap();
                OnMapChanged?.Invoke(CurrentMap);
                ApplyMapToScene(CurrentMap);
            }
        }

        private void SaveSelectedMap()
        {
            if (CurrentMap != null)
            {
                PlayerPrefs.SetString("HitMe_SelectedMapId", CurrentMap.id);
                PlayerPrefs.Save();
            }
        }

        public void ApplyMapToScene(MapConfig map)
        {
            if (map == null) return;
            var arenaBg = GameObject.Find("ArenaBackground");
            if (arenaBg != null)
            {
                var sr = arenaBg.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    var newSprite = Resources.Load<Sprite>(map.spriteName);
                    if (newSprite == null)
                    {
                        // Try loading from Sprites/ if Resources doesn't have it directly
                        var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                        foreach (var s in allSprites)
                        {
                            if (s.name.Equals(map.spriteName, StringComparison.OrdinalIgnoreCase))
                            {
                                newSprite = s;
                                break;
                            }
                        }
                    }

                    if (newSprite != null)
                    {
                        sr.sprite = newSprite;
                    }
                }
            }
        }
    }
}
