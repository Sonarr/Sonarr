<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Core.Entities.Series>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
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
    <%= Html.Telerik().Grid(Model.Episodes)
        .Name("Episodes")
        .Columns(columns => 
        {
           columns.Bound(c => c.EpisodeNumber).Width(10);
           columns.Bound(c => c.Title);
           columns.Bound(c => c.AirDate).Format("{0:d}").Width(150);
        })
        .Groupable(grouping => grouping.Groups(groups => groups.Add(c => c.SeasonNumber)))
        .Sortable(rows=>rows
            .OrderBy(epSort => epSort.Add(c => c.EpisodeNumber)))
            
    %>
    <p>
        <%-- <%: Html.ActionLink("Edit", "Edit", new { /* id=Model.PrimaryKey */ }) %> |--%>
        <%: Html.ActionLink("Back to List", "Index") %>
        <%: Html.ActionLink("Load Episodes", "LoadEpisodes", new{seriesId= Model.SeriesId}) %>
    </p>
</asp:Content>
