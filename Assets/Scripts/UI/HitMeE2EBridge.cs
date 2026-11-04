using System;
using System.Collections.Generic;
using HitMe.Core;
using HitMe.Data;
using HitMe.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HitMe.UI
{
    [Serializable]
    public class E2ERosterItem
    {
        public string id;
        public string name;
        public bool enabled;
    }

    [Serializable]
    public class E2ECanvasInfo
    {
        public float cssW;
        public float cssH;
        public float backingW;
        public float backingH;
        public float dpr;
    }

    [Serializable]
    public class E2ECssRect
    {
        public float x;
        public float y;
        public float w;
        public float h;
    }

    [Serializable]
    public class E2EPlayerState
    {
        public string id;
        public string name;
        public string characterId;
        public int slot;
        public int hp;
        public bool alive;
        public bool isLocal;
        public E2ECssRect rect = new E2ECssRect();
    }

    [Serializable]
    public class E2EMatchEvent
    {
        public float t;
        public string type; // "throw" | "hit" | "ko" | "win"
        public string sourceId;
        public string targetId;
    }

    [Serializable]
    public class E2EMatchState
    {
        public int round = 1;
        public string phase = "placing";
        public float timeLeft = 5.0f;
        public E2ECssRect playableRect = new E2ECssRect();
        public List<E2EPlayerState> players = new List<E2EPlayerState>();
        public List<E2EMatchEvent> events = new List<E2EMatchEvent>();
    }

    [Serializable]
    public class E2EStateRoot
    {
        public string build = "dev";
        public string version = "1.0.0";
        public string screen = "home";
        public string selectedCharacterId = "nam_lang";
        public string selectedWeaponId = "dep_to_ong";
        public List<E2ERosterItem> roster = new List<E2ERosterItem>();
        public List<E2ERosterItem> weapons = new List<E2ERosterItem>();
        public E2ECanvasInfo canvas = new E2ECanvasInfo();
        public E2EMatchState match = new E2EMatchState();
    }

    [Serializable]
    public class E2EUiElement
    {
        public string name;
        public string text;
        public string spriteName;
        public E2ECssRect rect = new E2ECssRect();
        public bool interactable;
        public bool visible;
    }

    [Serializable]
    public class E2EUiDumpList
    {
        public List<E2EUiElement> items = new List<E2EUiElement>();
    }

    public sealed class HitMeE2EBridge : MonoBehaviour
    {
        public static HitMeE2EBridge Instance { get; private set; }

        private readonly E2EStateRoot _root = new E2EStateRoot();
        private readonly List<E2EMatchEvent> _eventsBuffer = new List<E2EMatchEvent>();
        private Camera _mainCam;
        private Canvas _canvas;
        private ProductionUIController _ui;
        private MatchController _match;
        private float _lastPushTime;
        private bool _dirty = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitStaticData();
        }

        private void OnEnable()
        {
            MatchController.OnMatchE2EEvent += RecordEvent;
        }

        private void OnDisable()
        {
            MatchController.OnMatchE2EEvent -= RecordEvent;
        }

        private void Start()
        {
            _mainCam = Camera.main;
            _ui = FindFirstObjectByType<ProductionUIController>();
            _match = FindFirstObjectByType<MatchController>();
            _canvas = _ui != null ? _ui.GetComponent<Canvas>() : FindFirstObjectByType<Canvas>();
            MarkDirty();
        }

        private void InitStaticData()
        {
            _root.build = "Release_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            _root.version = "1.0.0";

            _root.roster.Clear();
            for (int i = 0; i < CharacterRegistry.CanonicalCharacters.Length; i++)
            {
                var c = CharacterRegistry.CanonicalCharacters[i];
                _root.roster.Add(new E2ERosterItem { id = c.id, name = c.displayName, enabled = c.enabled });
            }

            _root.weapons.Clear();
            _root.weapons.Add(new E2ERosterItem { id = "dep_to_ong", name = "Dép Tổ Ong", enabled = true });
            _root.weapons.Add(new E2ERosterItem { id = "chao", name = "Chảo", enabled = true });
            _root.weapons.Add(new E2ERosterItem { id = "vot", name = "Vợt", enabled = true });
        }

        public void RecordEvent(string type, string sourceId, string targetId)
        {
            var ev = new E2EMatchEvent
            {
                t = Time.time,
                type = type,
                sourceId = sourceId ?? "",
                targetId = targetId ?? ""
            };
            _eventsBuffer.Add(ev);
            if (_eventsBuffer.Count > 50) _eventsBuffer.RemoveAt(0);
            MarkDirty();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        private void LateUpdate()
        {
            if (Time.time - _lastPushTime < 0.08f && !_dirty) return;
            _lastPushTime = Time.time;
            _dirty = false;
            PushStateToJs();
        }

        public void PushStateToJs()
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_ui == null) _ui = FindFirstObjectByType<ProductionUIController>();
            if (_match == null) _match = FindFirstObjectByType<MatchController>();

            float screenW = Mathf.Max(1f, Screen.width);
            float screenH = Mathf.Max(1f, Screen.height);
            float dpr = Mathf.Max(1f, Screen.dpi > 0 ? (Screen.dpi / 160f) : 2.5f);

            // CSS dimensions
            float cssW = screenW / dpr;
            float cssH = screenH / dpr;

            _root.canvas.backingW = screenW;
            _root.canvas.backingH = screenH;
            _root.canvas.cssW = cssW;
            _root.canvas.cssH = cssH;
            _root.canvas.dpr = dpr;

            if (_ui != null)
            {
                if (_ui.dialogSettings != null && _ui.dialogSettings.activeSelf)
                {
                    _root.screen = "settings";
                }
                else
                {
                    _root.screen = ScreenToString(_ui.CurrentScreen);
                }
                _root.selectedCharacterId = _ui.SelectedCharacterId ?? "nam_lang";
                _root.selectedWeaponId = _ui.SelectedWeaponId ?? "dep_to_ong";
            }

            if (_match != null && _match.State != null)
            {
                var s = _match.State;
                _root.match.round = s.RoundNumber;
                _root.match.phase = PhaseToString(s.Phase);
                _root.match.timeLeft = _match.SecondsRemaining;

                // Playable rect in CSS coordinates
                if (_mainCam != null)
                {
                    Vector3 minWorld = new Vector3(ArenaCoordinateMapper.PlayMinX, ArenaCoordinateMapper.PlayMinY, 0f);
                    Vector3 maxWorld = new Vector3(ArenaCoordinateMapper.PlayMaxX, ArenaCoordinateMapper.PlayMaxY, 0f);
                    Vector3 minScreen = _mainCam.WorldToScreenPoint(minWorld);
                    Vector3 maxScreen = _mainCam.WorldToScreenPoint(maxWorld);

                    float leftCss = Mathf.Min(minScreen.x, maxScreen.x);
                    float rightCss = Mathf.Max(minScreen.x, maxScreen.x);
                    float topCss = screenH - Mathf.Max(minScreen.y, maxScreen.y);
                    float bottomCss = screenH - Mathf.Min(minScreen.y, maxScreen.y);

                    _root.match.playableRect.x = leftCss;
                    _root.match.playableRect.y = topCss;
                    _root.match.playableRect.w = rightCss - leftCss;
                    _root.match.playableRect.h = bottomCss - topCss;
                }

                _root.match.players.Clear();
                for (int i = 0; i < s.Players.Count; i++)
                {
                    var p = s.Players[i];
                    var ep = new E2EPlayerState
                    {
                        id = p.Id,
                        name = p.Name,
                        characterId = CharacterRegistry.GetByIndex(i).id,
                        slot = i,
                        hp = p.Hp,
                        alive = p.IsAlive,
                        isLocal = p.Id == "player" || i == 0
                    };

                    if (_mainCam != null)
                    {
                        Vector3 pWorld = new Vector3(p.Position.X, p.Position.Y, 0f);
                        Vector3 pScreen = _mainCam.WorldToScreenPoint(pWorld);
                        float halfW = 0.55f * 150f;
                        float halfH = 0.75f * 150f;
                        ep.rect.x = pScreen.x - halfW;
                        ep.rect.y = (screenH - pScreen.y) - halfH;
                        ep.rect.w = halfW * 2f;
                        ep.rect.h = halfH * 2f;
                    }
                    _root.match.players.Add(ep);
                }

                _root.match.events = new List<E2EMatchEvent>(_eventsBuffer);
            }

            string json = JsonUtility.ToJson(_root);
            HitMeWebBridge.UpdateHitMeStateJson(json);

            // Push UI Dump
            DumpUi(dpr, screenH);
        }

        private void DumpUi(float dpr, float screenH)
        {
            var dump = new List<E2EUiElement>();
            var selectables = FindObjectsByType<Selectable>(FindObjectsSortMode.None);
            for (int i = 0; i < selectables.Length; i++)
            {
                var sel = selectables[i];
                if (sel == null || !sel.gameObject.activeInHierarchy) continue;
                var rt = sel.GetComponent<RectTransform>();
                if (rt == null) continue;

                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);

                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                for (int c = 0; c < 4; c++)
                {
                    Vector2 sPoint = RectTransformUtility.WorldToScreenPoint(null, corners[c]);
                    if (sPoint.x < minX) minX = sPoint.x;
                    if (sPoint.x > maxX) maxX = sPoint.x;
                    if (sPoint.y < minY) minY = sPoint.y;
                    if (sPoint.y > maxY) maxY = sPoint.y;
                }

                // Pass raw screen pixels; WebBridge.jslib applies (cssW / canvas.width) accurately
                float cssX = minX;
                float cssY = screenH - maxY;
                float cssW = maxX - minX;
                float cssH = maxY - minY;

                var textComp = sel.GetComponentInChildren<TMP_Text>(true);
                string text = textComp != null ? textComp.text : "";
                var img = sel.GetComponent<Image>();
                string spriteName = (img != null && img.sprite != null) ? img.sprite.name : "";

                dump.Add(new E2EUiElement
                {
                    name = sel.gameObject.name,
                    text = text,
                    spriteName = spriteName,
                    interactable = sel.interactable,
                    visible = true,
                    rect = new E2ECssRect { x = cssX, y = cssY, w = cssW, h = cssH }
                });
            }

            string dumpJson = JsonUtility.ToJson(new E2EUiDumpList { items = dump });
            // Extract the inner list for JS
            int listStart = dumpJson.IndexOf("\"items\":", StringComparison.Ordinal);
            if (listStart != -1)
            {
                string rawArray = dumpJson.Substring(listStart + 8);
                if (rawArray.EndsWith("}")) rawArray = rawArray.Substring(0, rawArray.Length - 1);
                HitMeWebBridge.UpdateHitMeUiDumpJson(rawArray);
            }
        }

        private static string ScreenToString(ProductionScreen screen)
        {
            switch (screen)
            {
                case ProductionScreen.Home: return "home";
                case ProductionScreen.BotTraining: return "select";
                case ProductionScreen.Lobby: return "lobby";
                case ProductionScreen.CreateRoom: return "lobby";
                case ProductionScreen.JoinRoom: return "lobby";
                case ProductionScreen.Gameplay: return "match";
                case ProductionScreen.Result: return "result";
                default: return "home";
            }
        }

        private static string PhaseToString(Phase phase)
        {
            switch (phase)
            {
                case Phase.Placing: return "placing";
                case Phase.Reveal: return "reveal";
                case Phase.Throwing: return "throw";
                case Phase.Resolve: return "resolve";
                case Phase.RoundEnd: return "resolve";
                case Phase.MatchEnd: return "end";
                default: return "placing";
            }
        }
    }
}
