// Hand Danmaku — クライアント側ネットワークモジュール（ゲーム非依存）
// index.html にこのファイルを読み込むか、中身を <script> に貼り付けて使う。
// グローバル window.DanmakuNet を生やすだけ（ESモジュール不要 = どの静的ホストでも動く）。
(function () {
  // ↓↓↓ デプロイ後に自分の Worker の URL に書き換える（wss:// で）↓↓↓
  const DEFAULT_URL = "wss://danmaku-relay.<your-subdomain>.workers.dev";

  // 合言葉(room)で接続する。
  // opts: { url?, onRole, onPeerJoined, onPeerLeft, onMessage, onClose }
  //  - onRole(role, peers): role は "host"(攻撃側) か "guest"(防御側)
  //  - onMessage(msg): 相手から届いた JSON。{type:...} で分岐して使う
  function joinRoom(roomCode, opts) {
    opts = opts || {};
    const base = opts.url || DEFAULT_URL;
    const wsUrl = base + "?room=" + encodeURIComponent(String(roomCode).toUpperCase());

    let ws = null;
    let role = null;
    let closedByUs = false;
    let reconnectTimer = null;

    function connect() {
      ws = new WebSocket(wsUrl);

      ws.onmessage = function (ev) {
        let msg;
        try { msg = JSON.parse(ev.data); } catch (_) { return; }
        if (msg.type === "role")        { role = msg.role; opts.onRole && opts.onRole(msg.role, msg.peers); return; }
        if (msg.type === "peer-joined") { opts.onPeerJoined && opts.onPeerJoined(); return; }
        if (msg.type === "peer-left")   { opts.onPeerLeft && opts.onPeerLeft(); return; }
        opts.onMessage && opts.onMessage(msg);
      };

      ws.onclose = function () {
        if (closedByUs) return;
        opts.onClose && opts.onClose();
        // 自動再接続（回線が一瞬切れた時用）
        reconnectTimer = setTimeout(connect, 1000);
      };

      ws.onerror = function () { try { ws.close(); } catch (_) {} };
    }

    connect();

    return {
      get role() { return role; },
      get ready() { return ws && ws.readyState === 1; },
      // 相手へ送る。混雑時に古い位置更新を捨てたい場合は呼び出し側で間引く。
      send: function (obj) {
        if (ws && ws.readyState === 1) {
          ws.send(JSON.stringify(obj));
          return true;
        }
        return false;
      },
      close: function () {
        closedByUs = true;
        clearTimeout(reconnectTimer);
        try { ws && ws.close(); } catch (_) {}
      },
    };
  }

  window.DanmakuNet = { joinRoom };
})();
