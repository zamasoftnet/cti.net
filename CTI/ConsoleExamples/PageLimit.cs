using System;
using Zamasoft.CTI;

namespace examples
{
    /// <summary>
    /// 複数の結果を結合したPDFを生成します。
    /// </summary>
    class PageLimit
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // エラーメッセージを標準エラー出力に表示する
                Utils.SetErrorMessageHander(session);

                // test.pdfに結果を出力する
                Utils.SetResultFile(session, "test.pdf");

                // 高さの設定
                session.Property("output.page-height", "100mm");

                // ページ数制限
                session.Property("output.page-limit", "3");

                // 途中まで出力する
                session.Property("output.page-limit.abort", "normal");

                // リソースの送信
                Utils.SendResourceFile(session, "files\\test.css", "text/css", null);

                // 文書の送信
                Utils.TranscodeFile(session, "files\\test.html", "text/html", null);
            }
        }
    }
}
