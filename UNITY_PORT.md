# Unity 移植プラン (unityroom 投稿用)

現状の `index.html` (素のJS + MediaPipe Hands + Face Detection + Canvas)
を Unity に移植して WebGL ビルドし、unityroom に投稿するための作業計画。

実作業: ざっくり **3〜7日**（Unity 経験あり前提）

---

## 1. プロジェクト構成

- **Unity バージョン**: 2022 LTS 推奨 (WebGL ビルド成熟)
- **テンプレート**: 2D (URP は WebGL ビルドサイズが膨らむので不要)
- **解像度**: 320×240 内部 → 4:3 アスペクトで拡大
- **入力**: MediaPipe を WebGL 側で動かす

### MediaPipe を Unity WebGL でどう動かすか

3つの選択肢:

| 手法 | 難度 | 備考 |
|---|---|---|
| **MediaPipeUnityPlugin** (homuler/MediaPipeUnityPlugin) | ★★★ | C++ ネイティブ。WebGL ビルド対応は限定的 |
| **WebGL から JS の MediaPipe を呼ぶ** (.jslib) | ★★ | 既存 JS を流用、座標を Unity に渡す |
| **TensorFlow.js ハンドモデルを呼ぶ** | ★★ | TFJS Hand Pose を JS で動かす |

**推奨は中段の .jslib 連携**。既存の `Hands({...})` 初期化コードを
Plugins/WebGL/MediaPipeBridge.jslib に入れ、Unity 側 C# から
`[DllImport("__Internal")]` で呼び出し、検出結果（指先座標）を
`SendMessage("HandManager", "OnHandResult", json)` で受け取る。

### .jslib スケッチ (Plugins/WebGL/MediaPipeBridge.jslib)

```js
mergeInto(LibraryManager.library, {
  MP_Init: function() {
    if (window.__mpInited) return;
    window.__mpInited = true;
    const cam = document.createElement('video');
    cam.playsInline = true; cam.muted = true;
    document.body.appendChild(cam);
    const hands = new Hands({locateFile: f => 'https://cdn.jsdelivr.net/npm/@mediapipe/hands/'+f});
    hands.setOptions({maxNumHands:1, modelComplexity:1, selfieMode:true});
    hands.onResults(r => {
      if (!r.multiHandLandmarks?.length) {
        SendMessage('HandManager','OnHandLost','');
        return;
      }
      const lm = r.multiHandLandmarks[0];
      SendMessage('HandManager','OnHandResult', JSON.stringify(lm));
    });
    new Camera(cam, { onFrame: async () => await hands.send({image:cam}), width:640, height:480 }).start();
  }
});
```

C# 側:
```csharp
[DllImport("__Internal")] static extern void MP_Init();
void Start() { MP_Init(); }
public void OnHandResult(string json) { /* parse landmarks */ }
public void OnHandLost(string _) { /* ... */ }
```

---

## 2. 移植する要素

優先度順。

### Phase 1 — 最小プレイ可能版 (1〜2日)

- [ ] Unity 2D シーン構成、Canvas + Camera
- [ ] 320×240 仮想解像度 → カメラ Orthographic Size 設定
- [ ] 自機プレハブ + Sprite (ピクセルアート、Pixel Per Unit = 1)
- [ ] HandManager (.jslib bridge)
- [ ] 自機を fingertip 座標に追従
- [ ] 自動射撃 (Bullet プレハブ)
- [ ] Grunt 雑魚スポーン + 単発弾
- [ ] 衝突判定 (Collider2D + IsTrigger, OverlapCircle)
- [ ] スコア HUD (TextMeshPro)
- [ ] ステージ進行 (1ステージのみ)

### Phase 2 — ジェスチャ群 (1〜2日)

- [ ] Pinch → Bomb
- [ ] Fist → Guard (一定時間バリア)
- [ ] Peace → Focus (追尾弾切替)
- [ ] Swipe → Dash
- [ ] Thumb-bent → USE (Bullet Time)
- [ ] Thumbs-up → Dragon (チャージ＋発射)

C# で `FingerCurl(int tip, int pip)` 等のユーティリティを書き、
landmarks を毎フレーム解析。既存 JS の判定式をそのまま移植。

### Phase 3 — コンテンツ (1〜2日)

- [ ] Spinner / Fan 敵
- [ ] Boss 5 種スプライト + パターン (fan / spiral / ring / laser)
- [ ] Meteor (即死障害物)
- [ ] アイテム 6 種 (P/B/G/1/T/D) + ドロップ
- [ ] パララックス背景 5 テーマ
- [ ] HUD (Lives ハート / Bomb / Power / Guard / Graze / Dragon メーター)

### Phase 4 — メタ機能 (1日)

- [ ] Shop / Loadout (PlayerPrefs に保存、unityroom WebGL では LocalStorage が PlayerPrefs)
- [ ] Daily Challenge (シード固定)
- [ ] Rank / NO HIT 称号
- [ ] Pause / SFX (AudioSource)

### Phase 5 — unityroom 用 (半日)

- [ ] **解像度 960×720** (unityroom 標準は 960×540 か 1280×720。
      4:3 は黒帯になるので、`Player Settings → Resolution`)
- [ ] **WebGL Compression: Gzip** (Brotli は unityroom で動かない可能性)
- [ ] **Memory Size**: 256MB 程度に絞る
- [ ] **Strip Engine Code**: 有効
- [ ] テンプレート: `Better Minimal WebGL Template` (検索)
- [ ] **WebGL Player Settings → Publishing Settings → Disable "Decompression Fallback"**

---

## 3. unityroom 投稿

1. Unity → File → Build Settings → WebGL → Build
2. 出力フォルダを zip 化
3. https://unityroom.com/ にアカウント登録
4. 「ゲームを投稿」→ zip アップロード
5. メタデータ:
   - **タイトル**: HAND DANMAKU
   - **遊び方**: 手のジェスチャ操作の説明
   - **タグ**: `弾幕` `ハンドトラッキング` `MediaPipe` `カメラ` `アクション`
   - **画面サイズ**: 960×720
   - **画面の向き**: 横
6. 「カメラを使用する」旨を説明文に大きく書く
7. 公開

注意:
- unityroom は HTTPS で配信 → `getUserMedia` が動く
- iframe 内なので親ページの `feature-policy` で `camera` が必要
  (unityroom 側で対応してるはず)
- jsdelivr の MediaPipe は外部読み込みなので unityroom 規約で
  禁じられていないか念のため確認

---

## 4. 代替案

「unityroom 移植のコスト高すぎ」と感じたら:

- **A**: PLICY + itch.io だけで運用、unityroom はスキップ
- **B**: WebGL 出力に対応した他エンジンを使う
  - **Godot 4** (HTML5 export、軽量、JS との互換 OK)
  - **Bevy** (Rust + wasm、experimental)
- **C**: 開発者を雇う (ココナラ / Upwork で Unity + MediaPipe 経験者を探す。
  目安 5〜20 万円)

個人開発で時間優先なら **A** を強く推奨します。
バズ後に余力が出てから unityroom 版を検討で十分。

---

## Phase 2 以降の C# 移植ヒント

Phase 1 のスケルトンは `unity-starter/` に同梱（`unity-starter/README.md` 参照）。
そこから先で押さえるべきポイント:

### 弾・敵スポーン

- `Bullet.cs`: `Vector2 velocity` を持ち、Update で `transform.position += velocity * Time.deltaTime;`
  画面外（Camera のビューポート x/y < -0.1 or > 1.1）で `Destroy(gameObject)`。
- `EnemySpawner.cs`: ステージタイマを持ち、JS 版 `stageTick` と同じインターバルで Instantiate。
  `Object.Instantiate(prefab)` のコストはあるが、雑魚〜数十体ならフレーム落ちしない。

### 衝突判定

- 自機: 小さい `CircleCollider2D` (radius 0.05 程度) + `IsTrigger`
- 弾: 同じく `CircleCollider2D` + `IsTrigger`
- Physics2D の Layer Collision Matrix で「PlayerHitbox × EnemyBullet」だけ true にする
- `OnTriggerEnter2D` で被弾処理

### HUD

- TextMeshPro でテキスト、UI Image でメーター（Filled タイプ）
- `Canvas` を Screen Space Overlay にしてゲーム解像度に追従

### SFX

- Web Audio の代わりに `AudioSource.PlayClipAtPoint` または事前に AudioSource を1つ用意して PlayOneShot
- 効果音ファイルが必要なら sfxr / Audacity で作るか、`AudioClipBuilder` を書いて手続き的に生成

### LocalStorage / PlayerPrefs

- `localStorage` の代わりに **`PlayerPrefs.SetString` / `SetInt` / `SetFloat`**
- WebGL ビルドでは IndexedDB に保存される

### Daily シード RNG

- `System.Random(seed)` を使う
- seed は `string.GetHashCode()` で十分（移植中の小さな差は誤差）

### unityroom 固有の注意

- **WebGL Player Settings → Publishing → Decompression Fallback は OFF**
  （ON だと unityroom 側の解凍と干渉する報告あり）
- **Asset Bundle / Addressables は使わない**（unityroom の制約）
- 説明文に「カメラを使用します」と明記（unity1week でレギュレーション違反扱いされる事例回避）
- 起動時にカメラ許可ダイアログが出る前に「許可してください」案内を Canvas に表示すると親切

### バイナリサイズの目安

- Unity 2022 LTS + 2D で何も足さなければ WebGL ビルドは **5〜10MB** 程度
- スプライト/SFX を足しても 15MB 以下に収まる想定
- MediaPipe Hands モデル（4MB）は CDN 経由なのでバイナリには含まれない
