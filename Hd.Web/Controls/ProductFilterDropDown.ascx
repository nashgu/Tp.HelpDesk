<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductFilterDropDown.ascx.cs" Inherits="Hd.Web.Controls.ProductFilterDropDown" %>

<asp:Panel ID="pnlProductList" runat="server">
    <asp:DropDownList ID="lstProducts" runat="server"
        DataSourceID="productsSource" DataTextField="Name" DataValueField="Name"
        OnDataBound="lstProducts_DataBound" OnSelectedIndexChanged="lstProducts_SelectedIndexChanged"
        AutoPostBack="True" CssClass="dropDown">
    </asp:DropDownList>
</asp:Panel>

<tp:TpObjectDataSource ID="productsSource" runat="server" DataObjectTypeName="Hd.Portal.Request"
    SelectMethod="RetrieveProducts" TypeName="Hd.Portal.Request">
</tp:TpObjectDataSource>