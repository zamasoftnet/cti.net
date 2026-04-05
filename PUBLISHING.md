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
3. Trusted Publishing で NuGet.org へパッケージを公開
4. `scripts/build-release-zip.ps1` でバイナリ zip を生成して GitHub Releases に公開（`build/cti-dotnet-{VERSION}.zip`）
5. GitHub Pages にドキュメントをデプロイ

## Trusted Publishing 設定

NuGet.org の Trusted Publishing で以下の policy を設定してください。

- Repository Owner: `zamasoftnet`
- Repository: `cti.net`
- Workflow File: `publish.yml`
- Environment: workflow 側で `environment:` を使わないなら空欄

GitHub リポジトリの Settings → Secrets and variables → Actions に、NuGet.org のプロフィール名を `NUGET_USER` として登録してください。

ワークフローは `NuGet/login@v1` を使って GitHub OIDC トークンを短命の NuGet API キーに交換します。長期の `NUGET_API_KEY` シークレットは不要です。

## ドキュメント

- **GitHub Pages**: https://zamasoftnet.github.io/cti.net/
- **NuGet**: https://www.nuget.org/packages/Zamasoft.CTI/
- リリース時に自動更新
