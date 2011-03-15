<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<NzbDrone.Web.Models.SeriesSearchResultModel>>" %>
<%@ Import Namespace="NzbDrone.Core.Repository" %>

<div id="searchResults">
    <fieldset>
        <legend>Search Results</legend>

        <% int r = 0; %>
        <% foreach (var result in Model)
           { %>
                <%: Html.RadioButton("selectedSeries", result.TvDbId, new { @class="searchRadio examplePart", id="searchRadio_" + r }) %>
                <b><%: Html.Label(result.TvDbName) %></b> (<%: Html.Label(result.FirstAired.ToString("MM/dd/yyyy"))%>)

                <%: Html.TextBox(result.TvDbName + "_text", result.TvDbName, new { id = result.TvDbId + "_text", style="display:none" }) %>
                <% r++;%>
                <br />
        <%
           } %>
    </fieldset>
</div>

<div id="RootDirectories" style="display:none">
    <fieldset>
        <legend>Root TV Folders</legend>

        <% int d = 0; %>
        <% foreach (var dir in (List<RootDir>)ViewData["RootDirs"])
            { %>
                <%: Html.RadioButton("selectedRootDir", dir.Path, dir.Default, new { @class="dirList examplePart", id="dirRadio_" + d }) %>
                <%: Html.Label(dir.Path) %>
                <% if (dir.Default) { %> * <% } %>
                <% d++;%>
                <br />
        <% } %>
    </fieldset>

    <div id="example"></div>

    <button class="t.button" onclick="addSeries ()">Add New Series</button>
</div>

<div id="addResult"></div>

<script type="text/javascript">
    $(".searchRadio").live("change", function () {
        var checked = $(this).attr('checked');

        if (checked) {
            document.getElementById('RootDirectories').style.display = 'inline';
        }
    });

    function addSeries() {
        //Get the selected tvdbid + selected root folder
        //jquery bit below doesn't want to work...

        var checkedSeries = $("input[name='selectedSeries']:checked").val();
        //var checkedSeries = $('input.searchRadio:checked').val();
        //var checkedSeries = $('input:radio[name=selectedSeries]:checked').val();
        //var checkedSeries = $('input:radio[class=searchRadio]:checked').val();

        var checkedDir = $("input[name='selectedRootDir']:checked").val();

        var id = "#" + checkedSeries + "_text";
        var seriesName = $(id).val();

        $("#addResult").load('<%=Url.Action("AddNewSeries", "Series") %>', {
            dir: checkedDir,
            seriesId: checkedSeries,
            seriesName: seriesName
        });
    }


    //Need to figure out how to use 'ViewData["DirSep"]' instead of hardcoding '\'
    $(".examplePart").live("change", function () {
        var dir = $("input[name='selectedRootDir']:checked").val();
        var series = $("input[name='selectedSeries']:checked").val();

        var id = "#" + series + "_text";
        var seriesName = $(id).val();

        //var sep = '<%= ViewData["DirSep"] %>';
        var sep = "\\";

        var str = "Example: " + dir + sep + seriesName;

        $('#example').text(str);

    });
</script>