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
  }
});