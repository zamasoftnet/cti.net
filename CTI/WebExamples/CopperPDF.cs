using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zamasoft.CTI;
using Zamasoft.CTI.Result;

namespace WebExamples
{
    public class CopperPDF
    {
        // Copper PDFに接続する。
        static public Session GetSession()
        {
            return DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa");
        }

        // 結果を直接ブラウザに返すように設定します。
        static public void SetResponse(Session session, HttpResponse response)
        {
            session.Results = new SingleResult(new ContentLengthSender(response));
        }

        // Content-Lengthヘッダを送信するためのビルダー。
        private class ContentLengthSender : Builder
        {
            private readonly HttpResponse response;
            public ContentLengthSender(HttpResponse response)
                : base(response.OutputStream)
            {
                this.response = response;
            }

            public override void Finish()
            {
                this.response.ContentType = this.info.MimeType;
                response.AppendHeader("Content-Length", this.length.ToString());
                base.Finish();
            }
        }
    }
}