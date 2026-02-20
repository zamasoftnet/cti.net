Imports System
Imports System.IO
Imports Zamasoft.CTI

''' <summary>
''' Copper PDF の情報を取得して表示します。
''' </summary>
Module ServerInfo

    Sub Main()
        Using session As Session = DriverManager.getSession(New Uri("ctip://localhost:8099/"), "user", "kappa")
            ' バージョン情報
            Using info As New StreamReader(session.GetServerInfo("http://www.cssj.jp/ns/ctip/version"))
                While info.Peek >= 0
                    Console.WriteLine(info.ReadLine)
                End While
            End Using
            ' サポートされる出力形式
            Using info As New StreamReader(session.GetServerInfo("http://www.cssj.jp/ns/ctip/output-types"))
                While info.Peek >= 0
                    Console.WriteLine(info.ReadLine)
                End While
            End Using
        End Using
    End Sub

End Module
