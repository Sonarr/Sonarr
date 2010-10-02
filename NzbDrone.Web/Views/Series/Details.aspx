<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Core.Repository.Series>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%: Model.Title %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <fieldset>
        
        <div class="display-label">ID</div>
        <div class="display-field"><%: Model.TvdbId %></div>
        
        <div class="display-label">Overview</div>
        <div class="display-field"><%: Model.Overview %></div>

        <div class="display-label">Status</div>
        <div class="display-field"><%: Model.Status %></div>
               
        <div class="display-label">AirTimes</div>
        <div class="display-field"><%: Model.AirTimes %></div>
        
        <div class="display-label">Language</div>
        <div class="display-field"><%: Model.Language.ToUpper() %></div>
        
        <div class="display-label">Location</div>
        <div class="display-field"><%: Model.Path %></div>
      
    </fieldset>
    <p>
       <%-- <%: Html.ActionLink("Edit", "Edit", new { /* id=Model.PrimaryKey */ }) %> |--%>
        <%: Html.ActionLink("Back to List", "Index") %>
    </p>

</asp:Content>

