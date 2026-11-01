mergeInto(LibraryManager.library, {
  MarkHomeShown: function () {
    window.__hitmeHomeShown = performance.now();
    console.log('[HITME] HOME_SHOWN timestamp=' + window.__hitmeHomeShown);
  }
});
