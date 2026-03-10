# リリース手順

## リリース方法

`CTI/CTI/CTI.csproj` の `<Version>` を更新し、バージョンタグを push します。

```bash
git tag v2.1.0
git push origin v2.1.0
```

GitHub Actions が以下を自動実行します：

1. ビルド・テスト（`dotnet build` / `dotnet test`）
2. DocFX によるAPIドキュメント生成
3. NuGet.org へパッケージを公開（要: `NUGET_API_KEY` シークレット）
4. GitHub Releases にバイナリ zip を公開（`cti-dotnet-{VERSION}.zip`）
5. GitHub Pages にドキュメントをデプロイ

## NuGet シークレット設定

GitHub リポジトリの Settings → Secrets → `NUGET_API_KEY` に NuGet.org の API キーを設定してください。

## ドキュメント

- **GitHub Pages**: https://zamasoftnet.github.io/cti.net/
- **NuGet**: https://www.nuget.org/packages/Zamasoft.CTI/
- リリース時に自動更新
