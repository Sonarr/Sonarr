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
            document.getElementById('unmappedGrid').style.display = 'block';
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

        function Grid_onLoad(e) {
            $('.t-no-data').text("Loading...");
        };

    </script>
    <%= Html.Label("Quality Profile")%>
    <%: Html.DropDownList("qualityProfileId", (SelectList)ViewData["QualitySelectList"], ViewData["QualityProfileId"])%>
    <div id="unmappedGrid" style="display: none">
        <%
            Html.Telerik().Grid<AddExistingSeriesModel>().Name("Unmapped_Series_Folders")
                .TableHtmlAttributes(new { id = "UnmappedSeriesGrid" })
                .Columns(columns =>
                             {
                                 columns.Bound(c => c.IsWanted).ClientTemplate("<input type='checkbox' name='<#= Path #>' class='checkedSeries' value='<#= TvDbId #>' checked='true'/>")
                                     .Width(20).Title("<input id='mastercheckbox' type='checkbox' style='margin-left:5px'/>")
                                     .HtmlAttributes(new { style = "text-align:center" });

                                 columns.Bound(c => c.Path).ClientTemplate("<a href=" + Url.Action("AddExistingManual", "Series", new { path = "<#= PathEncoded #>" }) + "><#= Path #></a>")
                                     .Template(c =>
                                      { %>
        <%:Html.ActionLink(c.Path, "AddExistingManual", new { path = c.Path })%>
        <% }).Title("Path");
                                 columns.Bound(c => c.TvDbName);
                             })
                .DataBinding(d => d.Ajax().Select("_AjaxUnmappedFoldersGrid", "Series"))
                .ClientEvents(events => events.OnRowDataBound("Grid_onRowDataBound"))
                .ClientEvents(events => events.OnLoad("Grid_onLoad"))
                .Footer(false)
                .Render();
        %>
        <p>
            <button class="t.button" onclick="syncSelected ()">
                Sync Selected Series</button>
        </p>
    </div>
    <div id="result">
    </div>
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

            var qualityProfileId = $("#qualityProfileId").val();

            $checkedRecords.each(function () {
                $.ajax(
                            {
                                type: "POST",
                                url: "/Series/SyncSelectedSeries",
                                data: jQuery.param({ path: this.name, tvdbid: this.value, qualityProfileId: qualityProfileId }),
                                error: function (req, status, error) {
                                    alert("Sorry! We could not add " + this.name + "at this time");
                                }

                            }
                       );


            });

            $checkedRecords.each(function () {

                var id = "#row_" + this.value;
                $(this).attr("checked", false);
                $(id).hide();
            });


        }
    </script>
</asp:Content>
