using System;
using Zamasoft.CTI;

namespace examples
{
    /// <summary>
    /// 複数の結果を結合したPDFを生成します。
    /// </summary>
    class Continuous
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // エラーメッセージを標準エラー出力に表示する
                Utils.SetErrorMessageHander(session);

                // test.pdfに結果を出力する
                Utils.SetResultFile(session, "test.pdf");

                // 結果の結合を開始する
                session.Continuous = true;

                // リソースの送信
                Utils.SendResourceFile(session, "files\\test.css", "text/css", null);

                // 文書の送信
                Utils.TranscodeFile(session, "files\\test.html", "text/html", null);

                // 文書の送信
                Utils.TranscodeFile(session, "files\\test.html", "text/html", null);

                // 結果の結合を完了する
                session.Join();
            }
        }
    }
}
