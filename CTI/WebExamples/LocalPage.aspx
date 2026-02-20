<%@ Page Language="C#" ContentType="application/pdf" AutoEventWireup="false" CodeBehind="LocalPage.aspx.cs" Inherits="WebExamples.ServerInfo"%>
<%@ OutputCache Location="None" %><%-- 毎回更新するのでキャッシュをさせないようにする --%>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Zamasoft.CTI" %>
<%@ Import Namespace="Zamasoft.CTI.Source" %>
<%@ Import Namespace="WebExamples" %>

<%
    using (Session session = CopperPDF.GetSession())
    {
        CopperPDF.SetResponse(session, Response);
        string template = Request.ApplicationPath + "PDFTemplate.aspx";
        using (StreamWriter writer = new StreamWriter(session.Transcode(new SourceInfo(Request.ApplicationPath))))
        {
            Server.Execute(template, writer);
        }
    }
%>

