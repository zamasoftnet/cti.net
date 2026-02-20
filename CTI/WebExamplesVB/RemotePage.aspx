<%@ Page Language="vb" ContentType="application/pdf" AutoEventWireup="false" CodeBehind="RemotePage.aspx.vb" Inherits="WebExamplesVB.RemotePage" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Zamasoft.CTI" %>
<%@ Import Namespace="WebExamplesVB" %>

<%
    ' インターネット上のページにアクセスしてPDFに変換し、ブラウザに送ります。
    Using session As Session = CopperPDF.GetSession()
        CopperPDF.SetResponse(session, Response)
        session.Property("input.include", "http://www.w3.org/**")
        session.Transcode("http://www.w3.org/TR/xslt")
    End Using
%>
