using System;
using Zamasoft.CTI;

namespace examples
{
    /// <summary>
    /// サーバー側からインターネット上の文書にアクセスして変換します。
    /// </summary>
    class ServerResource
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // エラーメッセージを標準エラー出力に表示する
                Utils.SetErrorMessageHander(session);

                // test.pdfに結果を出力する
                Utils.SetResultFile(session, "test.pdf");

                // ハイパーリンクとブックマークを作成する
                session.Property("output.pdf.hyperlinks", "true");
                session.Property("output.pdf.bookmarks", "true");
                session.Property("output.pdf.compression", "none");

                // http://copper-pdf.com/以下にあるリソースへのアクセスを許可する
                session.Property("input.include", "http://copper-pdf.com/**");

                // ウェブページを変換
                session.Transcode("http://copper-pdf.com/");
            }
        }
    }
}
