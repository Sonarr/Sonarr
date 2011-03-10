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

    <script type="text/javascript">
        $(document).ready(function () {
            $('#mastercheckbox').attr("checked", "checked");
        });

        function Grid_onRowDataBound(e) {
            //the DOM element (<tr>) representing the row which is being databound
            var row = e.row;
            //the data item - JavaScript object.
            var tvDbId = e.dataItem.TvDbId;

            $(row).attr('id', 'row_' + tvDbId);

            //var info = row.cells[1].text();
            //row.cells[1].innerHTML = '<strong>' + dataItem + '</strong>';
            //You can use the OnRowDataBound event to customize the way data is presented on the client-side
        };

    </script>
    
    //Get AJAX listing of unmapped directories
    //When getting unmapped, also do a quick lookup on TVDB to see which series we would map this to... Don't do the mapping though...
    //ITvDbProvider.GetSeries(string title);

    <%
        Html.Telerik().Grid<AddExistingSeriesModel>().Name("Unmapped_Series_Folders")
            .TableHtmlAttributes(new { id = "UnmappedSeriesGrid" })
            .Columns(columns =>
                         {
                             columns.Bound(c => c.IsWanted).ClientTemplate("<input type='checkbox' name='<#= Path #>' class='checkedSeries' value='<#= TvDbId #>' checked='true'/>")
                                 .Width(20).Title("<input id='mastercheckbox' type='checkbox' />")
                                 .HtmlAttributes(new { style = "text-align:center" });

                             columns.Bound(c => c.Path);
                             columns.Bound(c => c.TvDbName);
                         })
            .DataBinding(d => d.Ajax().Select("_AjaxUnmappedFoldersGrid", "Series"))
            .ClientEvents(events => events.OnRowDataBound("Grid_onRowDataBound"))
            .Footer(false)
            .Render();
    %>

    <p>
        <button class="t.button" onclick="syncSelected ()">Sync Selected Series</button>   
    </p>

    <div id="result"></div>
    <div id="tester"></div>

<script type="text/javascript" language="javascript">

        // MasterCheckBox functionality
        $('#mastercheckbox').click(function () {
            if ($(this).attr('checked')) {
                $('.checkedSeries').attr('checked', true);
            } else {
                $('.checkedSeries').attr('checked', false);
            }
        });

        //Unchecking a 'normal' checkbox should clear the mastercheckbox as well

        $(".checkedSeries").live("click", function () {
            var numChkBoxes = $('.checkedSeries').length;
            var numChkBoxesChecked = $('.checkedSeries:checked').length;

            if (numChkBoxes == numChkBoxesChecked & numChkBoxes > 0) {
                $('#mastercheckbox').attr('checked', true);
            }
            else {
                $('#mastercheckbox').attr('checked', false);
            }

        });

        //Sync for selected series
        function syncSelected() {

            var $checkedRecords = $('.checkedSeries:checked');

            if ($checkedRecords.length < 1) {
                alert("Check one or more series first");
                return;
            }

            $("#result").load('<%=Url.Action("SyncSelectedSeries", "Series") %>', {
                checkedRecords: $checkedRecords.map(function () { return jQuery.param({ path: this.name, tvdbid: this.value }) })
            }
            
            //this.window.location = '<%= Url.Action("Index", "Series") %>';

            );

            

            var grid = $('#UnmappedSeriesGrid').data('tGrid');
        }
</script>
</asp:Content>
