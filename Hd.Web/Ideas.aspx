<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/main.master" CodeBehind="Ideas.aspx.cs" Inherits="Hd.Web.Ideas"%>

<%@ Register Src="~/Controls/RequestList.ascx" TagName="RequestList" TagPrefix="tp" %>
<%@ Register Src="~/Controls/ProductFilterDropDown.ascx" TagName="ProductFilterDropDown" TagPrefix="tp" %>
<%@ Import Namespace="Hd.Portal" %>

<asp:Content ID="Content2" ContentPlaceHolderID="phPageTitle" runat="server">Ideas</asp:Content>
<asp:Content ID="product" runat="server" ContentPlaceHolderID="phProduct">
    <tp:productfilterdropdown id="tpProductDropDown" runat="server" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="tabs" runat="server">
    <div id="tabs">
        <div class="tab" style="float: left" runat="server" id="div2">
            <a href="Default.aspx">Home</a>
        </div>
        <tp:requestscopetab id="RequestScopeTab1" scope="Private" cssclass="tab" runat="server" text="My Requests"
            url="MyRequests.aspx" />
        <tp:requestscopetab id="RequestScopeTab2" ispublic="True" scope="Global" cssclass="tab" runat="server" url="Issues.aspx"
            text="Issues" />
        <tp:requestscopetab id="RequestScopeTab4" cssclass="selectedTab" ispublic="True" scope="Global" runat="server" url="Ideas.aspx" text="Ideas" />
        <tp:requestscopetab id="RequestScopeTab3" scope="Private" cssclass="btn btn-warning btn-sm" runat="server" url="Request.aspx"
            text="Add Request" />
    </div>
</asp:Content>
<asp:Content ID="cnt" ContentPlaceHolderID="plcContent" runat="Server">
    <asp:UpdatePanel runat="server" ID="u">
        <ContentTemplate>
            <tp:gridcontroller id="controller" runat="server" gridid="ideasRequestList.requestListing" querytype="Hd.Portal.IdeasQuery"
                entitytype="Hd.Portal.Request, Hd.Portal" pagerid="pager" filters="Project.Name" />
            <tp:requestlist id="ideasRequestList" runat="server" />
            <div style="text-align: right; width: 97%; margin: 0 1%;">
                <tp:pager id="pager" runat="server" width="100%" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>