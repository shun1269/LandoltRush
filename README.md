# LANDOLT RUSH

マウスで棒の先端を操り、流れてくるランドルト環の切れ目を狙う2Dアクションゲームです。

## 遊び方

Unityでは `Assets/Scenes/GameScene.unity` を開いて Play を押します。元の `SampleScene` からのPlayもGameSceneへ移動します。

現段階ではUnityでのプレイ確認を優先し、Windows実行ファイルはビルドしていません。

Unityでのコンパイルとゲームルールの検証記録は `Documentation/verification.txt` にあります。実際の操作感はUnityのPlayで確認してください。

| 操作 | 内容 |
| --- | --- |
| マウス移動 | 棒の先端を直接動かす |
| START RUN / Enter / Space | ゲーム開始。結果画面では再挑戦 |
| R | 最初からやり直す |
| Esc | 一時停止・再開 |
| M | 効果音のオン・オフ |
| BACK TO TITLE | タイトルへ戻る |

先端の中心が切れ目を通って環の内側半径より内側に入ると成功です。切れ目の外側に触れただけでは得点になりません。先端が黒い部分に触れるとゲームオーバー、棒の胴体が黒い部分に触れるとその環がMissになります。Missではゲームは続き、スコアは減らず、コンボが切れます。

コンボ継続時間は **6秒**。成功時に回復し、環が1つも存在しない待機時間には減りません。別の環が残っている場合はそのままカウントダウンします。得点は `100 + (コンボ数 - 1) × 20` です。

前の環が残っていても次の環が出現します。出現間隔は **4秒 → 90秒後に0.9秒** へ徐々に短くなります。同時出現は最大12個で、上限に達した場合は空きができるまで生成を待ちます。環の移動・回転速度の範囲は従来どおりです。一時停止中は加速せず、リトライで初期ペースに戻ります。各数値は `GameConfig` で変更できます。

## プロジェクト

- Unity **6000.0.51f1** / Universal Render Pipeline 2D
- VContainer **1.16.8**: `Packages/jp.hadashikick.vcontainer` に公式ソースを同梱
- R3 **1.3.0**: `Assets/Plugins/R3` に公式NuGetのDLLと依存ライブラリを同梱
- 外部画像・音源を必要としないメッシュ描画と生成効果音

参照チャットから抽出した確定仕様は `SPEC.md` に保存しています。

`Assets/LandoltRush/Scripts` は Data / Params / Components / Systems / Viewers / Info / Presentators / Scopes に分かれています。ゲーム進行はVContainerのEntryPointである `GameMainPresentator` が担当し、イベント通知にはR3を使います。購読は終了時と環の交換時に破棄します。

`Assets/LandoltRush/Settings` の4つのScriptableObjectでスコア・コンボ・棒・環の形状・生成条件を調整できます。環は `Assets/LandoltRush/Prefabs/LandoltRing.prefab` です。

## 判定について

表示と同じ頂点から作った環の領域に対し、先端の円と棒の胴体が前フレームから今フレームまでに通った領域を調べます。環の移動も相対移動として扱い、回転は0.5度以下に分割して曲がり分を保守的に補います。全ての環を確認してから結果を処理し、同一フレームでは **先端接触によるゲームオーバー → 胴体接触によるMiss → 内側到達による成功** の順で優先します。

結果とタイトルは同一GameScene内のパネルで切り替えます。設定アセットはプレイ中に変更しません。保存・ランキング・難易度選択は今回の対象外です。

## 再生成・検証

Unityの `LANDOLT RUSH > Create or update game scene` でシーンと環Prefabを再生成できます。この操作は生成済みGameSceneとPrefabを上書きするため、それらを手作業で編集した場合は先に保存コピーを作成してください。既存の設定アセットの数値は維持します。

`LandoltRush.Editor.GameProjectBuilder.BuildEverything` はシーン生成、ゲームルール検証、Windows版ビルドを実行します。ビルドした実行ファイルに `--smoke-test` を付けると、実行版の開始・成功・コンボ・ミス・終了・再開を自動検証して終了し、`SmokeTest` フォルダーへ結果とスクリーンショットを保存します。

依存ライブラリの出典とライセンスは `THIRD_PARTY_NOTICES.md` を参照してください。
