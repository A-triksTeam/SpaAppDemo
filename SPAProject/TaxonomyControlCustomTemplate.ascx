<%@ Control Language="C#" %>
<%@ Register Assembly="Telerik.Sitefinity" Namespace="Telerik.Sitefinity.Web.UI" TagPrefix="sf" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
 
<sf:SitefinityLabel ID="titleLabel" runat="server" WrapperTagName="h2" HideIfNoText="true" CssClass="sftaxonTitle" />
<sf:ConditionalTemplateContainer ID="conditionalTemplate" runat="server">
    <Templates>
        <sf:ConditionalTemplate ID="ConditionalTemplate1" Left="RenderAs" Operator="Equal" Right="HorizontalList" runat="server">
            <asp:Repeater ID="repeater_horizontallist" runat="server">
                <HeaderTemplate>
                    <ul class="sftaxonHorizontalList">
                </HeaderTemplate>
                <ItemTemplate>
                    <li class="sftaxonItem">
                        <sf:SitefinityHyperLink ID="link" runat="server" CssClass="selectCommand sftaxonLink"></sf:SitefinityHyperLink></li>
                </ItemTemplate>
                <FooterTemplate></ul></FooterTemplate>
            </asp:Repeater>
        </sf:ConditionalTemplate>
        <sf:ConditionalTemplate ID="ConditionalTemplate2" Left="RenderAs" Operator="Equal" Right="VerticalList" runat="server">
            <asp:Repeater ID="repeater_verticallist" runat="server">
                <HeaderTemplate>
                    <ul class="sftaxonVerticalList">
                </HeaderTemplate>
                <ItemTemplate>
                    <li class="sftaxonItem">
                        <sf:SitefinityHyperLink ID="link" runat="server" CssClass="selectCommand sftaxonLink"></sf:SitefinityHyperLink>
                    </li>
                </ItemTemplate>
                <FooterTemplate>
                    </ul>
                </FooterTemplate>
            </asp:Repeater>
        </sf:ConditionalTemplate>

        <sf:ConditionalTemplate ID="ConditionalTemplate3" Left="RenderAs" Operator="Equal" Right="Cloud" runat="server">
            <asp:Repeater ID="repeater_cloud" runat="server">
                <HeaderTemplate>
                    <ul class="sftaxonCloud">
                </HeaderTemplate>
                <ItemTemplate>
                    <li class="sftaxonItem">
                        <%-- Note the sfCloudSize class.
                    The name can be different, just have to be last in the list.
                    The size factor is appendend to it - sfCloudSize1, sfCloudSize2,.. sfCloudSize6 --%>
                        <sf:SitefinityHyperLink ID="link" runat="server" CssClass="selectCommand sftaxonLink sfCloudSize"></sf:SitefinityHyperLink>
                    </li>
                </ItemTemplate>
                <FooterTemplate>
                    </ul>
                </FooterTemplate>
            </asp:Repeater>
        </sf:ConditionalTemplate>

        <sf:ConditionalTemplate ID="ConditionalTemplate4" Left="RenderAs" Operator="Equal" Right="HierarchicalList" runat="server">           
            <telerik:RadTreeView ID="treeview_hierarchicallist" runat="server" ShowLineImages="false" ExpandAnimation-Type="None" CollapseAnimation-Type="None">
            </telerik:RadTreeView>
        </sf:ConditionalTemplate>

        <sf:ConditionalTemplate ID="ConditionalTemplate5" Left="RenderAs" Operator="Equal" Right="Menu" runat="server">
            <telerik:RadMenu ID="taxa_menu" runat="server">
            </telerik:RadMenu>
        </sf:ConditionalTemplate>
    </Templates>
</sf:ConditionalTemplateContainer>

<sf:SitefinityHyperLink ID="SeeAllTaxaLink" runat="server" CssClass="selectCommand sftaxonsAll" Visible="false">All ({0})</sf:SitefinityHyperLink>
