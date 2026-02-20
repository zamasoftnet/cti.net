Imports System
Imports System.IO
Imports Zamasoft.CTI
Imports Zamasoft.CTI.Progress

''' <summary>
''' 変換の進行状況を表示します。
''' </summary>
Module Progress

    Sub Main()
        Using session As Session = DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
            ' test.pdfに結果を出力する
            Utils.SetResultFile(session, "test.pdf")

            session.ProgressListener = New MyProgressListener()

            ' リソースへのアクセスを許可する
            session.Property("input.include", "http://www.w3.org/**")

            ' ウェブページを変換
            session.Transcode("http://www.w3.org/TR/xslt")
        End Using
    End Sub

    Private Class MyProgressListener
        Implements ProgressListener
        Private _sourceLength As Long = -1

        Sub SourceLength(sourceLength As Long) Implements ProgressListener.SourceLength
            _sourceLength = sourceLength
        End Sub


        Sub Progress(serverRead As Long) Implements ProgressListener.Progress
            If _sourceLength = -1 Then
                Console.WriteLine(serverRead)
            Else
                Console.WriteLine(serverRead & "/" & _sourceLength)
            End If
        End Sub

    End Class

End Module

