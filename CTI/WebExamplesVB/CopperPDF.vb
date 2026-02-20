Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports Zamasoft.CTI
Imports Zamasoft.CTI.Result

Public Class CopperPDF
    ' Copper PDFに接続する。
    Public Shared Function GetSession() As Session
        Return DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
    End Function

    ' 結果を直接ブラウザに返すように設定します。
    Public Shared Sub SetResponse(session As Session, response As HttpResponse)
        session.Results = New SingleResult(New ContentLengthSender(response))
    End Sub

    ' Content-Lengthヘッダを送信するためのビルダー。
    Private Class ContentLengthSender
        Inherits Builder
        Private ReadOnly response As HttpResponse

        Sub New(response As HttpResponse)
            MyBase.New(response.OutputStream)
            Me.response = response
        End Sub

        Overrides Sub Finish()
            response.ContentType = Info.MimeType
            response.AppendHeader("Content-Length", length.ToString())
            MyBase.Finish()
        End Sub

    End Class
End Class
