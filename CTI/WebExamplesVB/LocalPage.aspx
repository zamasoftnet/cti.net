<%@ Page Language="vb" ContentType="application/pdf" AutoEventWireup="false" CodeBehind="LocalPage.aspx.vb" Inherits="WebExamplesVB.LocalPage" %>
<%@ OutputCache Location="None" %><%-- 毎回更新するのでキャッシュをさせないようにする --%>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Zamasoft.CTI" %>
<%@ Import Namespace="Zamasoft.CTI.Source" %>
<%@ Import Namespace="WebExamplesVB" %>

<%
    Using session As Session = CopperPDF.GetSession()
        CopperPDF.SetResponse(session, Response)
        Dim template As String = Request.ApplicationPath + "PDFTemplate.aspx"
        Using writer As New StreamWriter(session.Transcode(New SourceInfo(".")))
            Server.Execute(template, writer)
        End Using
    End Using
        
%>

