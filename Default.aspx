<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <article>
        <asp:Panel ID="defaultMainMenu" runat="server" CssClass="mainMenu">
        <asp:Repeater ID="rCenterMainMenu" runat="server" Visible="true"
            ItemType="WebApplication.Models.Menu" SelectMethod="GetMenu"
            OnItemDataBound="rCenterMainMenu_ItemDataBound">
            <HeaderTemplate>
                <ul>
            </HeaderTemplate>
            <ItemTemplate>
                <asp:Label ID="menuId" runat="server" Text='<%# Eval("ID") %>' Visible="false" />
                <asp:Label ID="pageId" runat="server" Text='<%# Eval("pageId") %>' Visible="false" />
                <asp:Literal ID="liMenuTopLiteral" runat="server" Text='<li>' />
                    <div class="mainPageTitle">
                        <asp:HyperLink ID="menuHyperLink" runat="server" />
                    </div>
                    <div class="mainPageIconImage">
                        <asp:Image ID="iconImage" runat="server" />
                    </div>
                    <div class="mainPageDescription">
                        <asp:Literal ID="descriptionLiteral" runat="server" />
                    </div>
                    <div class="pageImageDashedBorder">
                        <asp:Image ID="mainImage" runat="server" />
                    </div>
                <asp:Literal ID="liMenuBottomLiteral" runat="server" Text='</li>' />
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>
        </asp:Panel>
    </article>
</asp:Content>
