Imports System
Imports System.IO
Imports System.Net
Imports Zamasoft.CTI
Imports Zamasoft.CTI.Source

''' <summary>
''' 文書から参照されているリソースを必要に応じて送ります。
''' </summary>
Module Resolver

    Sub Main()
        Using session As Session = DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
            ' test.pdfに結果を出力する
            Utils.SetResultFile(session, "test.pdf")

            session.SourceResolver = New MySourceResolver()

            ' 文書の送信
            Utils.TranscodeFile(session, "files\test.html", "text/html", Nothing)
        End Using
    End Sub

    Private Class MySourceResolver
        Implements SourceResolver

        Function Resolve(_uri As String, ByRef info As SourceInfo) As Stream Implements SourceResolver.Resolve
            Dim uri As New Uri(_uri)
            If uri.IsFile Then
                Dim file As String = uri.AbsolutePath
                If Not System.IO.File.Exists(file) Then
                    Return Nothing
                End If
                info = New SourceInfo(_uri)
                Return New FileStream(file, FileMode.Open, FileAccess.Read)
            ElseIf uri.Scheme = "http" Then
                Dim req As WebRequest = WebRequest.Create(uri)
                Dim resp As WebResponse = req.GetResponse()
                info = New SourceInfo(_uri)
                info.MimeType = resp.Headers.Get("Content-Type")
                Return resp.GetResponseStream()
            End If

            Return Nothing
        End Function
    End Class

End Module
