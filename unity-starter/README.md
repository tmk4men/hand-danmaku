# Unity スターター — HAND DANMAKU 移植 Phase 1

「手の人差し指で四角が動く」最小 Unity プロジェクトのスケルトン。
unityroom 投稿に向けた第一歩。

## 0. 前提

- **Unity Hub** インストール済（https://unity.com/download）
- **Unity 2022.3 LTS** + **WebGL Build Support** モジュール
- 何かしらのテキストエディタ（VS Code / Rider / Visual Studio）

> Unity 6 でも動きますが LTS のほうが unityroom 配布実績多数。

## 1. 新規プロジェクト作成

1. Unity Hub → **New Project**
2. テンプレート: **2D (Built-In Render Pipeline)**
3. プロジェクト名: `HandDanmaku`
4. **Create project**

エディタが開いたら一旦そのまま置いて、次のステップへ。

## 2. ファイル配置

このディレクトリの中身を、新規プロジェクトの `Assets/` 配下に **そのままコピー**します。

```
unity-starter/Assets/        ↓ コピー
HandDanmaku/Assets/
  WebGLTemplates/MediaPipe/index.html
  Plugins/WebGL/MediaPipeBridge.jslib
  Scripts/HandManager.cs
  Scripts/PlayerShip.cs
  Scripts/GestureClassifier.cs
```

ファイラ（Windowsエクスプローラ/Finder）で `Assets` フォルダにドラッグ＆ドロップでOK。

Unity エディタ側に戻ると自動でインポートされる。

## 3. シーンセットアップ

1. **Hierarchy** で右クリック → Create Empty → 名前 `HandManager`
2. `HandManager` を選択 → Inspector → **Add Component** → `Hand Manager`
3. Hierarchy で右クリック → **2D Object → Sprites → Square** → 名前 `Player`
4. `Player` を選択 → Sprite Renderer → Color をお好み色（黄色推奨）
5. `Player` の Inspector → **Add Component** → `Player Ship`
6. **Main Camera** を選択 → Inspector → Orthographic Size = `4`
7. **File → Save Scene** → 名前 `Main`、保存先 `Assets/Scenes/`

## 4. WebGL ビルド設定

**Edit → Project Settings → Player → WebGL**

### Resolution and Presentation
- Default Canvas Width: `960`
- Default Canvas Height: `720`
- **WebGL Template**: **MediaPipe** を選択 (`Assets/WebGLTemplates/MediaPipe/` を自動認識)

### Other Settings
- Color Space: `Gamma`

### Publishing Settings
- Compression Format: **Gzip** （unityroom 推奨）
- ☐ Decompression Fallback: OFF
- ☑ Strip Engine Code: ON

### Player → General
- Active Input Handling: `Input Manager (Old)` または `Both`

## 5. プラットフォーム切替

**File → Build Settings**
1. Platform 一覧 → **WebGL** をクリック
2. 右下 **Switch Platform**（数分かかる）
3. Scenes In Build に `Scenes/Main` が含まれていることを確認（なければ Add Open Scenes）

## 6. ビルド

1. Build Settings → **Build** をクリック
2. 出力先フォルダを作成（例: `Builds/Web/`）
3. ビルド完了まで数分

完了したら出力フォルダの `index.html` を **HTTPS or localhost** 経由でブラウザに開く（直接 `file://` だとカメラAPIが拒否される）。

### ローカルで動作確認する簡単な方法

ビルド出力フォルダで:
```sh
python3 -m http.server 8080
```

ブラウザで `http://localhost:8080` → カメラ許可 → 人差し指で黄色い四角が動けば成功。

## 7. unityroom 投稿

1. 出力フォルダ（index.html を含む丸ごと）を **zip 化**
2. https://unityroom.com にアカウント作成 / ログイン
3. **ゲーム投稿** → zip アップロード
4. メタデータ:
   - タイトル: `HAND DANMAKU`
   - タグ: `弾幕`, `ハンドトラッキング`, `MediaPipe`, `カメラ`
   - 画面サイズ: 960×720
   - 遊び方: 「カメラに手を映し、人差し指で機体を動かして弾幕を避けるシューティング」
5. 公開

## 8. 次の Phase

このスターターは Phase 1（手で機体が動く）のみ。

- Phase 2: ジェスチャ群（GestureClassifier.cs に Bomb/Guard/Focus 等の判定済み）
- Phase 3: 敵スポーン・弾幕・衝突判定
- Phase 4: ボス・隕石・アイテム
- Phase 5: HUD・スコア・コイン・ショップ
- Phase 6: unityroom 用最終調整

リポジトリ ルートの `UNITY_PORT.md` に Phase 2 以降の詳細あり。

---

## トラブルシュート

| 症状 | 対処 |
|---|---|
| `Hands is not defined` がコンソールに | WebGL Template が **MediaPipe** になっていない。Player Settings で再選択 |
| カメラ許可ダイアログが出ない | `file://` で開いている。`http://localhost:xxx` または `https://` で実行 |
| 動作が重い・FPS落ちる | `MediaPipeBridge.jslib` 内の `modelComplexity: 1` を `0` に下げる（精度低下と引き換えに高速化） |
| Player が動かない | コンソールでエラー確認。`HandManager` GameObject が存在するか、名前が完全一致か（`SendMessage` は名前で送る） |
| ビルドが失敗（Brotli/Gzip error） | Publishing Settings の Compression Format を Gzip に固定、Decompression Fallback OFF |
| iframe 内で動かない | unityroom 側で camera permission が必要。ゲーム概要に「カメラ使用」と明記 |
| エディタ実行（▶︎）では動かない | これは仕様。WebGL ビルドでのみ MediaPipe が動く（`#if UNITY_WEBGL && !UNITY_EDITOR` で囲んでいるため） |
