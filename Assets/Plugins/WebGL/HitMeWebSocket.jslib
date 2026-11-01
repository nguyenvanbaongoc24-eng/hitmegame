// AG: copy into Assets/Plugins/WebGL/. A Unity GameObject named by the second
// HitMeWsOpen argument must implement OnHitMeWsOpen/Message/Error/Close(string).
// Callback data is "handle|payload" (split only the first separator). Browser
// WebSocket callbacks run on the Unity WebGL main thread. No reconnect here.
mergeInto(LibraryManager.library, {
  HitMeWsOpen: function (urlPtr, receiverPtr) {
    var root = globalThis.HitMeWebGLSockets || (globalThis.HitMeWebGLSockets = { next: 1, sockets: {} });
    var handle = root.next++;
    var receiver = UTF8ToString(receiverPtr);
    var socket;
    try {
      socket = new WebSocket(UTF8ToString(urlPtr));
    } catch (error) {
      SendMessage(receiver, 'OnHitMeWsError', handle + '|connect_failed');
      return 0;
    }
    root.sockets[handle] = socket;
    socket.onopen = function () { SendMessage(receiver, 'OnHitMeWsOpen', String(handle)); };
    socket.onmessage = function (event) {
      if (typeof event.data === 'string') SendMessage(receiver, 'OnHitMeWsMessage', handle + '|' + event.data);
      else SendMessage(receiver, 'OnHitMeWsError', handle + '|non_text_message');
    };
    socket.onerror = function () { SendMessage(receiver, 'OnHitMeWsError', handle + '|socket_error'); };
    socket.onclose = function (event) {
      delete root.sockets[handle];
      SendMessage(receiver, 'OnHitMeWsClose', handle + '|' + event.code + '|' + (event.reason || ''));
    };
    return handle;
  },
  HitMeWsIsOpen: function (handle) {
    var root = globalThis.HitMeWebGLSockets;
    return root && root.sockets[handle] && root.sockets[handle].readyState === WebSocket.OPEN ? 1 : 0;
  },
  HitMeWsSend: function (handle, payloadPtr) {
    var root = globalThis.HitMeWebGLSockets;
    var socket = root && root.sockets[handle];
    if (!socket || socket.readyState !== WebSocket.OPEN) return 0;
    socket.send(UTF8ToString(payloadPtr));
    return 1;
  },
  HitMeWsClose: function (handle) {
    var root = globalThis.HitMeWebGLSockets;
    var socket = root && root.sockets[handle];
    if (!socket) return;
    if (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING) socket.close(1000, 'client_leave');
  }
});
