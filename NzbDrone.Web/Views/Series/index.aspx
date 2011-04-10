<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Repository.Series>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Series
</asp:Content>
<asp:Content ID="Menu" ContentPlaceHolderID="ActionMenu" runat="server">
    <%
        Html.RenderPartial("SubMenu");
%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
        Html.Telerik().Grid(Model)
            .Name("Grid")
            .Columns(columns =>
                         {
                             columns.Template(c =>
                                                  {
%>
    <%:Html.ActionLink(c.Title ?? "New Series",
                                                                                        "Details",
                                                                                        new {seriesId = c.SeriesId})%>
    <%
                                                  }).Title("Title");
                             columns.Bound(o => o.Seasons.Count).Title("Seasons");
                             columns.Bound(o => o.QualityProfile.Name).Title("Quality");
                             columns.Bound(o => o.Status);
                             columns.Bound(o => o.AirsDayOfWeek);
                             columns.Bound(o => o.Path);
                         })
            .Sortable(sort => sort.OrderBy(order => order.Add(o => o.Title).Ascending()).Enabled(false))
            .Render();
%>
</asp:Content>
