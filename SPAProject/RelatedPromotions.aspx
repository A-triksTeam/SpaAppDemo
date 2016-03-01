<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RelatedPromotions.aspx.cs" Inherits="SitefinityWebApp.RelatedPromotions" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Button ID="relateBtn" Text="Relate first 2 promotions!" runat="server" />
        <br />
    
    <asp:Repeater ID="relatedPromotionsRptr" runat="server">
        <ItemTemplate>
            <asp:Literal id="ownerLbl" Text='<%#Eval("Title") %>' runat="server"></asp:Literal>
            <br />
            <asp:Literal id="Literal1" Text='<%#Eval("Owner") %>' runat="server"></asp:Literal>

        </ItemTemplate>
    </asp:Repeater>
    </div>
    </form>
</body>
</html>
