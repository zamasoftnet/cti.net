<%@ Page Language="C#" ContentType="application/pdf" AutoEventWireup="false" CodeBehind="REST.aspx.cs" Inherits="WebExamples.ServerInfo" EnableSessionState="False"%>
<%@ OutputCache Location="None" %><%-- 毎回更新するのでキャッシュをさせないようにする --%>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.Collections.Specialized" %>

<%
    Response.ClearContent();
    Response.ContentType = "application/pdf";
    
    string data = @"
<html xmlns:cssj='http://www.cssj.jp/ns/cssjml'>
<body>
C#からCopper PDFを使う。
</body>
</html>
            ";

    WebClient client = new WebClient();
    NameValueCollection par = new NameValueCollection();
    par.Add("rest.user", "user");
    par.Add("rest.password", "kappa");
    par.Add("rest.main", data);
    byte[] res = client.UploadValues("http://localhost:8097/transcode", par);
    Response.OutputStream.Write(res, 0, res.Length);
    client.Dispose();
%>

