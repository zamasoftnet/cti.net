using System;
using Zamasoft.CTI;
using Zamasoft.CTI.Result;

namespace examples
{
    /// <summary>
    /// 複数のページを画像ファイルとして出力します。
    /// </summary>
    class ImageReaults
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // エラーメッセージを標準エラー出力に表示する
                Utils.SetErrorMessageHander(session);

                // page-[0から始まるページ番号].jpgというファイル名のJPEG画像として結果を出力する
                session.Property("output.type", "image/jpeg");
                session.Results = new FileResults("page-", ".jpg");

                // リソースの送信
                Utils.SendResourceFile(session, "files\\test.css", "text/css", null);

                // 文書の送信
                Utils.TranscodeFile(session, "files\\test.html", "text/html", null);
            }
        }
    }
}
