# Hand Danmaku — 2人オンライン中継（Cloudflare Durable Objects・無料）

合言葉（ルームコード）で友人と2人だけ繋がるための薄い WebSocket 中継。
攻撃側 / 防御側の同時プレイ用。**Workers 無料プランで動く**（SQLiteバックエンドDO）。

## なぜこれ
- 2人とも日本から繋ぐと DO はほぼ東京に作られる → 中継で増える遅延は片道 1〜10ms 程度（≒1フレーム）。
- 常時起動サーバー不要・課金なし・メンテなし。ハイバネーションで休眠中は課金されない。
- 無料枠: 1日10万リクエスト。WebSocket受信は20:1課金なので、30分セッションでも約5,400リクエスト換算 → **1日18セッション回しても無料**。

## デプロイ手順（コピペ）

```bash
cd multiplayer
npm install -g wrangler        # 初回のみ
wrangler login                 # ブラウザでCloudflareにログイン
wrangler deploy
```

デプロイ後に出る URL（例 `https://danmaku-relay.<your-subdomain>.workers.dev`）を控える。

ローカルで試すだけなら:
```bash
wrangler dev    # ws://localhost:8787 で動く
```

## クライアント設定
`net-client.js` の `DEFAULT_URL` を、上で控えた URL の `https` を `wss` に変えたものに書き換える:
```js
const DEFAULT_URL = "wss://danmaku-relay.<your-subdomain>.workers.dev";
```

## 使い方（最小例）
```html
<script src="multiplayer/net-client.js"></script>
<script>
  const conn = DanmakuNet.joinRoom("ABCD", {
    onRole: (role) => console.log("自分は", role),     // "host"=攻撃側 / "guest"=防御側
    onPeerJoined: () => console.log("相手が来た → ゲーム開始"),
    onPeerLeft:  () => console.log("相手が抜けた"),
    onMessage:   (m) => { /* m.type で分岐して状態反映 */ },
    onClose:     () => console.log("切断（自動再接続中）"),
  });
  // conn.send({ type: "player", x: 400, y: 500 });
</script>
```

## 同期メッセージ設計（実装時の指針）
- **開始時**: host が `gameRand()` 用シードを決めて `{type:"start", seed}` を送る → 両者が同じ決定論シミュを回す。
- **攻撃側(host) → 防御側**:
  - `{type:"bullet", x, y, vx, vy, r, color, bt}` … `fireBullet()` のたびに送る（弾は線形移動なので受信側は追加するだけで再現）。
  - `{type:"boss", x, y, hp}` … 20〜30Hz でボス位置。
  - `{type:"pattern", p}` … パターン切替時のみ。
- **防御側(guest) → 攻撃側**:
  - `{type:"player", x, y, lives}` … 20〜30Hz。
  - `{type:"gameover"}` などイベント系は発生時のみ。
- 自分の手・自機の当たり判定は**ローカル即時反映**。相手の状態は受信して補間。混雑時は古い位置更新を捨てる。
- 2人友人用なら**当たり/残機は防御側ローカル確定**でOK（ホスト権限制を厳密にしなくても実用上問題なし）。

## 既存ゲームへの組み込みポイント（index.html の該当行）
- `fireBullet()` … line 1861 → 発生時に host が `bullet` メッセージを emit。
- `onHandResults()` … line 1667 → guest が自分の `player` 位置を emit / host が自分の手でボスを操作。
- `updateEnemy()`/ボスパターン … line 2230〜 → host のみ実行、guest は受信した弾を再生。
- `gameRand()`/`srng()` … line 1198 / 1191 → `start` の seed で両者を揃える。
- `state` … line 980 → 受信した相手側エンティティをここへ反映。
