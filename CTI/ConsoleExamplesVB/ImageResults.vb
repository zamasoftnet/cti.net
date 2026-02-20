Imports System
Imports System.IO
Imports Zamasoft.CTI
Imports Zamasoft.CTI.Result

''' <summary>
''' 複数のページを画像ファイルとして出力します。
''' </summary>
Module ImageResults

    Sub Main()
        Using session As Session = DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
            ' エラーメッセージを標準エラー出力に表示する
            Utils.SetErrorMessageHander(session)

            ' page-[0から始まるページ番号].jpgというファイル名のJPEG画像として結果を出力する
            session.Property("output.type", "image/jpeg")
            session.Results = New FileResults("page-", ".jpg")

            ' リソースの送信
            Utils.SendResourceFile(session, "files\test.css", "text/css", Nothing)

            ' 文書の送信
            Utils.TranscodeFile(session, "files\test.html", "text/html", Nothing)
        End Using
        Threading.Thread.Sleep(10000)
    End Sub

End Module
