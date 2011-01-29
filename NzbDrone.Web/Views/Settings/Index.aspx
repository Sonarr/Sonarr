<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Web.Models.SettingsModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Settings
    
</asp:Content>

<asp:Content ID="Menu" ContentPlaceHolderID="ActionMenu" runat="server">
    <% Html.RenderPartial("SubMenu"); %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <% Html.RenderPartial(ViewData["viewName"].ToString(), Model); %>
</asp:Content>
