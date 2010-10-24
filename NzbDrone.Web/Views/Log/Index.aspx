<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Instrumentation.Log>>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Logs
</asp:Content>
<asp:Content ID="Menu" ContentPlaceHolderID="ActionMenu" runat="server">
    <%
        Html.Telerik().Menu().Name("logMenu").Items(items => items.Add().Text("Clear Logs").Action("Clear", "Log")).Render();
    %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%Html.Telerik().Grid(Model).Name("logs")
                          .Columns(columns =>
                                       {
                                           columns.Bound(c => c.Time).Title("Time").Width(190);
                                           //columns.Bound(c => c.Time).Title("Time").Template(c => c.Time.ToShortTimeString()).Groupable(false);
                                           columns.Bound(c => c.DisplayLevel).Title("Level").Width(0);
                                           columns.Bound(c => c.Message);
                                       })
                           .DataBinding(dataBinding => dataBinding.Ajax().Select("_AjaxBinding", "Log"))
          //.DetailView(detailView => detailView.Template(e => Html.RenderPartial("LogDetail", e)))
                          .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.Time).Descending()).Enabled(true))
                          .Pageable(c => c.PageSize(50).Position(GridPagerPosition.Both).Style(GridPagerStyles.PageInput | GridPagerStyles.NextPreviousAndNumeric))
          //.Groupable()
                          .Filterable()
          //.Groupable(grouping => grouping.Groups(groups => groups.Add(c => c.Time.Date)).Enabled(true))
                          .Render();
    %>
</asp:Content>

