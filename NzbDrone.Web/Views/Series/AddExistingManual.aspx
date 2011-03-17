<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Web.Models.AddExistingManualModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Add Series Manually

    <script type="text/javascript">
        jQuery(document).ready(function () {
            $('#searchButton').click();
            $('#searchButton').attr('disabled', '');
        });
    </script>

</asp:Content>
<asp:Content ID="Menu" ContentPlaceHolderID="ActionMenu" runat="server">
    <%
        Html.RenderPartial("SubMenu");
    %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div>
        <h4><%= Html.Label(Model.Path) %></h4>
    </div>

    <%= Html.Label("Enter a Series Name") %>
    <%= Html.TextBoxFor(m => m.FolderName, new { id="existing_series_id" }) %>
    <%= Html.TextBoxFor(m => m.Path, new { id ="series_path", style="display:none" }) %>

    <p>
        <button class="t.button" id="searchButton" disabled="disabled" onclick="searchSeries ()">Search</button>
    </p>
    
    <div id="result"></div>

    <div id="addSeriesControls" style="display:none">
        <button class="t.button" onclick="addSeries ()">Add Series</button>
    </div>

    <div id="addResult"></div>

    <script type="text/javascript" language="javascript">

        $('#existing_series_id').bind('keydown', function (e) {
            if (e.keyCode == 13) {
                $('#searchButton').click();
            }
        });

        function searchSeries() {
            var seriesSearch = $('#existing_series_id');

            $("#result").text("Searching..."); //Tell the user that we're performing the search
            document.getElementById('addSeriesControls').style.display = 'none'; //Hide the add button
            $("#result").load('<%=Url.Action("SearchForSeries", "Series") %>', {
                seriesName: seriesSearch.val()
            });
        }

        $(".searchRadio").live("change", function () {
            var checked = $(this).attr('checked');

            if (checked) {
                document.getElementById('addSeriesControls').style.display = 'inline';
            }
        });

        function addSeries() {
            //Get the selected tvdbid + selected root folder
            //jquery bit below doesn't want to work...

            var checkedSeries = $("input[name='selectedSeries']:checked").val();

            var id = "#" + checkedSeries + "_text";
            var seriesName = $(id).val();

            var pathTest = $('#series_path').val();
            $('#tester').text(pathTest);

            $("#addResult").load('<%=Url.Action("AddExistingSeries", "Series") %>', {
                path: pathTest,
                seriesId: checkedSeries
            });
        }

    </script>
    <div id="tester"></div>

</asp:Content>
