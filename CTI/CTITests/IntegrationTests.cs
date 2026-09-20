using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Xunit;
using Xunit.Abstractions;
using Zamasoft.CTI;
using Zamasoft.CTI.Progress;
using Zamasoft.CTI.Result;
using Zamasoft.CTI.Source;

namespace CTITests
{
    /// <summary>
    /// CTI ドライバの統合テストです。
    /// ctip://cti.li/ に接続してテストを行います。
    /// サーバーに接続できない場合はテストをスキップします。
    /// </summary>
    public class IntegrationTests
    {
        // 接続試験マトリクスの共通契約(copperpdf4/docs/design/2026-09-20-cti-driver-tls-test-matrix-design.md §2):
        // CTI_TLS_INSECURE=1 で証明書を検証しない(URI に ?insecure=1 を付ける)、
        // CTI_EXPECT_REJECT=1 で「証明書の検証で拒否されること」だけを試験する(Test10 を --filter で選ぶ)
        private static readonly bool Insecure = Environment.GetEnvironmentVariable("CTI_TLS_INSECURE") == "1";
        private static readonly bool ExpectReject = Environment.GetEnvironmentVariable("CTI_EXPECT_REJECT") == "1";
        private static readonly Uri ServerUri = ResolveServerUri();

        private static Uri ResolveServerUri()
        {
            string raw = Environment.GetEnvironmentVariable("CTI_SERVER_URI") ?? "ctip://cti.li/";
            if (Insecure && ExpectReject)
                throw new InvalidOperationException("CTI_TLS_INSECURE=1 と CTI_EXPECT_REJECT=1 は同時に指定できません");
            if (raw.Contains("insecure"))
                throw new InvalidOperationException("CTI_SERVER_URI に insecure を含めず、CTI_TLS_INSECURE=1 で指定してください");
            if (Insecure)
                raw = raw + (raw.Contains("?") ? "&" : "?") + "insecure=1";
            // CTI_TLS_CA_FILE: 独自の認証局(自己署名の試験サーバー)を ?cafile= で信頼させる(2.2.2)。
            // 他言語版の SSL_CERT_FILE に相当。OS のストアには触らない
            string caFile = Environment.GetEnvironmentVariable("CTI_TLS_CA_FILE");
            if (!string.IsNullOrEmpty(caFile) && !Insecure)
                raw = raw + (raw.Contains("?") ? "&" : "?") + "cafile=" + Uri.EscapeDataString(caFile);
            return new Uri(raw);
        }
        private static readonly string User =
            Environment.GetEnvironmentVariable("CTI_TEST_USER") ?? "user";
        private static readonly string Password =
            Environment.GetEnvironmentVariable("CTI_TEST_PASSWORD") ?? "kappa";

        private static readonly string DataDir =
            Path.Combine(AppContext.BaseDirectory, "data");
        private static readonly string OutDir =
            Path.Combine(AppContext.BaseDirectory, "out");

        private static readonly bool ServerAvailable = CheckServer();

        private static bool CheckServer()
        {
            string host = ServerUri.Host;
            int port = ServerUri.Port > 0 ? ServerUri.Port : 8099;
            try
            {
                using (var tcp = new TcpClient())
                {
                    var result = tcp.BeginConnect(host, port, null, null);
                    bool ok = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (ok) tcp.EndConnect(result);
                    return ok;
                }
            }
            catch { return false; }
        }

        private Session CreateSession() =>
            DriverManager.getSession(ServerUri, User, Password);

        private void TranscodeHtml(Session session, string outputFile)
        {
            Utils.SetResultFile(session, outputFile);
            session.Resource(
                new SourceInfo("test.css", "text/css", null,
                    new FileInfo(Path.Combine(DataDir, "test.css")).Length),
                new FileStream(Path.Combine(DataDir, "test.css"), FileMode.Open, FileAccess.Read));
            session.Transcode(
                new SourceInfo("test.html", "text/html", null,
                    new FileInfo(Path.Combine(DataDir, "test.html")).Length),
                new FileStream(Path.Combine(DataDir, "test.html"), FileMode.Open, FileAccess.Read));
        }

        private static void AssertPdf(string path)
        {
            Assert.True(File.Exists(path), path + " が生成されること");
            byte[] header = new byte[4];
            using (var fp = new FileStream(path, FileMode.Open, FileAccess.Read))
                fp.Read(header, 0, 4);
            Assert.Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 }, header); // %PDF
        }

        private bool EnsureServer()
        {
            // 到達不能は失敗(黙って成功にしない。2026-09-20、マトリクスの契約)
            int port = ServerUri.Port > 0 ? ServerUri.Port : 8099;
            Assert.True(ServerAvailable, "Copper PDF サーバー (" + ServerUri.Host + ":" + port + ") に接続できません。");
            return true;
        }

        static IntegrationTests()
        {
            Directory.CreateDirectory(OutDir);
        }

        // 試験の出力(CTI-MATRIX の行)は xUnit の ITestOutputHelper 経由でしか残らない。
        // dotnet test --logger "console;verbosity=detailed" で見える
        private readonly ITestOutputHelper _output;

        public IntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test01_Connection()
        {
            if (!EnsureServer()) return;
            using (Session session = CreateSession())
                Assert.NotNull(session);
        }

        [Fact]
        public void Test02_ServerInfo()
        {
            if (!EnsureServer()) return;
            using (Session session = CreateSession())
            using (StreamReader info = new StreamReader(
                session.GetServerInfo("http://www.cssj.jp/ns/ctip/version")))
            {
                string text = info.ReadToEnd();
                Assert.True(text.Length > 0, "サーバー情報が取得できる");
            }
        }

        [Fact]
        public void Test03_HtmlToPdfFile()
        {
            if (!EnsureServer()) return;
            string outFile = Path.Combine(OutDir, "dotnet-output-file.pdf");
            if (File.Exists(outFile)) File.Delete(outFile);

            using (Session session = CreateSession())
                TranscodeHtml(session, outFile);

            AssertPdf(outFile);
        }

        [Fact]
        public void Test04_OutputToDirectory()
        {
            if (!EnsureServer()) return;
            string outputDir = Path.Combine(OutDir, "output-dir");
            if (Directory.Exists(outputDir))
                foreach (var f in Directory.GetFiles(outputDir)) File.Delete(f);
            else
                Directory.CreateDirectory(outputDir);

            using (Session session = CreateSession())
            {
                session.Property("output.type", "image/jpeg");
                session.Results = new FileResults(
                    Path.Combine(outputDir, "page-"), ".jpg");
                session.Transcode(
                    new SourceInfo("test.html", "text/html", null,
                        new FileInfo(Path.Combine(DataDir, "test.html")).Length),
                    new FileStream(Path.Combine(DataDir, "test.html"),
                        FileMode.Open, FileAccess.Read));
            }

            string[] jpgs = Directory.GetFiles(outputDir, "*.jpg");
            Assert.True(jpgs.Length > 0, "出力ディレクトリにJPEGファイルが生成される");
        }

        [Fact]
        public void Test05_PropertySetting()
        {
            if (!EnsureServer()) return;
            string outFile = Path.Combine(OutDir, "dotnet-property.pdf");
            if (File.Exists(outFile)) File.Delete(outFile);

            using (Session session = CreateSession())
            {
                session.Property("output.pdf.version", "1.5");
                TranscodeHtml(session, outFile);
            }

            AssertPdf(outFile);
        }

        [Fact]
        public void Test06_ResolverCallback()
        {
            if (!EnsureServer()) return;
            string outFile = Path.Combine(OutDir, "dotnet-resolver.pdf");
            if (File.Exists(outFile)) File.Delete(outFile);

            var resolver = new TestResolver(DataDir);

            using (Session session = CreateSession())
            {
                session.SourceResolver = resolver;
                Utils.SetResultFile(session, outFile);
                session.Transcode(
                    new SourceInfo("test.html", "text/html", null,
                        new FileInfo(Path.Combine(DataDir, "test.html")).Length),
                    new FileStream(Path.Combine(DataDir, "test.html"),
                        FileMode.Open, FileAccess.Read));
            }

            Assert.True(resolver.Resolved, "resolver が呼ばれてリソースを解決できる");
            AssertPdf(outFile);
        }

        [Fact]
        public void Test07_ProgressCallback()
        {
            if (!EnsureServer()) return;
            var progress = new TestProgress();

            using (Session session = CreateSession())
            {
                session.Results = new SingleResult(Stream.Null);
                session.ProgressListener = progress;
                session.Property("input.include", "https://www.w3.org/**");
                session.Transcode("https://www.w3.org/TR/xslt-10/");
            }

            Assert.True(progress.ReadCount > 0, "進行状況コールバックが呼ばれる");
        }

        [Fact]
        public void Test08_Reset()
        {
            if (!EnsureServer()) return;
            string out1 = Path.Combine(OutDir, "dotnet-reset-1.pdf");
            string out2 = Path.Combine(OutDir, "dotnet-reset-2.pdf");
            if (File.Exists(out1)) File.Delete(out1);
            if (File.Exists(out2)) File.Delete(out2);

            using (Session session = CreateSession())
            {
                TranscodeHtml(session, out1);
                session.Reset();
                TranscodeHtml(session, out2);
            }

            AssertPdf(out1);
            AssertPdf(out2);
        }

        // ---- プロトコルの周辺機能(2026-09-20、接続試験マトリクスの拡張)。
        // 主要機能の 8 項目に、メッセージ受信・中断・ストリーム出力・連続結合を足す。7 本のドライバで同じ 4 項目。

        private const string MissingCssHtml = "<html><head><link rel=\"stylesheet\" href=\"missing.css\"></head><body><p>message test</p></body></html>";

        private static byte[] BigHtml(int paragraphs)
        {
            var sb = new System.Text.StringBuilder("<html><body>");
            string filler = new string('x', 300);
            for (int i = 0; i < paragraphs; i++) sb.Append("<p>paragraph ").Append(i).Append(' ').Append(filler).Append("</p>");
            sb.Append("</body></html>");
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static void TranscodeBytes(Session session, byte[] html)
        {
            session.Transcode(new SourceInfo(".", "text/html", "UTF-8", html.Length), new MemoryStream(html));
        }

        private sealed class CollectingMessageHandler : Zamasoft.CTI.Message.MessageHandler
        {
            public readonly List<(short code, string[] args, string mes)> Messages = new List<(short, string[], string)>();
            public void Message(short code, string[] args, string mes) { Messages.Add((code, args ?? new string[0], mes ?? "")); }
        }

        private static void AssertPdfBytes(byte[] bytes, string what)
        {
            Assert.True(bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46, what + ": PDF でない");
        }

        [Fact]
        public void Test11_MessageCallback()
        {
            // 存在しないスタイルシートを参照する文書を変換し、サーバーのエラーメッセージがハンドラに届く(引数にその名前が入る)
            if (!EnsureServer()) return;
            var handler = new CollectingMessageHandler();
            var buf = new MemoryStream();
            using (Session session = CreateSession())
            {
                session.MessageHandler = handler;
                Utils.SetResultStream(session, buf);
                TranscodeBytes(session, System.Text.Encoding.UTF8.GetBytes(MissingCssHtml));
            }
            AssertPdfBytes(buf.ToArray(), "message test");
            var hits = handler.Messages.FindAll(m => Array.IndexOf(m.args, "missing.css") >= 0 || m.mes.Contains("missing.css"));
            Assert.True(hits.Count > 0, "missing.css についてのメッセージが届いていない: " + string.Join(" | ", handler.Messages.ConvertAll(m => m.code + " " + m.mes)));
            foreach (var m in hits) Assert.True(m.code > 0);
        }

        [Fact]
        public void Test12_Abort()
        {
            // 本文の送信中に Abort を送ると変換が止まり(完全な出力が返らない)、Reset 後に同じセッションで再変換できる。
            // サーバーが中断をどのメッセージで報告するかは版で違うので見ない
            if (!EnsureServer()) return;
            byte[] html = BigHtml(3000);
            using (Session session = CreateSession())
            {
                var full = new MemoryStream();
                Utils.SetResultStream(session, full);
                TranscodeBytes(session, html);
                AssertPdfBytes(full.ToArray(), "full");
                session.Reset();

                var aborted = new MemoryStream();
                Utils.SetResultStream(session, aborted);
                int half = html.Length / 2;
                try
                {
                    using (Stream outStream = session.Transcode(new SourceInfo(".", "text/html", "UTF-8", html.Length)))
                    {
                        outStream.Write(html, 0, half);
                        outStream.Flush();
                        session.Abort(AbortMode.FORCE);
                        outStream.Write(html, half, html.Length - half);
                    }
                }
                catch (IOException e)
                {
                    // 中断がサーバーのエラー報告として届き、ドライバが例外にする版がある(3.2.33)。中断の報告としては正しい
                    _output.WriteLine("abort reported as: " + e.Message);
                }
                Assert.True(aborted.ToArray().Length < full.ToArray().Length, "中断したのに完全な出力が返った");
                session.Reset();

                var again = new MemoryStream();
                Utils.SetResultStream(session, again);
                TranscodeBytes(session, System.Text.Encoding.UTF8.GetBytes("<p>after abort</p>"));
                AssertPdfBytes(again.ToArray(), "after abort");
            }
        }

        [Fact]
        public void Test13_OutputStream()
        {
            // SetResultStream(SingleResult(Stream))で結果がストリームに書かれる
            if (!EnsureServer()) return;
            var buf = new MemoryStream();
            using (Session session = CreateSession())
            {
                Utils.SetResultStream(session, buf);
                session.Resource(
                    new SourceInfo("test.css", "text/css", null, new FileInfo(Path.Combine(DataDir, "test.css")).Length),
                    new FileStream(Path.Combine(DataDir, "test.css"), FileMode.Open, FileAccess.Read));
                session.Transcode(
                    new SourceInfo("test.html", "text/html", null, new FileInfo(Path.Combine(DataDir, "test.html")).Length),
                    new FileStream(Path.Combine(DataDir, "test.html"), FileMode.Open, FileAccess.Read));
            }
            AssertPdfBytes(buf.ToArray(), "stream");
            Assert.True(buf.ToArray().Length > 100);
        }

        [Fact]
        public void Test14_ContinuousJoin()
        {
            // 連続モードで 2 文書を変換して Join すると 1 つの PDF になる(1 文書より大きい)
            if (!EnsureServer()) return;
            var single = new MemoryStream();
            using (Session session = CreateSession())
            {
                Utils.SetResultStream(session, single);
                TranscodeBytes(session, System.Text.Encoding.UTF8.GetBytes("<p>doc 0</p>"));
            }
            var joined = new MemoryStream();
            using (Session session = CreateSession())
            {
                Utils.SetResultStream(session, joined);
                session.Continuous = true;
                for (int i = 0; i < 2; i++)
                    TranscodeBytes(session, System.Text.Encoding.UTF8.GetBytes("<p>doc " + i + "</p>"));
                session.Join();
            }
            AssertPdfBytes(joined.ToArray(), "joined");
            Assert.True(joined.ToArray().Length > single.ToArray().Length, "結合した出力が 1 文書より大きくない");
        }

        [Fact]
        public void Test09_AuthenticationFailure()
        {
            bool reachable;
            try
            {
                int port = ServerUri.Port > 0 ? ServerUri.Port : 8099;
                using (var tcp = new TcpClient())
                {
                    var r = tcp.BeginConnect(ServerUri.Host, port, null, null);
                    reachable = r.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (reachable) tcp.EndConnect(r);
                }
            }
            catch { reachable = false; }

            Assert.True(reachable, "サーバーに接続できません。");

            Assert.Throws<IOException>(() =>
                DriverManager.getSession(ServerUri, "invalid-user", "invalid-password"));
        }

        /// <summary>
        /// 拒否試験(tls-reject / tls-badname)。CTI_EXPECT_REJECT=1 のときは、接続が
        /// 証明書の検証で拒否されることだけを確かめる(変換まで進む・接続拒否・認証失敗は
        /// 成功に数えない)。通常のときは、接続が拒否されないことを確かめる。
        /// </summary>
        [Fact]
        public void Test10_CertificateVerification()
        {
            if (!ExpectReject)
            {
                EnsureServer();
                using (var session = CreateSession())
                {
                    Assert.NotNull(session.GetServerInfo("http://www.cssj.jp/ns/ctip/version"));
                }
                return;
            }
            var error = Assert.Throws<System.Security.Authentication.AuthenticationException>(() =>
            {
                using (var session = CreateSession())
                {
                    session.GetServerInfo("http://www.cssj.jp/ns/ctip/version");
                }
            });
            _output.WriteLine("CTI-MATRIX reject: " + error.Message);
            Assert.Contains("certificate", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ヘルパークラス

        private class TestResolver : SourceResolver
        {
            public bool Resolved = false;
            private readonly string _dataDir;
            public TestResolver(string dataDir) { _dataDir = dataDir; }

            public Stream Resolve(string uri, ref SourceInfo info)
            {
                if (uri.EndsWith("test.css"))
                {
                    Resolved = true;
                    info = new SourceInfo(uri, "text/css", null, -1);
                    return new FileStream(Path.Combine(_dataDir, "test.css"),
                        FileMode.Open, FileAccess.Read);
                }
                return null;
            }
        }

        private class TestProgress : ProgressListener
        {
            public int ReadCount = 0;
            public void SourceLength(long length) { }
            public void Progress(long read) { ReadCount++; }
        }
    }
}
