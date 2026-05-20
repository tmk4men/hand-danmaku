// Hand Danmaku — 2人専用オンライン中継 (Cloudflare Durable Objects + WebSocket)
// 役割: 合言葉(room)ごとに1つの Durable Object を作り、最大2人を繋いで
//       受け取った JSON メッセージを「相手」へそのまま転送するだけの薄い中継。
// 無料プラン(SQLiteバックエンド)で動く。WebSocket Hibernation API を使用。

export class Room {
  constructor(state, env) {
    this.state = state; // DurableObjectState
    this.env = env;
  }

  async fetch(request) {
    if (request.headers.get("Upgrade") !== "websocket") {
      return new Response("expected websocket", { status: 426 });
    }

    const peers = this.state.getWebSockets();
    if (peers.length >= 2) {
      return new Response("room full", { status: 403 });
    }

    const { 0: client, 1: server } = new WebSocketPair();

    // 最初に入った人が host(=攻撃側), 2人目が guest(=防御側)
    const role = peers.length === 0 ? "host" : "guest";

    // Hibernation API: tag に role を保存しておくと、休眠から復帰しても役割が分かる
    this.state.acceptWebSocket(server, [role]);

    // 入った本人へ役割を通知
    server.send(JSON.stringify({ type: "role", role, peers: peers.length + 1 }));

    // 既存の相手へ「来たよ」を通知
    for (const p of peers) {
      try { p.send(JSON.stringify({ type: "peer-joined" })); } catch (_) {}
    }

    return new Response(null, { status: 101, webSocket: client });
  }

  // 受信したらそのまま「自分以外」へ転送（2人なので実質1人へ）
  webSocketMessage(ws, message) {
    for (const p of this.state.getWebSockets()) {
      if (p !== ws) {
        try { p.send(message); } catch (_) {}
      }
    }
  }

  webSocketClose(ws, code, reason, wasClean) {
    try { ws.close(code, "closing"); } catch (_) {}
    for (const p of this.state.getWebSockets()) {
      if (p !== ws) {
        try { p.send(JSON.stringify({ type: "peer-left" })); } catch (_) {}
      }
    }
  }

  webSocketError(ws, error) {
    // 何もしない（close で後処理される）
  }
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    const room = url.searchParams.get("room");
    if (!room) {
      return new Response("missing ?room=CODE", { status: 400 });
    }
    // room コード → 同じ名前なら必ず同じ Durable Object へ
    const id = env.ROOM.idFromName(room.toUpperCase());
    const stub = env.ROOM.get(id);
    return stub.fetch(request);
  },
};
