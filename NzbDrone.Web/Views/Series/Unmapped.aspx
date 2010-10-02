<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<String>>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Unmapped
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        The following folders aren't currently mapped to any Series.</h2>
    <h3>
        Please Re-sync your folders. If the problem persists try renaming your folders to
        something more similar to the name of series they contain.</h3>
    <%


        Html.Telerik().Grid(Model)
       .Name("Grid")
       .Columns(columns => columns.Bound(o => o).Title("Folder"))
       .Sortable(sort => sort.OrderBy(order => order.Add(o => o).Ascending()).Enabled(false))
       .Render();
    %>
</asp:Content>
