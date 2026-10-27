using System.Collections;
using HitMe.Core;
using HitMe.Gameplay;
using HitMe.UI;
using UnityEngine;

namespace HitMe.Presentation
{
    /// <summary>Development-build telemetry for startup and viewport recovery.</summary>
    public sealed class WebGLRuntimeDiagnostics : MonoBehaviour
    {
        [SerializeField] private MatchStateMachine match;
        private bool _gameplayLogged;

        private void Awake()
        {
            Debug.Log("BOOT_STARTED");
            if (match == null) match = GetComponent<MatchStateMachine>();
        }

        private IEnumerator Start()
        {
            Debug.Log("UNITY_INITIALIZED");
            yield return null;
            LogStartupState();
            yield return new WaitForSeconds(1f);
            LogViewportState();
        }

        private void Update()
        {
            if (!_gameplayLogged && match != null && match.currentState != MatchState.Initializing)
            {
                _gameplayLogged = true;
                Debug.Log("GAMEPLAY_SCENE_ENTERED state=" + match.currentState);
            }
        }

        private void LogStartupState()
        {
            Debug.Log("STARTUP_SCENE_LOADED name=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
            var ui = FindAnyObjectByType<UIManager>();
            Debug.Log(ui != null ? "MAIN_MENU_CONTROLLER_FOUND" : "MAIN_MENU_CONTROLLER_MISSING");
            var canvas = FindAnyObjectByType<Canvas>();
            Debug.Log(canvas != null ? "UI_ROOT_CREATED" : "UI_ROOT_MISSING");
            if (canvas != null)
                Debug.Log("CANVAS_ENABLED=" + canvas.isActiveAndEnabled + " rect=" + canvas.pixelRect + " mode=" + canvas.renderMode);
            var camera = Camera.main;
            Debug.Log(camera != null ? "MAIN_CAMERA_FOUND" : "MAIN_CAMERA_MISSING");
            if (camera != null)
                Debug.Log("CAMERA viewport=" + camera.rect + " pixel=" + camera.pixelRect + " ortho=" + camera.orthographic + " size=" + camera.orthographicSize + " pos=" + camera.transform.position + " mask=" + camera.cullingMask);
            Debug.Log(ui != null && ui.mainMenuPanel != null && ui.mainMenuPanel.activeInHierarchy ? "MAIN_MENU_READY" : "MAIN_MENU_NOT_READY");
        }

        private void LogViewportState()
        {
            Debug.Log("VIEWPORT screen=" + Screen.width + "x" + Screen.height + " dpi=" + Screen.dpi);
            LogSprite("CombatField");
            LogSprite("ArenaBorder");
            LogSprite("FarMountains");
            LogActor("PlayerActor");
            LogActor("BotActor");
        }

        private static void LogSprite(string objectName)
        {
            var go = GameObject.Find(objectName);
            var renderer = go != null ? go.GetComponent<SpriteRenderer>() : null;
            if (renderer == null)
            {
                Debug.LogError("RUNTIME_SPRITE_MISSING name=" + objectName);
                return;
            }
            Debug.Log("RUNTIME_SPRITE name=" + objectName + " enabled=" + renderer.enabled + " visible=" + renderer.isVisible + " sprite=" + (renderer.sprite != null ? renderer.sprite.name : "null") + " bounds=" + renderer.bounds + " layer=" + renderer.sortingLayerName + "/" + renderer.sortingOrder);
        }

        private static void LogActor(string objectName)
        {
            var actor = GameObject.Find(objectName)?.GetComponent<ActorView>();
            if (actor == null)
            {
                Debug.LogError("RUNTIME_ACTOR_MISSING name=" + objectName);
                return;
            }
            Debug.Log("RUNTIME_ACTOR name=" + objectName + " active=" + actor.gameObject.activeInHierarchy + " pos=" + actor.transform.position + " renderer=" + (actor.characterRenderer != null && actor.characterRenderer.enabled) + " sprite=" + (actor.characterRenderer != null && actor.characterRenderer.sprite != null ? actor.characterRenderer.sprite.name : "null"));
        }
    }
}
