Imports System
Imports System.IO
Imports Zamasoft.CTI

''' <summary>
''' 標準出力に結果を出力します。
''' </summary>
Module OutputStdout

    Sub Main()
        Using session As Session = DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
            ' 標準出力に結果を出力する
            Utils.SetResultStream(session, Console.OpenStandardOutput())

            ' リソースの送信
            Utils.SendResourceFile(session, "files\test.css", "text/css", Nothing)

            ' 文書の送信
            Utils.TranscodeFile(session, "files\test.html", "text/html", Nothing)
        End Using
    End Sub

End Module
