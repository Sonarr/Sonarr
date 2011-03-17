<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<NzbDrone.Web.Models.SeriesSearchResultModel>>" %>
<%@ Import Namespace="NzbDrone.Core.Repository" %>

<div id="searchResults">
    <fieldset class="tvDbSearchResults">
        <legend>Search Results</legend>

        <% if (Model.Count == 0)
           { %>
               <b>No results found for the series name</b>
        <% }
        %>


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