<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Core.Repository.Series>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<%@ Import Namespace="NzbDrone.Core.Repository" %>
<%@ Import Namespace="NzbDrone.Web.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%: Model.Title %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <fieldset>
        <div class="display-label">
            ID</div>
        <div class="display-field">
            <%: Model.SeriesId %></div>
        <div class="display-label">
            Overview</div>
        <div class="display-field">
            <%: Model.Overview %></div>
        <div class="display-label">
            Status</div>
        <div class="display-field">
            <%: Model.Status %></div>
        <div class="display-label">
            AirTimes</div>
        <div class="display-field">
            <%: Model.AirTimes %></div>
        <div class="display-label">
            Language</div>
        <div class="display-field">
            <%: Model.Language.ToUpper() %></div>
        <div class="display-label">
            Location</div>
        <div class="display-field">
            <%: Model.Path %></div>
    </fieldset>
    <% 
        //Todo: This breaks when using SQLServer... thoughts?
        //Normal Seasons
        foreach (var season in Model.Seasons.Where(s => s.SeasonNumber > 0))
        {
    %>
    <br />
    <h3>
        Season
        <%: season.SeasonNumber %></h3>
    <%
        Season season1 = season;
        Html.Telerik().Grid<EpisodeModel>().Name("seasons_" + season.SeasonNumber)
                          .Columns(columns =>
                          {
                              columns.Bound(c => c.SeasonNumber).Width(0).Title("Season");
                              columns.Bound(c => c.EpisodeNumber).Width(0).Title("Episode");
                              columns.Bound(c => c.Title).Title("Title");
                              columns.Bound(c => c.AirDate).Format("{0:d}").Width(0);
                          })
            //.DetailView(detailView => detailView.Template(e => Html.RenderPartial("EpisodeDetail", e)))
             .DetailView(detailView => detailView.ClientTemplate("<div><#= Overview #></div>"))
             .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.EpisodeNumber).Ascending()).Enabled(false))
                          .Footer(false)
                          .DataBinding(d => d.Ajax().Select("_AjaxSeasonGrid", "Series", new RouteValueDictionary { { "seasonId", season1.SeasonId.ToString() } }))
            //.EnableCustomBinding(true)
            //.ClientEvents(e => e.OnDetailViewExpand("episodeDetailExpanded")) //Causes issues displaying the episode detail multiple times...
            .Render();
        }

        //Specials
        var specialSeasons = Model.Seasons.Where(s => s.SeasonNumber == 0).FirstOrDefault();

        if (specialSeasons != null)
        {
    %>
    <br />
    <h3>
        Specials</h3>
    <%
            
Html.Telerik().Grid(specialSeasons.Episodes).Name("seasons_specials")
                 .Columns(columns =>
                 {
                     columns.Bound(c => c.EpisodeNumber).Width(0).Title("Episode");
                     columns.Bound(c => c.Title);
                     columns.Bound(c => c.AirDate).Format("{0:d}").Width(0);
                 })
                 .DetailView(detailView => detailView.ClientTemplate("<div><#= Overview #></div>"))
                 .Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.EpisodeNumber)).Enabled(false))
                 .Footer(false)
                 .Render();
        }
    %>
    <p>
        <%-- <%: Html.ActionLink("Edit", "Edit", new { /* id=Model.PrimaryKey */ }) %> |--%>
        <%: Html.ActionLink("Back to Series", "Index") %>
    </p>
</asp:Content>
<asp:Content ContentPlaceHolderID="Scripts" runat="server">
    <script type="text/javascript">

        function episodeDetailExpanded(e) {
            $console.log("OnDetailViewExpand :: " + e.masterRow.cells[1].innerHTML);
        }
   
    </script>
</asp:Content>
