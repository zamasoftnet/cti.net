<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PDFTemplate.aspx.cs" Inherits="WebExamples.PDFTemplate" %>

<%@ Register assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" namespace="System.Web.UI.DataVisualization.Charting" tagprefix="asp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>サンプル</title>
</head>
<body>
    <h1>ASP.NETサンプル(C# 版)</h1>
    <form id="form1" runat="server">
    <h2>カレンダー</h2>
     <asp:Calendar id="calendar1" runat="server">

           <OtherMonthDayStyle ForeColor="LightGray">
           </OtherMonthDayStyle>

           <TitleStyle BackColor="LightBlue"
                       ForeColor="White">
           </TitleStyle>

           <DayStyle BackColor="White">
           </DayStyle>

           <SelectedDayStyle BackColor="LightGray"
                             Font-Bold="True">
           </SelectedDayStyle>

      </asp:Calendar>
      <h2>時刻</h2>
      <p><%= DateTime.Now.ToString("yyyy年MM月dd日 hh時mm分ss秒")%></p>
      <h2>画像</h2>
      <p><img src="red.png" /></p>
      <h2>チャート</h2>
      <p>
      <asp:Chart ID="Chart1" runat="server"> 
  <Series> 
    <asp:Series Name="Series1" ChartType="Column"> 
      <Points> 
        <asp:DataPoint AxisLabel="Product A" YValues="345"/> 
        <asp:DataPoint AxisLabel="Product B" YValues="456"/> 
        <asp:DataPoint AxisLabel="Product C" YValues="125"/> 
        <asp:DataPoint AxisLabel="Product D" YValues="957"/>
      </Points> 
    </asp:Series> 
  </Series> 
  <ChartAreas> 
    <asp:ChartArea Name="ChartArea1"> 
      <AxisY IsLogarithmic="True" /> 
    </asp:ChartArea> 
  </ChartAreas> 
  <Legends> 
    <asp:Legend Name="Legend1" Title="Product Sales" /> 
  </Legends> 

</asp:Chart>
     </p>
     <h2>セッション</h2>
     <%        
        if (Session["counter"] == null)
        {
            Session["counter"] = 1;
        }
        else
        {
            Session["counter"] =  (int)Session["counter"] + 1;
        }
     %>
     <p>カウンタ：<%= Session["counter"] %></p>
     <p>Session ID：<%= Session.SessionID %></p>
     <h2>セッションに依存する画像</h2>
     <p><img src="SessionImage.aspx" /></p>
     </form>
</body>
</html>
