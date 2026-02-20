using System;
using System.IO;
using Zamasoft.CTI;

namespace examples
{
    /// <summary>
    /// １つのセッションで繰り返し変換します。
    /// </summary>
    class Reset
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // エラーメッセージを標準エラー出力に表示する
                Utils.SetErrorMessageHander(session);

                // test.pdfに結果を出力する
                Utils.SetResultFile(session, "reset-1.pdf");

                // リソースの送信
                Utils.SendResourceStream(session, new FileStream("files\\test.css", FileMode.Open, FileAccess.Read), "test.css", "text/css", null);

                // 文書の送信
                Utils.TranscodeStream(session, new FileStream("files\\test.html", FileMode.Open, FileAccess.Read), "test.html", "text/html", null);

                //事前に送って変換
                Utils.SetResultFile(session, "reset-2.pdf");
                Utils.SendResourceStream(session, new FileStream("files\\test.html", FileMode.Open, FileAccess.Read), "test.html", "text/html", null);
                session.Transcode("test.html");

                //同じ文書を変換
                Utils.SetResultFile(session, "reset-3.pdf");
                session.Transcode("test.html");

                //リセットして変換
                session.Reset();
                Utils.SetResultFile(session, "reset-4.pdf");
                try
                {
                    session.Transcode("test.html");

                }
                catch (TranscoderException)
                {

                    // ignore
                }

                //再度変換
                Utils.SetResultFile(session, "reset-5.pdf");
                Utils.TranscodeFile(session, "files\\test.html", "text/html", null);
            }
        }
    }
}
