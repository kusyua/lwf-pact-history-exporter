# LWF Modding Notes

LWF MOD 開発時に、調査で再発見しにくい点を残す場所。

## 確認済み前提

`meowous3/lwf-modding` の資料では以下。

- Unity 6000.0.80f1
- Mono / x64
- BepInEx 5
- plugin target: `netstandard2.1`
- full release Steam app: `3971650`
- demo Steam app: `4638750`
- full release executable: `LazyWitchsFactory.exe`
- demo executable: `LazyWitchFactory.exe`
- full release data dir: `LazyWitchsFactory_Data`
- demo data dir: `LazyWitchFactory_Data`

これらはゲーム更新時に再確認する。

## Reverse engineering

ゲームコードは obfuscation されていないとの報告あり。
`Assembly-CSharp.dll` を `ilspycmd` / ILSpy で読む。

Harmony patch を決めるときは最低限:

- 対象メソッドの正確な signature
- 呼び出し元
- 値の生成元
- save/load で値が戻されないか
- 小さすぎて inline されないか

を確認する。

## 既知の罠

### Mono inlining

小さいメソッドは Harmony patch が適用されたように見えても実行されない場合がある。
負荷の大きい重要値は計算メソッドを patch するより、適切な lifecycle で field / property に直接反映した方が安全な場合がある。

### Synthetic enum

vanilla 以外の enum 値を追加する場合:

- `Enum.IsDefined`
- `Enum.GetValues`
- range guard
- serializer / parser
- save validation
- run history validation
- UI 側で構築済みの enum 配列

を調べる。

### Save

実験的 MOD ではセーブ破損・進行汚染を避ける。
実機テスト前にバックアップする。
書き込み抑制をする場合、単に disk persist を止めるだけでは、メモリ上の変更が後の別 save で書き込まれる可能性がある。

## 調査ログ

MOD ごとに、この下へ調査日・ゲームバージョン・対象 symbol・観測結果を追加する。

### YYYY-MM-DD / game version X.Y.Z

- Purpose:
- Symbols inspected:
- Patch candidate:
- Static findings:
- Runtime findings:
- Remaining uncertainty:

### 2026-08-31 / Unity 6000.0.80f1

- Purpose: リザルト画面から契約履歴パネルを縦並びPNGとして出力し、出力日時と契約取得時刻を表示できるか調査。
- Symbols inspected: `ResultUIManager`, `ResultWindow`, `PactHistoryUIController`, `PactHistoryStore`, `PactCellComponents`, `PactCellSnapshot`, `WindowWithCells`, `GameStateManager.RecordRunPactHistoryEntry`。
- Patch candidate: `ResultWindow` 構築後に出力用 `ResultCellData` とセル数を追加する局所的な constructor postfix。実装前に実機で生成タイミングを確認する。
- Static findings: `PactHistoryStore.GetSnapshots()` から表示用スナップショットを取得でき、ゲームの `PactCellComponents.ApplySnapshot()` で元のパネル表示を再利用できる。契約取得時には `GameStateManager.GetElapsedGameplaySeconds()` と同じラン内経過秒が `RunPactHistoryEntryV1.acquiredAtElapsedGameplaySeconds` に記録される。MODは `PactHistoryStore.Add` を監視し、セーブへ触れずに同時点の経過秒をメモリ保持できる。
- Runtime findings: 未確認。
- Remaining uncertainty: パネルの実寸、Canvasの描画モード、`SystemInfo.maxTextureSize`、大量履歴時の分割単位、出力完了通知方法、取得時刻ラベルを既存パネルのどの位置に追加するか。

### Timestamp configuration policy

- 画像ヘッダーの出力日時と、契約ごとのラン内取得時刻は別機能として扱う。
- 契約ごとの取得時刻は `IncludePactTimestamps`（既定値 `true`）の BepInEx 設定で無効化できるようにする。
- 公式機能との重複や表示上の問題が起きた場合は、機能削除ではなく上記設定で即座に回避できるようにする。
- 設定キーと既定値は実装時にREADMEへ記載し、実機確認する。
