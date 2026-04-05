# CTI Driver for .NET

Copper PDF 文書変換サーバーに接続するための.NETドライバ（C#, VB.NET等）

バージョン: 2.1.0

## API ドキュメント

- **オンライン**: https://zamasoftnet.github.io/cti.net/
- **NuGet**: https://www.nuget.org/packages/Zamasoft.CTI/

## 動作要件

- .NET Standard 2.0 以降（.NET Framework 4.6.1+ / .NET Core 2.0+ / .NET 5–9）

## インストール

### NuGet を使う方法（推奨）

```bash
dotnet add package Zamasoft.CTI
```

または `PackageReference` をプロジェクトファイルに追加:

```xml
<PackageReference Include="Zamasoft.CTI" Version="2.1.0" />
```

### GitHub Releases のアーカイブを使う方法

NuGet を使わない場合は、GitHub Releases からビルド済み zip を取得できます。

- **Releases**: https://github.com/zamasoftnet/cti.net/releases/latest
- **配布形式**: `cti-dotnet-{VERSION}.zip`

zip には `CTI.dll`, `CTI.xml`, `README.md`, `apidoc/` が含まれます。展開後、`CTI.dll` をプロジェクト参照に追加して利用してください。

### ソースからビルドする方法

`CTI/CTI.sln` をVisual Studio または `dotnet build` でビルドしてください。

配布用 zip を生成する場合は、リポジトリルートで以下を実行してください。

```bash
pwsh ./scripts/build-release-zip.ps1
```

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
| `CTI/CTI/` | ライブラリ本体（netstandard2.0）|
| `CTI/ConsoleExamples/` | C# サンプルプログラム |
| `CTI/ConsoleExamplesVB/` | VB.NET サンプルプログラム |

## テストの実行方法

PDF生成テスト（`src/test/generate-pdfs/`）は .NET 9 対応の SDK スタイルプロジェクトです。
CTIライブラリのソースを直接取り込んでビルドするため、別途DLLは不要です。

```bash
cd cti.net
dotnet run --project src/test/generate-pdfs
```

生成されたPDFは `../test-output/` ディレクトリに保存されます。
Java側の `PdfBoxTest` によって全PDFの妥当性（1ページ以上あること）が検証されます。

Gradleからの実行：
```bash
./gradlew :cti-driver-ctip:generateDotNetPdfs
```

## ライセンス

Apache License 2.0

Copyright (c) 2011-2015 Zamasoft

## オンラインマニュアル

http://dl.cssj.jp/docs/copper/3.0/html/3423_ctip2_dotnet.html

## 変更履歴

### v2.1.0 (2026/3/9)

- .NET Framework 4.0 から .NET Standard 2.0 に移行（.NET 5–9 / Linux / macOS 対応）。
- NuGet パッケージ（`Zamasoft.CTI`）として配布可能に。
- `Thread.Abort()` を `Thread.Interrupt()` / `Join()` に置き換え（.NET Core 対応）。

### v2.0.1 (2015/04/09)

- SSL/TLS対応
- タイムアウト設定（`ctip://hostname/?timeout=10000`）
- Copper PDF 3.1.1認証対応

### v2.0.0 (2013/02/08)

- .NET 3.5と4.0のDLLを添付

### v2.0.0 (2011/11/06)

- 初回リリース
