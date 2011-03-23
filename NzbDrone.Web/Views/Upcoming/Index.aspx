<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<NzbDrone.Web.Models.UpcomingEpisodeModel>>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<%@ Import Namespace="NzbDrone.Web.Models" %>
<%@ Import Namespace="NzbDrone.Core.Repository" %>

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
    Upcoming
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
    <%Html.Telerik().Grid<UpcomingEpisodeModel>().Name("Yesterday").NoRecordsTemplate("No watched shows aired yesterday")
                          .Columns(columns =>
                                       {
                                           columns.Bound(c => c.SeriesName).Title("Series Name").Width(110);
                                           columns.Bound(c => c.SeasonNumber).Title("Season #").Width(40);
                                           columns.Bound(c => c.EpisodeNumber).Title("Episode #").Width(40);
                                           columns.Bound(c => c.Title).Title("Episode Title").Width(120);
                                           columns.Bound(c => c.AirDate).Title("Air Date").Width(0);
                                       })
                          .DetailView(detailView => detailView.ClientTemplate(
                              "<div><b>Overview: </b><#= Overview #></div>"
                              ))
                          .DataBinding(data => data.Ajax().Select("_AjaxBindingYesterday", "Upcoming"))
                          .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.AirDate).Ascending()).Enabled(true))
                          //.Pageable(c => c.PageSize(20).Position(GridPagerPosition.Both).Style(GridPagerStyles.PageInput | GridPagerStyles.NextPreviousAndNumeric))
                          //.Filterable()
                          //.ClientEvents(c => c.OnRowDataBound("onRowDataBound"))
                          .Render();
    %>

    <%Html.Telerik().Grid<UpcomingEpisodeModel>().Name("Today").NoRecordsTemplate("No watched shows airing today.")
                          .Columns(columns =>
                                       {
                                           columns.Bound(c => c.SeriesName).Title("Series Name").Width(110);
                                           columns.Bound(c => c.SeasonNumber).Title("Season #").Width(40);
                                           columns.Bound(c => c.EpisodeNumber).Title("Episode #").Width(40);
                                           columns.Bound(c => c.Title).Title("Episode Title").Width(120);
                                           columns.Bound(c => c.AirDate).Title("Air Date").Width(0);
                                       })
                          .DetailView(detailView => detailView.ClientTemplate(
                              "<div><b>Overview: </b><#= Overview #></div>"
                              ))
                          .DataBinding(data => data.Ajax().Select("_AjaxBindingToday", "Upcoming"))
                          .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.AirDate).Ascending()).Enabled(true))
                          //.Pageable(c => c.PageSize(20).Position(GridPagerPosition.Both).Style(GridPagerStyles.PageInput | GridPagerStyles.NextPreviousAndNumeric))
                          //.Filterable()
                          //.ClientEvents(c => c.OnRowDataBound("onRowDataBound"))
                          .Render();
    %>

    <%Html.Telerik().Grid<UpcomingEpisodeModel>().Name("Week").NoRecordsTemplate("No watched shows airing in the next week...")
                          .Columns(columns =>
                                       {
                                           columns.Bound(c => c.SeriesName).Title("Series Name").Width(110);
                                           columns.Bound(c => c.SeasonNumber).Title("Season #").Width(40);
                                           columns.Bound(c => c.EpisodeNumber).Title("Episode #").Width(40);
                                           columns.Bound(c => c.Title).Title("Episode Title").Width(120);
                                           columns.Bound(c => c.AirDate).Title("Air Date").Width(0);
                                       })
                          .DetailView(detailView => detailView.ClientTemplate(
                              "<div><b>Overview: </b><#= Overview #></div>"
                              ))
                          .DataBinding(data => data.Ajax().Select("_AjaxBindingWeek", "Upcoming"))
                          .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.AirDate).Ascending()).Enabled(true))
                          //.Pageable(c => c.PageSize(20).Position(GridPagerPosition.Both).Style(GridPagerStyles.PageInput | GridPagerStyles.NextPreviousAndNumeric))
                          //.Filterable()
                          //.ClientEvents(c => c.OnRowDataBound("onRowDataBound"))
                          .Render();
    %>
</asp:Content>
