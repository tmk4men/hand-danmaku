# Unity スターター — HAND DANMAKU 移植（unityroom 投稿用）

**このフォルダ自体がそのまま Unity プロジェクトです。** 新規プロジェクトを
作って `Assets/` をコピーする昔の手順はもう不要。Unity Hub でこのフォルダを
開く → ビルドコマンド1発 → zip を unityroom にアップ、で完了します。

GameBootstrap / SpriteFactory / ProceduralSFX により、シーン配置・画像インポート・
音声ファイルはすべてコード生成。手作業の GUI 操作はほぼゼロです。

実装済みの遊び:
- 人差し指で機体追従、自動射撃、敵スポーン（追尾/スプレッド/回転弾）
- 隕石（即死、GUARD/DASH で回避）、ボス（14ウェーブ毎・4パターン）
- ジェスチャ: ピンチ→ボム / 握り→ガード / ピース→フォーカス / 指振り→ダッシュ
  / 親指曲げ→バレットタイム / 親指立て10秒→ドラゴンビーム
- HUD（SCORE/HI/LIVES）、コンボ、ポップアップ、ステージバナー
- アイテム6種・コイン・ショップ、PlayerPrefs 永続化、JA/EN 切替、デイリー

---

## 0. 前提（人間がやる唯一の準備）

1. **Unity Hub** をインストール — https://unity.com/download
2. **Unity 2022.3 LTS** + **WebGL Build Support** モジュールを追加
   - `ProjectSettings/ProjectVersion.txt` に `2022.3.62f1` を指定済み。
     別の 2022.3 LTS を入れている場合はこの1行を手持ちのバージョンに書き換えるだけでOK。

> Claude（自分）は Unity エディタを起動できないため、ここから先のビルドは
> 人間が下記コマンドを1回叩く形になります。設定・シーン・ビルドはすべて
> `Assets/Editor/BuildScript.cs` が自動でやります。

## 1. ビルド（コマンド1発）

### Windows
このフォルダの **`build_webgl.bat` をダブルクリック**。以上。
（Unity の場所が自動検出できない場合は exe パスを引数で渡す）
```bat
build_webgl.bat "C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe"
```

### macOS / Linux
```sh
./build_webgl.sh
```

### 手で叩く場合（中身はこれだけ）
```sh
"<Unity.exe>" -quit -batchmode -projectPath "<このフォルダ>" \
  -buildTarget WebGL -executeMethod BuildScript.BuildWebGL -logFile -
```

`BuildScript.BuildWebGL` が自動で:
- `Assets/Scenes/Main.unity` を生成（空オブジェクト + GameBootstrap）
- WebGL Template = **MediaPipe** / Compression = **Gzip** /
  Decompression Fallback = **OFF** / Color Space = **Gamma** / 960×720 / コードストリップ
- プラットフォームを WebGL に切替えて `Builds/Web/` に出力

### GUI で操作したい人向け
Unity Hub → **Open** → このフォルダを選択 → メニュー **Build ▸ WebGL (unityroom)**。
同じ処理が走ります。

## 2. 動作確認（ローカル）

`file://` 直開きはカメラAPIが拒否されるので HTTP で:
```sh
cd Builds/Web
python3 -m http.server 8080
```
ブラウザで `http://localhost:8080` → カメラ許可 → 人差し指で機体が動けばOK。

> エディタの ▶ 再生では MediaPipe は動きません（`UNITY_WEBGL && !UNITY_EDITOR`
> でガードしているため）。確認は必ず WebGL ビルドで。

## 3. unityroom 投稿

1. `Builds/Web/` の**中身**を zip 化（index.html がルートに来るように）
2. https://unityroom.com にログイン → **ゲームを投稿する**
3. zip をアップロード
4. メタデータ:
   - タイトル: `HAND DANMAKU`
   - 画面サイズ: **960 × 720**
   - タグ: `弾幕` `ハンドトラッキング` `MediaPipe` `カメラ` `シューティング`
   - 遊び方: 「カメラに手を映し、人差し指で機体を操作。ピンチでボム、握りでガード。
     弾幕と隕石を避けてハイスコアを狙え」
   - **カメラを使用する旨を概要に明記**（iframe 内で許可ダイアログを出すため）
5. サムネはリポジトリ ルートの `unityroom_banner.png`（960×540）
6. 公開

---

## トラブルシュート

| 症状 | 対処 |
|---|---|
| Hub で「エディタが見つからない」 | `ProjectVersion.txt` を手持ちの 2022.3 LTS に書き換える |
| `build_webgl.bat` が Unity を見つけられない | exe のフルパスを第1引数で渡す |
| `Hands is not defined` | WebGL Template が MediaPipe でない。BuildScript 経由でビルドし直す |
| カメラ許可が出ない | `file://` で開いている。`http://localhost` か `https://` で |
| 重い / FPS 低下 | `Assets/Plugins/WebGL/MediaPipeBridge.jslib` の `modelComplexity:1` を `0` に |
| Player が動かない | `HandManager` という名前の GameObject が必要（`SendMessage` は名前で送る） |
| Brotli/Gzip エラー | Compression=Gzip / Decompression Fallback=OFF（BuildScript で設定済み） |
| ▶ 再生で動かない | 仕様。WebGL ビルドでのみ MediaPipe が動く |

## ファイル構成

```
unity-starter/                ← Unity Hub でこのフォルダを開く
  build_webgl.bat / .sh       ← ビルド launcher
  Packages/manifest.json      ← 依存パッケージ（2D Built-In 相当）
  ProjectSettings/ProjectVersion.txt
  Assets/
    Editor/BuildScript.cs     ← シーン生成 + 全設定 + WebGL ビルドを自動化
    Scripts/                  ← ゲーム本体 22 ファイル（GameBootstrap が全部組む）
    Plugins/WebGL/MediaPipeBridge.jslib   ← MediaPipe Hands → Unity ブリッジ
    WebGLTemplates/MediaPipe/index.html   ← MediaPipe をロードする WebGL テンプレート
```
