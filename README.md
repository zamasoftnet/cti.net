# CTI Driver for .NET

Copper PDF 文書変換サーバーに接続するための.NETドライバ（C#, VB.NET等）

バージョン: 2.0.1

## 動作要件

- .NET Framework 3.5以降

## インストール

ビルド済みDLLを参照に追加するか、ソースからビルドしてください。

- .NET Framework 3.5の場合: `dll/3.5/CTI.dll` を参照に追加
- .NET Framework 4.0の場合: `dll/4.0/CTI.dll` を参照に追加
- ソースからビルドする場合: `CTI.sln` をVisual Studioで開いてビルド

## 基本的な使い方

名前空間 `Zamasoft.CTI` を使用します。

### C# の例

```csharp
using Zamasoft.CTI;
using Zamasoft.CTI.Result;

// セッションを取得
using (Session session = DriverManager.getSession(
    new Uri("ctip://localhost:8099/"), "user", "kappa"))
{
    // 出力先をファイルに設定
    Utils.SetResultFile(session, "output.pdf");

    // HTML文書を変換
    Utils.TranscodeFile(session, "test.html", "text/html", null);
}
```

### VB.NET の例

```vb
Imports Zamasoft.CTI

Using session As Session = DriverManager.getSession(
    New Uri("ctip://localhost:8099/"), "user", "kappa")

    Utils.SetResultFile(session, "output.pdf")
    Utils.TranscodeFile(session, "test.html", "text/html", Nothing)
End Using
```

## API概要

### Session インターフェース

| メンバー | 種別 | 説明 |
|---|---|---|
| Results | プロパティ | 出力先の設定 |
| MessageHandler | プロパティ | メッセージハンドラの設定 |
| ProgressListener | プロパティ | 進捗リスナーの設定 |
| SourceResolver | プロパティ | リソースリゾルバの設定 |
| Continuous | プロパティ | 連続モードの設定 |
| GetServerInfo(uri) | メソッド | サーバー情報の取得 |
| Property(key, value) | メソッド | プロパティの設定 |
| Resource(info) | メソッド | リソースの送信 |
| Transcode(info) | メソッド | 変換の実行（ストリーム） |
| Transcode(uri) | メソッド | 変換の実行（URI） |
| Join() | メソッド | 結果の結合 |
| Abort(mode) | メソッド | 変換の中断 |
| Reset() | メソッド | セッションのリセット |
| Close() | メソッド | セッションのクローズ |

### Utils ヘルパークラス

`Utils` クラスは、よく使う操作を簡単に行うためのヘルパーメソッドを提供します。

- `SetResultFile` - 出力先をファイルに設定
- `SetResultStream` - 出力先をストリームに設定
- `TranscodeFile` - ファイルを変換
- `TranscodeStream` - ストリームを変換
- `SendResourceFile` - リソースファイルの送信

## 付属物

| パス | 説明 |
|---|---|
| `CTI/` | ソースとサンプルプログラム |
| `dll/3.5/CTI.dll` | .NET 3.5向けDLL |
| `dll/4.0/CTI.dll` | .NET 4.0向けDLL |

## ドキュメント生成方法

APIドキュメントは従来どおり Sandcastle を使用して生成できます。
Sandcastle は更新停止が進んでいるため、将来的には DocFX などへの移行を検討しています。

## テストの実行方法

このドライバはレガシー扱いのため、ユニットテストは実装していません。
配布物のビルド確認（`ant dist`）を優先しています。

## ライセンス

Apache License 2.0

Copyright (c) 2011-2015 Zamasoft

## オンラインマニュアル

http://dl.cssj.jp/docs/copper/3.0/html/3423_ctip2_dotnet.html

## 変更履歴

### v2.0.1 (2015/04/09)

- SSL/TLS対応
- タイムアウト設定（`ctip://hostname/?timeout=10000`）
- Copper PDF 3.1.1認証対応

### v2.0.0 (2013/02/08)

- .NET 3.5と4.0のDLLを添付

### v2.0.0 (2011/11/06)

- 初回リリース
