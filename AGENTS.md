# AGENTS.md

このリポジトリは Lazy Witch's Factory 用 `LWF Pact History Exporter` MOD。

## 最優先

- 変更前に `README.md` と `docs/MODDING_NOTES.md` を読む。
- 大きな実装を一度に行わず、ロード確認 → 対象調査 → 最小パッチ → 実機確認の順で進める。
- ゲーム内部 API を推測しない。Harmony patch を書く前に、現在の `Assembly-CSharp.dll` を逆コンパイルして対象メソッドとシグネチャを確認する。
- ゲーム本体 DLL や BepInEx DLL をリポジトリへコピー・コミットしない。
- `Directory.Build.user.props` はローカル専用。コミットしない。
- セーブデータを書き換える可能性がある変更では、実機確認前に人間へバックアップを依頼する。

## 技術前提

- Unity 6000.0.80f1 / Mono / x64
- BepInEx 5
- Harmony
- `netstandard2.1`
- ゲームコードは `*_Data/Managed/Assembly-CSharp.dll`
- ゲーム / BepInEx 参照は `Private=false`

資料の値は過去バージョン由来の可能性がある。現在のゲーム版で必ず再確認する。

## 作業フロー

1. 目的と変更対象を短く整理する。
2. `Assembly-CSharp.dll` を逆コンパイルし、関連クラス・呼び出し元・保存経路を読む。
3. Harmony patch の対象を最小化する。
4. `dotnet build -c Release` を通す。
5. 必要なら静的テストを追加・実行する。
6. 実機確認が必要なものは、人間へ「1回の起動 + 1つの確認」に絞った手順を渡す。
7. 実機で観測した値・ログを根拠に次の変更を行う。

## Harmony / Mono の注意

- `[HarmonyPatch]` の対象名・シグネチャはコンパイル時に保証されない。
- 小さいメソッドは Mono に inline され、patch 済みに見えても呼ばれない場合がある。
- 重要な値は「patch を適用した」というログではなく、適用後の実値をログする。
- compiler-generated method の ordinal 名を固定文字列で頼らない。
- synthetic enum を使う場合、`Enum.IsDefined` / `Enum.GetValues` / save validation / history validation を調査する。
- save 書き込みを止める必要がある場合、メソッド名列挙より実際の保存 funnel / call path を追う。

## ビルド

```powershell
dotnet build .\src\LwfPactHistoryExporter\LwfPactHistoryExporter.csproj -c Release
```

## 公開リポジトリとしての注意

- ゲーム本体のファイル、逆コンパイル結果、画像・音声などのゲーム資産をコミットしない。
- ローカル絶対パス、ユーザー名、ログ、セーブ、生成PNG、認証情報をコミットしない。
- 公開前に `docs/RELEASE_CHECKLIST.md` を完了する。

## 実機確認

Codex はゲーム内挙動を推測で成功扱いしない。
ロード確認は `BepInEx/LogOutput.log`、挙動確認はゲーム画面または具体的なログ値で行う。

## 変更方針

- 不要な依存パッケージを増やさない。
- まず BepInEx + Harmony + game assemblies の範囲で解決を試す。
- public API のように扱えるものがないため、ゲーム更新で壊れる前提で patch point を局所化する。
- 不明な挙動は docs に仮説として残し、事実と分ける。
