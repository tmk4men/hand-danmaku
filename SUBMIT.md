# 配布ガイド (HAND DANMAKU)

`dist/hand-danmaku.zip` を各サイトにアップロードしてください。
`scripts/make_dist.py` で再生成できます (`python3 scripts/make_dist.py`)。

zip 中身: `index.html`, `og.png`。MediaPipe / オーディオ / フォントは
CDN を参照するので追加アセット不要。

---

## 1. PLICY (plicy.net)

最も近い体験。日本のインディゲーム向けプラットフォーム。

1. https://plicy.net/ にアカウント登録（無料）
2. マイページ → 「ゲーム登録」→ **HTML5** を選択
3. `dist/hand-danmaku.zip` をアップロード
4. メタデータ:
   - **タイトル**: HAND DANMAKU
   - **作者**: お好み
   - **カテゴリ**: シューティング / アクション
   - **タグ**: `弾幕` `ハンドトラッキング` `MediaPipe` `カメラ` `ブラウザ`
   - **説明文**: 「手のジェスチャだけで操作する弾幕シューティング。
     人差し指で移動、ピンチでボム、ピース✌で追尾射撃、グーでガード。
     カメラ許可が必要です。」
   - **対応デバイス**: PC（カメラ必須）
   - **スクリーンショット**: 5枚撮るとよい（録画ツールでスナップ）
5. **画面サイズ**: 自由（うちのは4:3レスポンシブ）
6. **公開** → URL が発行されます

注意:
- PLICY は iframe 埋め込み。カメラ許可は iframe 内でも通ります。
- 起動時に "カメラを使用しますか?" のブラウザダイアログが出ます。

---

## 2. itch.io

国際向け、英語タグが効きます。

1. https://itch.io/ にアカウント登録（無料）
2. ダッシュボード → "Create new project"
3. 設定:
   - **Title**: HAND DANMAKU
   - **Project URL**: hand-danmaku
   - **Short description**: "Hand-tracking bullet hell — pinch for bombs, peace sign for homing shots, fist to guard."
   - **Classification**: Game
   - **Kind of project**: **HTML** (これ重要)
   - **Pricing**: $0 (Free, "No payments")
4. **Uploads**:
   - `dist/hand-danmaku.zip` をアップロード
   - チェック: **"This file will be played in the browser"**
   - **Viewport dimensions**: `1024 × 768` または `100% × 100%` (Embed in page にチェック)
   - "Fullscreen button" を ON
5. **Details**:
   - **Genre**: Shooter / Action
   - **Tags**: `bullet-hell`, `hand-tracking`, `mediapipe`, `pixel-art`,
     `webgl-no`, `webcam`, `arcade`, `browser`
   - **Cover image**: `og.png` (1200×630 が表紙に)
   - **Screenshots**: 数枚
   - **Made with**: `JavaScript` `MediaPipe`
6. **Visibility**: Public 公開、または最初は Restricted で確認

注意:
- itch.io はゲームが HTTPS でホストされるのでカメラ OK
- jsdelivr の MediaPipe CDN を読みに行く。CSP は問題なし

---

## 3. unityroom

**Unity 専用**サイト。現状の JS 版はそのまま投稿できません。

選択肢:
- A) **見送り**（PLICY と itch.io だけで運用）
- B) **Unity に移植**（→ `UNITY_PORT.md` の手順）

unityroom は unity1week ジャムで露出が大きい一方、Unity 知識と
WebGL ビルド環境が必要。MediaPipe を Unity に持ってくる場合は
`MediaPipeUnityPlugin` (https://github.com/homuler/MediaPipeUnityPlugin) が
最有力で、Unity 2021+ で WebGL ビルド可能。

詳細は `UNITY_PORT.md` を参照。

---

## SNS 告知テンプレ

公開URL ができたら:

```
HAND DANMAKU 公開しました 🎮
手のジェスチャだけで操作するブラウザ弾幕。
ピンチでボム / ピース✌で追尾射撃 / グーでシールド

▶ PLICY: <URL>
▶ itch.io: <URL>
▶ GitHub: https://github.com/tmk4men/hand-danmaku

#手で弾幕 #handdanmaku #mediapipe #indiegame
```
