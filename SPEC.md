# LANDOLT RUSH

## 現行ルール（プレイテスト後の改訂）

以下の改訂を、この後に保存した初期仕様より優先する。

- 先端が黒い部分に接触した場合は引き続きGameOver。
- 棒の胴体が黒い部分に接触した場合は、その環をMissとして削除。コンボをリセットし、MissCountを加算する。ゲームとスコアは維持する。
- 成功条件は、先端の中心が切れ目を通り、外から内側半径より内側へ到達すること。切れ目の外周に触れただけでは成功しない。
- 同一フレームの優先順位は、全ての環についてGameOver > 胴体Miss > 成功。
- コンボ継続時間は6秒。環が1つも存在しない間のみ停止する。
- 同時に複数の環を出現可能にする。前の環の成功・Missを待たず、独立したタイマーで生成する。
- 初回生成は0.6秒後。以降の間隔はプレイ時間0秒で4秒、90秒で0.9秒になるよう線形に短縮し、その後は0.9秒を維持する。
- 同時出現上限は12個。満杯では空きができるまで待ち、生成の積み残しをまとめて出さない。
- 加速対象は生成間隔。各環の移動・回転速度の範囲は変更しない。
- 一時停止中は経過時間と生成タイマーを止め、リトライで全ての環・購読・加速状態をリセットする。
- 各調整値はGameConfig、胴体の半径はRodParamに保持する。

## 初期仕様（参照チャットからの記録）

以下は当初の設計記録。上の現行ルールと異なる箇所は改訂済み。

## Codexへの指示

Unityで2Dゲーム **LANDOLT RUSH** を実装する。

このドキュメントをゲーム仕様およびクラス設計の基準とする。

実装にあたっては、以下を守ること。

- Unityを使用する
- VContainerを使用して依存関係を管理する
- R3を使用してイベント通知・UI入力通知を行う
- ScriptableObjectをパラメータ管理に使用する
- 巨大なGameManagerを作らない
- クラスを責務ごとに分割する
- Viewerは表示だけを担当する
- InfoはUI入力通知だけを担当する
- ComponentはGameObject単体の挙動だけを担当する
- SystemはPure C#としてゲームルールを担当する
- Presentatorが全体の進行を管理する
- Scene上のMonoBehaviourとPure C#クラスの依存関係はVContainerで管理する
- R3の購読は原則Presentatorで行い、必ずDisposeする
- `FindObjectOfType` などによる依存取得は原則使用しない

実装を始める前に、既存のUnityプロジェクト構造を確認し、それに合わせてファイルを配置すること。

---

# 1. ゲーム概要

LANDOLT RUSHは、画面外右下から伸びる長い棒の先端をマウスで操作し、画面外から流れてくるランドルト環の切れ目を連続で狙う2Dアクションゲームである。

プレイヤーが直接操作するのは棒の先端である。

ランドルト環は、

- 画面上側
- 画面左側

のいずれかの画面外から生成される。

生成されたランドルト環は、

- ランダムな大きさ
- ランダムな移動速度
- ランダムな入射方向
- ランダムな初期角度
- ランダムな回転速度
- ランダムな回転方向

を持ち、回転しながら画面内へ移動する。

プレイヤーは移動・回転するランドルト環の切れ目へ棒の先端を入れる。

---

# 2. ゲームの核となる仕様

以下は特に重要な仕様であり、変更しないこと。

## 棒

- 棒の支点は画面外右下に固定する
- 棒の先端はマウスに追従する
- 棒は支点から先端まで一直線に表示する
- 当たり判定を持つのは棒の先端だけである
- 棒の胴体には当たり判定を持たせない

したがって、

> 棒の胴体がランドルト環の黒い部分を通過しても問題ない。

## 成功条件

棒の**先端**がランドルト環の切れ目に入った場合、成功とする。

## GameOver条件

棒の**先端**がランドルト環の黒い部分に触れた場合、GameOverとする。

棒の胴体が黒い部分に重なってもGameOverにはならない。

---

# 3. 操作

## マウス操作

マウスカーソルの位置へ棒の先端を追従させる。

MVPでは慣性や追従遅延を設けず、基本的に直接追従とする。

概念的には以下の状態になる。

```text
画面外右下のPivot
        \
         \
          \
           \
            Tip ← マウス位置
```

PivotとTipを結ぶ線が棒本体になる。

---

# 4. ランドルト環

## 4.1 構造

ランドルト環は判定上、以下の2種類の領域を持つ。

### BlackPart

ランドルト環の黒い部分。

棒の先端が接触するとGameOver。

### GapArea

ランドルト環の切れ目部分。

棒の先端が入ると成功。

---

# 5. ランドルト環の生成

## 5.1 同時出現数

MVPではランドルト環は**1個ずつ出現**する。

現在のランドルト環について、

- 成功する
- Missする

のいずれかが発生した後、次のランドルト環を生成する。

GameOver時には新しいランドルト環を生成しない。

---

# 6. 生成位置

ランドルト環は画面内ではなく、画面外から生成する。

生成側は以下のどちらか。

```text
Top
Left
```

初期仕様では、それぞれ50%程度の確率とする。

確率はParamから変更可能にする。

---

## 6.1 Top

画面上端より上にランドルト環を生成する。

X位置は画面横幅の範囲内からランダムに決定する。

ただし端ぎりぎりは除外する。

```text
           Spawn
             ○
             ↓
┌──────────────────┐
│                  │
│      Game        │
│      Screen      │
│                  │
└──────────────────┘
```

---

## 6.2 Left

画面左端より左にランドルト環を生成する。

Y位置は画面縦幅の範囲内からランダムに決定する。

上下端ぎりぎりは除外する。

```text
       ┌──────────────────┐
Spawn ○→                  │
       │      Game        │
       │      Screen      │
       │                  │
       └──────────────────┘
```

---

# 7. 入射方向

単純に角度だけをランダム化して、画面へ入らないランドルト環が発生しないようにする。

以下の方法を使用する。

```text
画面外のSpawnPositionを決める
↓
画面内にTargetPositionをランダム生成する
↓
SpawnPosition → TargetPosition の方向を求める
↓
その方向へランドルト環を移動させる
```

TargetPositionは画面端ではなく、画面中央寄りの領域から選択する。

これにより、生成されたランドルト環が確実に画面内を横切るようにする。

---

# 8. ランダムパラメータ

ランドルト環ごとに以下をランダム化する。

- SpawnSide
- SpawnPosition
- TargetPosition
- MoveDirection
- MoveSpeed
- Scale
- InitialAngle
- RotateSpeed
- RotateDirection

初期調整値の目安：

| 項目 | 範囲 |
|---|---|
| SpawnSide | Top / Left |
| MoveSpeed | 1.5〜3.0 |
| Scale | 0.8〜1.3 |
| InitialAngle | 0〜360° |
| RotateSpeed | 30〜180°/s |
| RotateDirection | CW / CCW |

数値はScriptableObjectから調整可能にする。

---

# 9. ランドルト環の移動

MVPでは等速直線運動とする。

```text
SpawnPosition
      ↓
      ○
       \
        \
         ○
          \
           \
            ○
```

以下はMVPでは実装しない。

- 加速
- 減速
- 曲線運動
- 追尾
- 途中での方向転換

---

# 10. ランドルト環の回転

ランドルト環は移動しながら自身の中心を軸に回転する。

各個体について、

- 回転速度
- 回転方向

をランダムに決定する。

回転速度がほぼ0になることは避ける。

---

# 11. 成功

棒の先端がGapAreaへ入った場合、成功とする。

成功時：

1. ランドルト環の判定を無効化する
2. コンボを1増やす
3. スコアを加算する
4. SuccessCountを増やす
5. UIを更新する
6. 成功したランドルト環を消す
7. 少し待って次のランドルト環を生成する

同じランドルト環に対して成功処理が複数回発生しないようにする。

---

# 12. GameOver

棒の先端がBlackPartに触れた場合、GameOverとする。

以下の場合もGameOver。

- プレイヤーが先端を黒部分へ動かした
- ランドルト環が移動して先端へ当たった
- ランドルト環が回転して先端へ当たった

GameOver時：

1. GamePhaseをFinishedにする
2. ResultTypeをGameOverにする
3. 棒の操作を停止する
4. 現在のランドルト環を停止する
5. 当たり判定を無効化する
6. GameOver演出を行う
7. ResultPanelを表示する

GameOver処理は1回だけ実行する。

---

# 13. Miss

ランドルト環が一度画面内に入った後、成功せずに画面外へ完全に出た場合はMissとする。

MissはGameOverにはしない。

Miss時：

1. コンボを0に戻す
2. MissCountを増やす
3. ランドルト環を削除する
4. 少し待つ
5. 次のランドルト環を生成する

スコアは減らさない。

---

# 14. 画面内外判定

ランドルト環は画面外から生成されるため、生成直後をMissとして扱ってはいけない。

ランドルト環は概念上以下の状態を持つ。

```text
BeforeEntry
↓
Entered
↓
Exited
```

一度でも画面内に入った後、環全体が画面外へ出た場合のみMissとする。

---

# 15. コンボ

成功するたびにComboCountを1増やす。

```text
成功
↓
ComboCount + 1
↓
ComboRemainingTimeを最大まで回復
```

成功から一定時間以内に次の成功を出せばコンボ継続。

時間切れで0へ戻る。

Missした場合も0へ戻る。

---

## 15.1 コンボ継続時間

初期値：

```text
2.0秒
```

ただしプレイテストで調整する。

---

## 15.2 コンボタイマー開始タイミング

前のランドルト環を成功した直後からではなく、

> 次のランドルト環が生成された時点

から減少を開始する。

生成待機時間によってプレイヤーが不利にならないようにする。

---

# 16. スコア

成功するたびにスコアを獲得する。

初期仕様：

```text
獲得スコア
=
BaseScore
+
(ComboCount - 1) × ComboBonusScore
```

初期値：

```text
BaseScore = 100
ComboBonusScore = 20
```

例：

| Combo | Score |
|---:|---:|
| 1 | 100 |
| 2 | 120 |
| 5 | 180 |
| 10 | 280 |

Missしてもスコアは減らさない。

GameOverしてもスコアは減らさない。

---

# 17. 判定優先順位

同じフレームで、

- GapAreaへの侵入
- BlackPartへの接触

の両方が発生した場合、

```text
GameOver
>
Success
```

とする。

ただしPrefab上ではGapAreaとBlackPartのColliderが重ならないように作ること。

---

# 18. 高速マウス移動への対応

Tipはマウスへ直接追従するため、高速にマウスを移動すると1フレームでColliderを飛び越える可能性がある。

単純なOverlap判定だけに依存しないこと。

前フレームのTip位置から現在位置までの軌跡についても衝突を確認できる構成にする。

具体的な実装方法は既存プロジェクトとUnityバージョンを確認して決定する。

---

# 19. UI

プレイ中：

- Score
- Combo
- ComboGauge

GameOver時：

- GAME OVER
- Final Score
- Max Combo
- Success Count
- RestartButton
- TitleButton

---

# 20. ResultPanel

MVPではResultSceneを作らず、

```text
GameScene
├── GameUI
└── ResultPanel
```

という構成にする。

GameOver時にResultPanelを表示する。

---

# 21. GameData

GameDataはゲーム中に変化する状態だけを保持する。

保持候補：

```text
GamePhase phase
ResultType resultType

int score

int comboCount
float comboRemainingTime
int maxCombo

int successCount
int missCount
```

GameData自身にはゲームルールを書かない。

---

# 22. Param系

すべてScriptableObjectとして扱う。

## GameConfig

ゲーム全体の調整値。

保持候補：

```text
baseScore
comboBonusScore
comboLimitTime

firstSpawnDelay
successSpawnDelay
missSpawnDelay
```

---

## RodParam

棒に関する固定設定。

保持候補：

```text
rightPivotMargin
bottomPivotMargin
tipRadius
```

---

## RingParam

ランドルト環自体の固定情報。

保持候補：

```text
prefab
ringType
visual
```

ランダムで変化する速度・サイズ・回転速度などはRingParamに保存しない。

---

## SpawnParam

生成に関する調整値。

保持候補：

```text
topSpawnWeight
leftSpawnWeight

spawnMargin
edgeExclusionMargin
targetAreaRate

minMoveSpeed
maxMoveSpeed

minScale
maxScale

minRotateSpeed
maxRotateSpeed
```

---

# 23. Runtime Data

## RingSpawnData

ランドルト環1個を生成するときの実行時データ。

保持候補：

```text
RingParam ringParam
SpawnSide spawnSide

Vector2 spawnPosition
Vector2 targetPosition

Vector2 moveDirection
float moveSpeed

float scale

float initialAngle
float rotateSpeed
```

ScriptableObjectにはしない。

---

# 24. クラス設計方針

以下の分類を厳守する。

```text
Data
→ 状態を保持する

Param
→ 固定設定を保持する

Viewer
→ 表示だけを行う

Info
→ UI入力を通知する

Component
→ GameObject単体を動かす

System
→ ゲームルールを処理する

Presentator
→ 各クラスをつないでゲーム進行を管理する

Scope
→ VContainerへ依存関係を登録する
```

---

# 25. Component系

## RodController: MonoBehaviour

**概要**

棒の先端をマウス位置へ移動させる。

**主な責務**

- マウス位置取得
- Tip移動
- 操作の有効/無効
- RodViewへの表示更新要求

ゲームルールは持たない。

---

## RodView: MonoBehaviour

**概要**

PivotとTipを結ぶ棒を表示する。

**主な責務**

- 棒の描画
- 棒の表示/非表示

当たり判定や入力処理は行わない。

---

## RodTipCollision: MonoBehaviour

**概要**

棒の先端に関する衝突を検知する。

**通知**

```text
OnBlackTouched
OnGapEntered
```

ゲーム状態を直接変更しない。

---

## LandoltRingComponent: MonoBehaviour

**概要**

ランドルト環1個のGameObjectとしての挙動を管理する。

**主な責務**

- 移動
- 回転
- 開始/停止
- 画面への侵入確認
- 画面外への退出確認
- Miss通知

ゲーム全体の進行管理はしない。

---

## LandoltRingSpawner: MonoBehaviour

**概要**

ランドルト環Prefabを実際に生成・削除する。

生成ルール自体は持たない。

---

## LandoltBlackPart: MonoBehaviour

**概要**

ランドルト環の黒部分であることを表す。

---

## LandoltGapArea: MonoBehaviour

**概要**

ランドルト環の切れ目であることを表す。

所属するLandoltRingComponentへの参照を持つ。

---

# 26. System系

すべてPure C#とする。

## RingSpawnSystem

**概要**

ランドルト環の生成条件を決め、RingSpawnDataを作成する。

**担当**

- SpawnSide決定
- SpawnPosition決定
- TargetPosition決定
- MoveDirection決定
- MoveSpeed決定
- Scale決定
- InitialAngle決定
- RotateSpeed決定

PrefabのInstantiateはしない。

---

## ScoreSystem

**概要**

GameDataのスコアをゲームルールに従って更新する。

---

## ComboSystem

**概要**

以下を管理する。

- ComboCount
- ComboRemainingTime
- MaxCombo
- コンボリセット

---

## GameJudgeSystem

**概要**

GamePhaseやゲーム終了判定を管理する。

UIやScene遷移は行わない。

---

# 27. Viewer系

## GameUIViewer: MonoBehaviour

**概要**

以下を表示する。

- Score
- Combo
- ComboGauge

表示だけを担当する。

---

## ResultPanelViewer: MonoBehaviour

**概要**

以下を表示する。

- GAME OVER
- Final Score
- Max Combo
- Success Count

表示だけを担当する。

---

# 28. Info系

## ResultPanelInfo: MonoBehaviour

**概要**

以下のUI入力をR3 Observableとして公開する。

```text
OnRestartClicked
OnTitleClicked
```

GameDataの変更やScene遷移は行わない。

---

# 29. Presentator系

## GameMainPresentator

MonoBehaviourにしない。

VContainerのEntryPointとして登録する。

必要に応じて以下を実装する。

```text
IStartable
ITickable
IDisposable
```

**概要**

ゲーム全体の進行を管理する。

**担当**

- ゲーム初期化
- R3イベント購読
- RodControllerの開始/停止
- RingSpawnSystemの呼び出し
- LandoltRingSpawnerへの生成要求
- 成功処理
- Miss処理
- GameOver処理
- ScoreSystem呼び出し
- ComboSystem呼び出し
- GameJudgeSystem呼び出し
- Viewer更新
- Restart処理
- TitleSceneへの遷移

---

# 30. Scope系

## GameLifetimeScope

GameSceneの依存関係を登録する。

登録対象：

```text
GameData

GameConfig
RodParam
RingParam
SpawnParam

RodController
RodTipCollision
LandoltRingSpawner

GameUIViewer
ResultPanelViewer
ResultPanelInfo

RingSpawnSystem
ScoreSystem
ComboSystem
GameJudgeSystem

GameMainPresentator
```

---

# 31. R3方針

ComponentやInfoは必要なイベントをObservableとして公開する。

```text
Component / Info
↓
Observable
↓
Presentator
↓
System / Viewer / Component
```

Subjectはprivateにする。

R3の購読は原則GameMainPresentatorに集約する。

購読は必ずDisposeする。

---

# 32. VContainer方針

MonoBehaviour：

```text
GameData
Viewer
Info
Component
LifetimeScope
```

Pure C#：

```text
System
Presentator
```

Scene上に存在するMonoBehaviourはRegisterComponentする。

SystemはRegisterする。

PresentatorはRegisterEntryPointする。

ParamはRegisterInstanceする。

---

# 33. 禁止する設計

原則として以下を行わない。

```text
巨大なGameManagerを作る

FindObjectOfTypeで依存を探す

ViewerからGameDataを書き換える

ViewerがInfoをSubscribeする

ViewerがScene遷移する

InfoからGameDataを書き換える

Componentがゲーム全体を進行する

SystemをMonoBehaviourにする

SystemからUIを更新する

SystemからScene遷移する

ScriptableObjectのParamをゲーム中に変更する
```

---

# 34. ゲームフロー

```text
Game Start
↓
GameData初期化
↓
Rod操作開始
↓
RingSpawnSystemがRingSpawnData生成
↓
LandoltRingSpawnerが環を生成
↓
環が画面外から入射
↓
プレイヤーがTipを操作
│
├─ GapArea
│   ↓
│   Success
│   ↓
│   Combo +
│   Score +
│   ↓
│   次の環
│
├─ 画面外へ通過
│   ↓
│   Miss
│   ↓
│   Combo Reset
│   ↓
│   次の環
│
└─ BlackPart
    ↓
    GameOver
    ↓
    操作停止
    ↓
    ResultPanel
```

---

# 35. MVP完成条件

以下がすべて動作すればMVP完成とする。

- Pivotが画面外右下に存在する
- Tipがマウスに追従する
- PivotとTipの間に棒が表示される
- 棒の胴体には判定がない
- Tipだけに判定がある
- ランドルト環が上または左の画面外から生成される
- 環が画面内へ向かって移動する
- 環のサイズがランダム
- 環の速度がランダム
- 環の初期角度がランダム
- 環の回転速度がランダム
- 環の回転方向がランダム
- TipがGapAreaに入ると成功する
- TipがBlackPartに触れるとGameOverになる
- 棒の胴体がBlackPartを通過しても問題ない
- 成功するとスコアが増える
- 成功するとコンボが増える
- コンボタイマーが機能する
- Missするとコンボが切れる
- GameOverすると操作が止まる
- ResultPanelが表示される
- Restartできる
- Titleへ戻れる
- 高速なマウス移動でも判定をすり抜けにくい
- イベントが重複処理されない

---

# 36. MVP対象外

以下はMVP完成後に検討する。

- 複数ランドルト環の同時出現
- ライフ制
- アイテム
- ステージ制
- ボス
- 特殊ランドルト環
- 曲線移動
- 加速・減速
- 追尾
- オンラインランキング
- セーブ
- ゲームパッド対応
- 難易度選択

---

# 37. 実装方針

いきなり全クラスを実装しない。

以下の順番で進める。

1. 既存Unityプロジェクトの構造確認
2. 必要パッケージ確認
   - VContainer
   - R3
3. Data / Paramの定義
4. 棒の基本操作
5. ランドルト環Prefab
6. 環の生成・移動
7. 当たり判定
8. 成功 / Miss / GameOver
9. Score / Combo
10. UI
11. ResultPanel
12. 演出
13. パラメータ調整

各段階でUnity上で動作確認できる状態を維持すること。
