<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<NzbDrone.Web.Models.SeriesSearchResultModel>>" %>
<%@ Import Namespace="NzbDrone.Core.Repository" %>

<div id="searchResults">
    <fieldset>
        <legend>Search Results</legend>
        <% foreach (var result in Model)
           { %>
                <%: Html.RadioButton("selectedSeries", result.TvDbId, new { @class="searchRadio examplePart", name = result.TvDbName }) %>
                <%: Html.Label(result.TvDbName) %> (<%: Html.Label(result.FirstAired.ToString()) %>)
                <br />
        <% } %>
    </fieldset>
</div>

<div id="RootDirectories" style="display:none">
    <fieldset>
        <legend>Root TV Folders</legend>
        <% foreach (var dir in (List<RootDir>)ViewData["RootDirs"])
            { %>
                <%: Html.RadioButton("selectedRootDir", dir.RootDirId, dir.Default, new { @class="dirList examplePart", name = dir.Path }) %>
                <%: Html.Label(dir.Path) %>
                <% if (dir.Default) { %>* <% } %>
                <br />
        <% } %>
    </fieldset>

    <div id="example"></div>

    <button class="t.button" onclick="addSeries ()">Add New Series</button>
</div>

<div id="tester"></div>

<script type="text/javascript">
    $(".searchRadio").live("change", function () {
        var checked = $(this).attr('checked');

        if (checked) {
            $('#tester').text(this.value);
            document.getElementById('RootDirectories').style.display = 'inline';
        }
    });

    function addSeries() {
        //Get the selected tvdbid + selected root folder
        //jquery bit below doesn't want to work...

        var checkedSeries = $('input:radio[name=selectedSeries]:checked').val();
        //$('input:radio[name=selectedSeries]:checked').val();

        $('#tester').text(checkedSeries.value);

        //$('#tester').text("Hello_jhasdajsd");
    }

//    $(".examplePart").live("change", function() {
//        var dir = $('.selectedRootDir:checked');

//        var show = $('.selectedSeries:checked');
//        var sep = "\\";
//        var str = "Example: " + dir.name + sep + show.name;

//        $('#example').text(str);
//    
//    });

</script>