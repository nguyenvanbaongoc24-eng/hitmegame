mergeInto(LibraryManager.library, {
  MarkHomeShown: function () {
    window.__hitmeHomeShown = performance.now();
    console.log('[HITME] HOME_SHOWN timestamp=' + window.__hitmeHomeShown);
  },
  GetBuildCommitFromJS: function () {
    var commit = window.__hitmeBuildCommit || "dev";
    var bufferSize = lengthBytesUTF8(commit) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(commit, buffer, bufferSize);
    return buffer;
  },
  IsE2EEnabled: function () {
    if (typeof window !== "undefined" && window.location && window.location.search) {
      return (window.location.search.indexOf("e2e=1") !== -1) ? 1 : 0;
    }
    return 0;
  },
  EmitUnityToWebJson: function (jsonStrPtr) {
    if (typeof window !== "undefined" && window.hitmeUi && typeof window.hitmeUi.onUnity === "function") {
      try {
        var str = UTF8ToString(jsonStrPtr);
        var evt = JSON.parse(str);
        window.hitmeUi.onUnity(evt);
      } catch (e) {
        console.error("[HITME WebBridge] EmitUnityToWebJson error:", e);
      }
    }
  },
  UpdateHitMeStateJson: function (jsonStrPtr) {
    if (typeof window !== "undefined" && window.location && window.location.search && window.location.search.indexOf("e2e=1") !== -1) {
      try {
        var str = UTF8ToString(jsonStrPtr);
        var state = JSON.parse(str);
        var canvas = document.querySelector("#unity-canvas");
        var vv = window.visualViewport;
        var viewW = vv ? vv.width : window.innerWidth;
        var viewH = vv ? vv.height : window.innerHeight;
        var sx = canvas && canvas.width ? (viewW / canvas.width) : 1;
        var sy = canvas && canvas.height ? (viewH / canvas.height) : 1;
        if (state && state.canvas) {
          state.canvas.cssW = viewW;
          state.canvas.cssH = viewH;
          state.canvas.backingW = canvas ? canvas.width : viewW;
          state.canvas.backingH = canvas ? canvas.height : viewH;
          state.canvas.dpr = window.devicePixelRatio || 1;
        }
        if (state && state.match && state.match.playableRect) {
          state.match.playableRect.x *= sx;
          state.match.playableRect.y *= sy;
          state.match.playableRect.w *= sx;
          state.match.playableRect.h *= sy;
        }
        if (state && state.match && Array.isArray(state.match.players)) {
          for (var i = 0; i < state.match.players.length; i++) {
            if (state.match.players[i].rect) {
              state.match.players[i].rect.x *= sx;
              state.match.players[i].rect.y *= sy;
              state.match.players[i].rect.w *= sx;
              state.match.players[i].rect.h *= sy;
            }
          }
        }
        window.__hitmeState = state;
      } catch (e) {
        console.error("[HITME] UpdateHitMeStateJson error:", e);
      }
    }
  },
  UpdateHitMeUiDumpJson: function (jsonStrPtr) {
    if (typeof window !== "undefined" && window.location && window.location.search && window.location.search.indexOf("e2e=1") !== -1) {
      try {
        var str = UTF8ToString(jsonStrPtr);
        var dump = JSON.parse(str);
        var canvas = document.querySelector("#unity-canvas");
        var vv = window.visualViewport;
        var viewW = vv ? vv.width : window.innerWidth;
        var viewH = vv ? vv.height : window.innerHeight;
        var sx = canvas && canvas.width ? (viewW / canvas.width) : 1;
        var sy = canvas && canvas.height ? (viewH / canvas.height) : 1;
        if (Array.isArray(dump)) {
          for (var i = 0; i < dump.length; i++) {
            if (dump[i].rect) {
              dump[i].rect.x *= sx;
              dump[i].rect.y *= sy;
              dump[i].rect.w *= sx;
              dump[i].rect.h *= sy;
            }
          }
        }
        window.__hitmeUiDump = function () { return dump; };
      } catch (e) {
        console.error("[HITME] UpdateHitMeUiDumpJson error:", e);
      }
    }
  }
});
