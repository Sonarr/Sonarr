<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Repository.Series>>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<%@ Import Namespace="NzbDrone.Core.Repository" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Series
</asp:Content>
<asp:Content ID="Menue" ContentPlaceHolderID="ActionMenue" runat="server">
    <%
        Html.Telerik().Menu().Name("telerikGrid").Items(items => { items.Add().Text("View Unmapped Folders").Action("Unmapped", "Series"); })
                                                .Items(items => items.Add().Text("Sync With Disk").Action("Sync", "Series"))
                                                .Render();
    %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%


        Html.Telerik().Grid(Model)
       .Name("Grid")
       .Columns(columns =>
       {
           columns.Bound(o => o.TvdbId).Width(100);
           columns.Template(c =>
                                   {
    %>
    <%:Html.ActionLink(c.Title, "Details", new {tvdbId =c.TvdbId}) %>
    <%
        }).Title("Title");
           columns.Bound(o => o.Status);
           columns.Bound(o => o.Path);
       })
       .Sortable(sort => sort.OrderBy(order => order.Add(o => o.Title).Ascending()).Enabled(false))
       .Render();
    %>
</asp:Content>
