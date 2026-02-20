Imports System
Imports System.IO
Imports Zamasoft.CTI

''' <summary>
''' １つのセッションで繰り返し変換します。
''' </summary>
Module Reset

    Sub Main()
        Using session As Session = DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
            ' エラーメッセージを標準エラー出力に表示する
            Utils.SetErrorMessageHander(session)

            ' test.pdfに結果を出力する
            Utils.SetResultFile(session, "reset-1.pdf")

            ' リソースの送信
            Utils.SendResourceStream(session, New FileStream("files\test.css", FileMode.Open, FileAccess.Read), "test.css", "text/css", Nothing)

            ' 文書の送信
            Utils.TranscodeStream(session, New FileStream("files\test.html", FileMode.Open, FileAccess.Read), "test.html", "text/html", Nothing)

            ' 事前に送って変換
            Utils.SetResultFile(session, "reset-2.pdf")
            Utils.SendResourceStream(session, New FileStream("files\test.html", FileMode.Open, FileAccess.Read), "test.html", "text/html", Nothing)
            session.Transcode("test.html")

            ' 同じ文書を変換
            Utils.SetResultFile(session, "reset-3.pdf")
            session.Transcode("test.html")

            ' リセットして変換
            session.Reset()
            Utils.SetResultFile(session, "reset-4.pdf")
            Try
                session.Transcode("test.html")
            Catch ex As TranscoderException
                ' ignore
            End Try

            ' 再度変換
            Utils.SetResultFile(session, "reset-5.pdf")
            Utils.TranscodeFile(session, "files\test.html", "text/html", Nothing)
        End Using
    End Sub

End Module
