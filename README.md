# LANDOLT RUSH

マウスで棒の先端を操り、流れてくるランドルト環の切れ目を狙う2Dアクションゲームです。

## 遊び方

Unityでは `Assets/Scenes/GameScene.unity` を開いて Play を押します。元の `SampleScene` からのPlayもGameSceneへ移動します。

現段階ではUnityでのプレイ確認を優先し、Windows実行ファイルはビルドしていません。

Unityでのコンパイルと、先端の掃引判定・環の移動／回転・得点・コンボ・生成・ミスなど **2,025件の検証** は通過しています。実際の操作感と画面表示は、これからUnityのPlayで確認する段階です。記録は `Documentation/verification.txt` にあります。

| 操作 | 内容 |
| --- | --- |
| マウス移動 | 棒の先端を直接動かす |
| START RUN / Enter / Space | ゲーム開始。結果画面では再挑戦 |
| R | 最初からやり直す |
| Esc | 一時停止・再開 |
| M | 効果音のオン・オフ |
| BACK TO TITLE | タイトルへ戻る |

先端が切れ目に入ると成功、黒い部分に触れるとゲームオーバーです。棒の胴体には当たり判定がありません。環の中央の穴にいるだけでは得点になりません。

環を見送ってもゲームは続きますが、コンボが切れます。スコアは減りません。コンボの残り時間は次の環が出現してから2秒で、生成待ち時間には減りません。得点は `100 + (コンボ数 - 1) × 20` です。

## プロジェクト

- Unity **6000.0.51f1** / Universal Render Pipeline 2D
- VContainer **1.16.8**: `Packages/jp.hadashikick.vcontainer` に公式ソースを同梱
- R3 **1.3.0**: `Assets/Plugins/R3` に公式NuGetのDLLと依存ライブラリを同梱
- 外部画像・音源を必要としないメッシュ描画と生成効果音

参照チャットから抽出した確定仕様は `SPEC.md` に保存しています。

`Assets/LandoltRush/Scripts` は Data / Params / Components / Systems / Viewers / Info / Presentators / Scopes に分かれています。ゲーム進行はVContainerのEntryPointである `GameMainPresentator` が担当し、イベント通知にはR3を使います。購読は終了時と環の交換時に破棄します。

`Assets/LandoltRush/Settings` の4つのScriptableObjectでスコア・コンボ・棒・環の形状・生成条件を調整できます。環は `Assets/LandoltRush/Prefabs/LandoltRing.prefab` です。

## 判定について

表示と同じ頂点から作った環の領域に対し、前フレームから今フレームまでの先端の円を掃引します。環の移動も相対移動として扱い、回転は0.5度以下に分割して曲がり分を保守的に補います。1フレーム内に切れ目と黒い部分の両方を通った場合は、仕様どおりゲームオーバーを優先します。棒の胴体にはColliderを付けていません。

結果とタイトルは同一GameScene内のパネルで切り替えます。設定アセットはプレイ中に変更しません。保存・ランキング・難易度選択は今回の対象外です。

## 再生成・検証

Unityの `LANDOLT RUSH > Create or update game scene` でシーンと環Prefabを再生成できます。この操作は生成済みGameSceneとPrefabを上書きするため、それらを手作業で編集した場合は先に保存コピーを作成してください。既存の設定アセットの数値は維持します。

`LandoltRush.Editor.GameProjectBuilder.BuildEverything` はシーン生成、ゲームルール検証、Windows版ビルドを実行します。ビルドした実行ファイルに `--smoke-test` を付けると、実行版の開始・成功・コンボ・ミス・終了・再開を自動検証して終了し、`SmokeTest` フォルダーへ結果とスクリーンショットを保存します。

依存ライブラリの出典とライセンスは `THIRD_PARTY_NOTICES.md` を参照してください。
