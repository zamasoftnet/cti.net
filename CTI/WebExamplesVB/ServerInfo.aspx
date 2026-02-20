<%@ Page Language="vb" ContentType="application/xml" AutoEventWireup="false" CodeBehind="ServerInfo.aspx.vb" Inherits="WebExamplesVB.ServerInfo" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Zamasoft.CTI" %>
<%@ Import Namespace="WebExamplesVB" %>

<%
    ' XML形式のサーバー情報を取得して表示します。
    Using session As Session = CopperPDF.GetSession()
        Using info As New StreamReader(session.GetServerInfo("http://www.cssj.jp/ns/ctip/version"))
            While info.Peek() >= 0
                Response.Output.WriteLine(info.ReadLine())
            End While
        End Using
    End Using
        
%>