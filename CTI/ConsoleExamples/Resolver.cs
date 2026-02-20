using System;
using System.IO;
using System.Net;
using Zamasoft.CTI;
using Zamasoft.CTI.Source;

namespace examples
{
    class MySourceResolver : SourceResolver
    {
        public Stream Resolve(string _uri, ref SourceInfo info)
        {
            Uri uri = new Uri(_uri);
            if (uri.IsFile)
            {
                string file = uri.AbsolutePath;
                if (!File.Exists(file))
                {
                    return null;
                }
                info = new SourceInfo(_uri);
                return new FileStream(file, FileMode.Open, FileAccess.Read);
            }
            else if (uri.Scheme == "http")
            {
                WebRequest req = WebRequest.Create(uri);
                WebResponse resp = req.GetResponse();
                info = new SourceInfo(_uri);
                info.MimeType = resp.Headers.Get("Content-Type");
                return resp.GetResponseStream();
            }
            return null;
        }
    }

    /// <summary>
    /// 文書から参照されているリソースを必要に応じて送ります。
    /// </summary>
    class Resolver
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // test.pdfに結果を出力する
                Utils.SetResultFile(session, "test.pdf");

                session.SourceResolver = new MySourceResolver();

                // 文書の送信
                Utils.TranscodeFile(session, "files\\test.html", "text/html", null);
            }
        }
    }
}
