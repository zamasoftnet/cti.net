<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SessionImage.aspx.cs" Inherits="WebExamples.SessionImage" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Import Namespace="System.Drawing.Imaging" %>
<%@ Import Namespace="System.Drawing.Drawing2D" %>
<%
    Bitmap bImage =
            new Bitmap(600, 200, PixelFormat.Format64bppPArgb);
    Graphics graph = Graphics.FromImage(bImage);
    graph.Clear(Color.White);

    if (Session["counter"] == null)
    {
        Session["counter"] = 1;
    }
    else
    {
        Session["counter"] = (int)Session["counter"] + 1;
    }
    graph.DrawString("■カウンタ：" + Session["counter"],
              new Font("ＭＳ明朝", 10),
              new SolidBrush(Color.Red), new PointF(0, 10));
    graph.DrawString("■リファラ：" + Request.UrlReferrer,
              new Font("ＭＳ明朝", 10),
              new SolidBrush(Color.Red), new PointF(0, 30));
                  
    Response.ContentType = "image/jpg";
    bImage.Save(Response.OutputStream, ImageFormat.Jpeg);
 %>
