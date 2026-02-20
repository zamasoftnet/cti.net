<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PDFTemplate.aspx.vb" Inherits="WebExamplesVB.PDFTemplate" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>カレンダー</title>
</head>
<body>
    <h1>ASP.NETでカレンダー(VB.NET 版)</h1>
    <form id="form1" runat="server">
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
      <p></p><%= DateTime.Now.ToString("yyyy年MM月dd日 hh時mm分ss秒")%></p>
    </form>
</body>
</html>
