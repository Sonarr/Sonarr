<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Instrumentation.Log>>" %>
<asp:Content ContentPlaceHolderID="Scripts" runat="server">
    <script type="text/javascript">
        function onRowDataBound(e) {

            e.row.style.boarder = "";

            if (e.dataItem.Level == 3) {
                e.row.style.backgroundColor = "#FFD700";
            }
            else if (e.dataItem.Level == 4) {
                e.row.style.backgroundColor = "#FF7500";
            }
            else if (e.dataItem.Level == 5) {
                e.row.style.backgroundColor = "black";
                e.row.style.color = "red";
            }
            //e.row.style.color = 'blue';
        }
    </script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Logs
</asp:Content>
<asp:Content ID="Menu" ContentPlaceHolderID="ActionMenu" runat="server">
    <%
        Html.Telerik().Menu().Name("logMenu").Items(items => items.Add().Text("Clear Logs").Action("Clear", "Log")).
            Render();
%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
        Html.Telerik().Grid(Model).Name("logs")
            .Columns(columns =>
                         {
                             columns.Bound(c => c.Time).Title("Time").Width(190);
                             columns.Bound(c => c.DisplayLevel).Title("Level").Width(0);
                             columns.Bound(c => c.Message);
                         })
            .DetailView(detailView => detailView.ClientTemplate(
                "<div><#= Logger #></div>" +
                "<div><#= ExceptionType #></div>" +
                "<div><#= ExceptionMessage #></div>" +
                "<div class='stackframe'><#= ExceptionString #></div>"
                                          )).DataBinding(data => data.Ajax().Select("_AjaxBinding", "Log"))
            .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.Time).Descending()).Enabled(true))
            .Pageable(
                c =>
                c.PageSize(50).Position(GridPagerPosition.Bottom).Style(GridPagerStyles.NextPrevious))
            .Filterable()
            .ClientEvents(c => c.OnRowDataBound("onRowDataBound"))
            .Render();
%>
</asp:Content>
