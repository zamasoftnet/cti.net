using System;
using Zamasoft.CTI;

namespace examples
{
    /// <summary>
    /// 標準出力に結果を出力します。
    /// </summary>
    class OutputStdout
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // 標準出力に結果を出力する
                Utils.SetResultStream(session, Console.OpenStandardOutput());

                // リソースの送信
                Utils.SendResourceFile(session, "files\\test.css", "text/css", null);

                // 文書の送信
                Utils.TranscodeFile(session, "files\\test.html", "text/html", null);
            }
        }
    }
}
