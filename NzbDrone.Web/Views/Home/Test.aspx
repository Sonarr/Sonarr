<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Web.Models.TestModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Test
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Test</h2>
    
    <% using (Html.BeginForm("Test", "Home", FormMethod.Post, new { id = "form", name = "form" }))
       {%>

    <div>One: <%=Html.RadioButtonFor(t => t.Number, 1)%></div>
    <div>Two: <%=Html.RadioButtonFor(t => t.Number, 2)%></div>
    <div>Three: <%=Html.RadioButtonFor(t => t.Number, 3)%></div>
    <div>Four: <%=Html.RadioButtonFor(t => t.Number, 4)%></div>

    <input type="submit" class="button" value="Save" />
    <% } %>
    
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="headerContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ActionMenu" runat="server">
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
