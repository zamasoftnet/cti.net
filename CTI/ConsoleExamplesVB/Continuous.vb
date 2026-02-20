Imports System
Imports System.IO
Imports Zamasoft.CTI

''' <summary>
''' 複数の結果を結合したPDFを生成します。
''' </summary>
Module Continuous

    Sub Main()
        Using session As Session = DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
            ' エラーメッセージを標準エラー出力に表示する
            Utils.SetErrorMessageHander(session)

            ' test.pdfに結果を出力する
            Utils.SetResultFile(session, "test.pdf")

            ' 結果の結合を開始する
            session.Continuous = True

            ' リソースの送信
            Utils.SendResourceFile(session, "files\test.css", "text/css", Nothing)

            ' 文書の送信
            Utils.TranscodeFile(session, "files\test.html", "text/html", Nothing)

            ' 文書の送信
            Utils.TranscodeFile(session, "files\test.html", "text/html", Nothing)

            ' 結果の結合を完了する
            session.Join()
        End Using
    End Sub

End Module
