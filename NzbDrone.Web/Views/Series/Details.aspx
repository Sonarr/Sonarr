<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Core.Repository.Series>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Details
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Details</h2>

    <fieldset>
        <legend>Fields</legend>
        
        <div class="display-label">Id</div>
        <div class="display-field"><%: Model.TvdbId %></div>
        
        <div class="display-label">SeriesName</div>
        <div class="display-field"><%: Model.SeriesName %></div>
        
        <div class="display-label">Status</div>
        <div class="display-field"><%: Model.Status %></div>
        
        <div class="display-label">Overview</div>
        <div class="display-field"><%: Model.Overview %></div>
        
        <div class="display-label">AirTimes</div>
        <div class="display-field"><%: Model.AirTimes %></div>
        
        <div class="display-label">Language</div>
        <div class="display-field"><%: Model.Language %></div>
        
        <div class="display-label">Path</div>
        <div class="display-field"><%: Model.Path %></div>
        
        <div class="display-label">TvdbId</div>
        <div class="display-field"><%: Model.TvdbId %></div>
        
    </fieldset>
    <p>
       <%-- <%: Html.ActionLink("Edit", "Edit", new { /* id=Model.PrimaryKey */ }) %> |--%>
        <%: Html.ActionLink("Back to List", "Index") %>
    </p>

</asp:Content>

