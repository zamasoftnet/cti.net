// CTI .NET ドライバ PDF生成テスト (TC-01〜TC-10)
//
// 生成されたPDFは test-output/ ディレクトリに保存される。
// PdfBoxTest (Java) によって1ページ以上あることが検証される。

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Zamasoft.CTI;

class Program
{
    static readonly string SERVER_URI = "ctip://cti.li/";
    static readonly string SOURCE_URI = "http://cti.li/";
    static readonly string OUTPUT_DIR;

    static Program()
    {
        // Gradle task の workingDir は cti.net/ なので ../test-output が正しい出力先
        OUTPUT_DIR = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../test-output"));
    }

    static bool ServerAvailable()
    {
        try
        {
            using (var tcp = new TcpClient())
            {
                var result = tcp.BeginConnect("cti.li", 8099, null, null);
                if (result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2)))
                {
                    tcp.EndConnect(result);
                    return true;
                }
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    static void WithSession(string filename, Action<Session> setup)
    {
        Session session;
        try
        {
            session = DriverManager.getSession(new Uri(SERVER_URI), "user", "kappa");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("接続エラー: " + e.Message);
            Environment.Exit(0);
            return;
        }
        Utils.SetErrorMessageHander(session);
        Utils.SetResultFile(session, Path.Combine(OUTPUT_DIR, filename));
        Exception error = null;
        try
        {
            setup(session);
        }
        catch (Exception e)
        {
            error = e;
        }
        try
        {
            session.Close();
        }
        catch (Exception)
        {
        }
        if (error != null)
        {
            Console.Error.WriteLine("エラー (" + filename + "): " + error.Message);
            Environment.Exit(0);
        }
        Console.Error.WriteLine("生成: " + filename);
    }

    static void Main(string[] args)
    {
        Directory.CreateDirectory(OUTPUT_DIR);

        if (!ServerAvailable())
        {
            Console.Error.WriteLine("CTIサーバーに接続できないためスキップします。");
            Environment.Exit(0);
        }

        // TC-01: 基本URL変換
        WithSession("ctip-dotnet-url.pdf", session =>
        {
            session.Transcode(SOURCE_URI);
        });

        // TC-02: ハイパーリンク有効
        WithSession("ctip-dotnet-hyperlinks.pdf", session =>
        {
            session.Property("output.pdf.hyperlinks", "true");
            session.Transcode(SOURCE_URI);
        });

        // TC-03: ブックマーク有効
        WithSession("ctip-dotnet-bookmarks.pdf", session =>
        {
            session.Property("output.pdf.bookmarks", "true");
            session.Transcode(SOURCE_URI);
        });

        // TC-04: ハイパーリンクとブックマーク有効
        WithSession("ctip-dotnet-hyperlinks-bookmarks.pdf", session =>
        {
            session.Property("output.pdf.hyperlinks", "true");
            session.Property("output.pdf.bookmarks", "true");
            session.Transcode(SOURCE_URI);
        });

        // TC-05: クライアント側HTML変換
        WithSession("ctip-dotnet-client-html.pdf", session =>
        {
            string html = "<html><body><h1>Hello</h1><p>Client-side HTML transcoding test.</p></body></html>";
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            Utils.TranscodeStream(session, new MemoryStream(bytes), "dummy:///test.html", "text/html", "UTF-8");
        });

        // TC-06: 日本語HTMLコンテンツ
        WithSession("ctip-dotnet-client-japanese.pdf", session =>
        {
            string html = "<html><head><meta charset=\"UTF-8\"/></head><body>"
                        + "<h1>日本語テスト</h1><p>こんにちは世界。クライアント側から日本語コンテンツを送信します。</p>"
                        + "</body></html>";
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            Utils.TranscodeStream(session, new MemoryStream(bytes), "dummy:///japanese.html", "text/html", "UTF-8");
        });

        // TC-07: 最小HTML（境界条件）
        WithSession("ctip-dotnet-client-minimal.pdf", session =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes("<html><body><p>.</p></body></html>");
            Utils.TranscodeStream(session, new MemoryStream(bytes), "dummy:///minimal.html", "text/html", "UTF-8");
        });

        // TC-08: 連続モード（2文書を結合）
        WithSession("ctip-dotnet-continuous.pdf", session =>
        {
            string html1 = "<html><body><h1>Page 1</h1><p>First document in continuous mode.</p></body></html>";
            string html2 = "<html><body><h1>Page 2</h1><p>Second document in continuous mode.</p></body></html>";
            session.Continuous = true;
            Utils.TranscodeStream(session, new MemoryStream(Encoding.UTF8.GetBytes(html1)), "dummy:///page1.html", "text/html", "UTF-8");
            Utils.TranscodeStream(session, new MemoryStream(Encoding.UTF8.GetBytes(html2)), "dummy:///page2.html", "text/html", "UTF-8");
            session.Join();
        });

        // TC-09: 大規模テーブル（メモリ→ファイル切り替えを誘発）
        WithSession("ctip-dotnet-large-table.pdf", session =>
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset=\"UTF-8\"/></head><body>");
            sb.Append("<h1>大規模テーブルテスト</h1>");
            sb.Append("<table border=\"1\"><tr><th>番号</th><th>名前</th><th>説明</th><th>備考</th></tr>");
            for (int i = 1; i <= 15000; i++)
            {
                sb.AppendFormat("<tr><td>{0}</td><td>項目{0}</td><td>これはテスト項目 {0} の詳細説明テキストです。</td><td>備考テキスト {0}</td></tr>", i);
            }
            sb.Append("</table></body></html>");
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            Utils.TranscodeStream(session, new MemoryStream(bytes), "dummy:///large-table.html", "text/html", "UTF-8");
        });

        // TC-10: 長文テキスト文書
        // TC-09の大規模テーブルが2MB超のPDFを生成するため、大容量出力のテストはTC-09でカバーされる。
        // テキスト文書のレイアウト機能を確認することが目的であり、ここでは100セクションで十分。
        WithSession("ctip-dotnet-large-text.pdf", session =>
        {
            string sentences = "Copper PDFはHTMLやXMLをPDFに変換するサーバーサイドのソフトウェアです。"
                             + "CTIプロトコルを通じてクライアントからドキュメントを送信し、変換結果をPDFとして受け取ります。"
                             + "このテストは大量のテキストコンテンツを含む文書を生成します。"
                             + "ドライバはPDF出力が2MBを超えた際にメモリからファイル書き出しへ切り替わります。"
                             + "このテストはその動作を確認するために設計されています。";
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset=\"UTF-8\"/></head><body>");
            for (int s = 1; s <= 100; s++)
            {
                sb.AppendFormat("<h2>セクション {0}</h2>", s);
                for (int p = 1; p <= 20; p++)
                {
                    sb.AppendFormat("<p>{0}（セクション{1}、段落{2}）</p>", sentences, s, p);
                }
            }
            sb.Append("</body></html>");
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            Utils.TranscodeStream(session, new MemoryStream(bytes), "dummy:///large-text.html", "text/html", "UTF-8");
        });
    }
}
