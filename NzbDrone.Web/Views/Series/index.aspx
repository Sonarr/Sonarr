<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Entities.Series>>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>

<asp:Content ID="Content3" ContentPlaceHolderID="JavascriptContent" runat="server">
    $(document).ready(function () {
        $("#Mediabox").bind("click", MediaBoxClick);
        setTimeout('MediaDetect();', 5000);
    });
    var Discovered = false;

    function MediaDetect() {
        $.ajax({
            url: 'Series/MediaDetect',
            success: MediaDetectCallback
        });

    }
    function MediaDetectCallback(data) {
        Discovered=data.Discovered;
        if(!Discovered) 
            setTimeout('MediaDetect();', 10000);
        else 
            LightUpMedia(data);
    }

    function LightUpMedia(data) {
        $.ajax({
            url: 'Series/LightUpMedia',
            success: LightUpMediaSuccess
        });        
    }
    function LightUpMediaSuccess(data) {    
        $("#Mediabox").html(data.HTML);
    }
    function MediaBoxClick(args) {   
        $.ajax({
            url: 'Series/ControlMedia',
            data: "Action=" + args.target.className
        });        
    }
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Series
</asp:Content>
<asp:Content ID="Menue" ContentPlaceHolderID="ActionMenue" runat="server">
    <div id="Mediabox"></div>
    <%
        Html.Telerik().Menu().Name("telerikGrid").Items(items => { items.Add().Text("View Unmapped Folders").Action("Unmapped", "Series"); })
                                                .Items(items => items.Add().Text("Sync With Disk").Action("Sync", "Series"))
                                                .Render();
    %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%


        Html.Telerik().Grid(Model)
       .Name("Grid")
       .Columns(columns =>
       {
           columns.Bound(o => o.SeriesId).Width(100);
           columns.Template(c =>
                                   {
    %>
    <%:Html.ActionLink(c.Title, "Details", new {seriesId =c.SeriesId}) %>
    <%
        }).Title("Title");
           columns.Bound(o => o.Status);
           columns.Bound(o => o.Path);
       })
       .Sortable(sort => sort.OrderBy(order => order.Add(o => o.Title).Ascending()).Enabled(false))
       .Render();
    %>
</asp:Content>
