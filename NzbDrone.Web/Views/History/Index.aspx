<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<NzbDrone.Web.Models.HistoryModel>>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<%@ Import Namespace="NzbDrone.Web.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Scripts" runat="server">
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
<asp:Content ID="Content2" ContentPlaceHolderID="TitleContent" runat="server">
    History
</asp:Content>
<asp:Content ID="Menu" ContentPlaceHolderID="ActionMenu" runat="server">
    <%
        Html.Telerik().Menu().Name("historyMenu").Items(items =>
                                                            {
                                                                items.Add().Text("Trim History").Action("Trim", "History");
                                                                items.Add().Text("Purge History").Action("Purge", "History");
                                                            }).Render();
    %>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <%Html.Telerik().Grid<HistoryModel>().Name("history")
                          .Columns(columns =>
                                       {
                                           columns.Bound(c => c.SeriesTitle).Title("Series Name").Width(120);
                                           columns.Bound(c => c.SeasonNumber).Title("Season #").Width(10);
                                           columns.Bound(c => c.EpisodeNumber).Title("Episode #").Width(10);
                                           columns.Bound(c => c.EpisodeTitle).Title("Episode Title").Width(140);
                                           columns.Bound(c => c.Quality).Title("Quality").Width(30);
                                           columns.Bound(c => c.Date).Title("Date Grabbed").Width(60);
                                       })
                          .DetailView(detailView => detailView.ClientTemplate(
                              "<fieldset>" +
                              "<div><b>Overview: </b><#= EpisodeOverview #></div>" +
                              "<div><b>NZB Title: </b><#= NzbTitle #></div>" +
                              "<div><b>Proper: </b><#= IsProper #></div>" +
                              "</fieldset>"
                              
                              ))
                          .DataBinding(data => data.Ajax().Select("_AjaxBinding", "History"))
                          .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.Date).Descending()).Enabled(true))
                          .Pageable(c => c.PageSize(20).Position(GridPagerPosition.Both).Style(GridPagerStyles.PageInput | GridPagerStyles.NextPreviousAndNumeric))
                          //.Filterable()
                          //.ClientEvents(c => c.OnRowDataBound("onRowDataBound"))
                          .Render();
    %>
</asp:Content>
