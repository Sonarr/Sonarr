<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<%@ Import Namespace="NzbDrone.Web.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Add Existing Series
</asp:Content>
<asp:Content ID="Menu" ContentPlaceHolderID="ActionMenu" runat="server">
    <%
        Html.RenderPartial("SubMenu");
    %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    //Get AJAX listing of unmapped directories

    <%
        Html.Telerik().Grid<AddExistingSeriesModel>().Name("Unmapped Series Folders")
                          .Columns(columns =>
                          {
                              columns.Bound(c => c.IsWanted).Width(0).Title("Is Wanted?");
                              columns.Bound(c => c.Path);
                          })
             //.DetailView(detailView => detailView.Template(e => Html.RenderPartial("EpisodeDetail", e)))
             //.DetailView(detailView => detailView.ClientTemplate("<div><#= Overview #></div>"))
             //.Sortable(rows => rows.OrderBy(epSort => epSort.Add(c => c.EpisodeNumber).Descending()).Enabled(true))
                          .Footer(false)
                          .DataBinding(d => d.Ajax().Select("_AjaxUnmappedFoldersGrid", "Series"))
            //.EnableCustomBinding(true)
            //.ClientEvents(e => e.OnDetailViewExpand("episodeDetailExpanded")) //Causes issues displaying the episode detail multiple times...
            .Render();
    %>`

</asp:Content>
