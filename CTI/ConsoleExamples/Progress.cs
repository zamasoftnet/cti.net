using System;
using Zamasoft.CTI;
using Zamasoft.CTI.Progress;

namespace examples
{
    class MyProgressListener : ProgressListener
    {
        private long sourceLength = -1;

        public void SourceLength(long sourceLength)
        {
            this.sourceLength = sourceLength;
        }

        public void Progress(long serverRead)
        {
            if (this.sourceLength == -1)
            {
                Console.WriteLine(serverRead);
            }
            else
            {
                Console.WriteLine(serverRead + "/" + this.sourceLength);
            }
        }
    }

    /// <summary>
    /// 変換の進行状況を表示します。
    /// </summary>
    class Progress
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // test.pdfに結果を出力する
                Utils.SetResultFile(session, "test.pdf");

                session.ProgressListener = new MyProgressListener();

                // リソースへのアクセスを許可する
                session.Property("input.include", "http://www.w3.org/**");

                // ウェブページを変換
                session.Transcode("http://www.w3.org/TR/xslt");
            }
        }
    }
}
