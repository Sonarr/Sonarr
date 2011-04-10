<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<NzbDrone.Web.Models.SeriesSearchResultModel>>" %>
<div id="searchResults">
    <fieldset>
        <legend>Search Results</legend>
        <%
            if (Model.Count == 0)
            {%>
        <b>No results found for the series name</b>
        <%
            }
%>
        <%
            int r = 0;%>
        <%
            foreach (var result in Model)
            {%>
        <%:Html.RadioButton("selectedSeries", result.TvDbId, r == 0,
                                                   new {@class = "searchRadio examplePart", id = "searchRadio_" + r})%>
        <b>
            <%:result.TvDbName + " (" + result.FirstAired.ToShortDateString()%>)
            <%:Html.TextBox(result.TvDbName + "_text", result.TvDbName,
                                               new {id = result.TvDbId + "_text", style = "display:none"})%>
            <%

                r++;%>
            <br />
            <%
            }%>
    </fieldset>
</div>
