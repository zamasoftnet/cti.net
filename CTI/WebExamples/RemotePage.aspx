<%@ Page Language="C#" ContentType="application/pdf" AutoEventWireup="false" CodeBehind="RemotePage.aspx.cs" Inherits="WebExamples.ServerInfo" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Zamasoft.CTI" %>
<%@ Import Namespace="WebExamples" %>

<%
    // インターネット上のページにアクセスしてPDFに変換し、ブラウザに送ります。
    using (Session session = CopperPDF.GetSession())
    {
        CopperPDF.SetResponse(session, Response);
        session.Property("input.include", "http://www.w3.org/**");
        session.Transcode("http://www.w3.org/TR/xslt");
    }
%>

