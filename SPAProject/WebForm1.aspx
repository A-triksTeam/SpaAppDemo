<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="SitefinityWebApp.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Repeater ID="rep" runat="server">
        <ItemTemplate>
            <p><%# Eval("Title") %></p>
        </ItemTemplate>
    </asp:Repeater>
    </div>
    </form>
</body>
</html>
