<%@ Page Language="C#"  ContentType="application/xml" AutoEventWireup="false" CodeBehind="ServerInfo.aspx.cs" Inherits="WebExamples.ServerInfo" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Zamasoft.CTI" %>
<%@ Import Namespace="WebExamples" %>

<%
    // XML形式のサーバー情報を取得して表示します。
    using (Session session = CopperPDF.GetSession())
    {
        using (StreamReader info = new StreamReader(session.GetServerInfo("http://www.cssj.jp/ns/ctip/version")))
        {
            while (info.Peek() >= 0)
            {
                Response.Output.WriteLine(info.ReadLine());
            }
        }
    }
%>

