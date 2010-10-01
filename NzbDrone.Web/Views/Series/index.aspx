<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Repository.Series>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    SeriesView
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <ul id="menu">
        <li>
            <%: Html.ActionLink("Sync With Disk", "Sync") %></li>
        <li>
            <%: Html.ActionLink("View Unmapped Folders", "Unmapped") %></li>
    </ul>
    <h2>
        SeriesView</h2>
    <p>
    </p>
    <table>
        <tr>
            <th>
                Id
            </th>
            <th>
                SeriesName
            </th>
            <th>
                Status
            </th>
            <th>
                Path
            </th>
        </tr>
        <% foreach (var item in Model)
           { %>
        <tr>
            <%--   <td>
                         <%: Html.ActionLink("Details", "Details", new { item.TvdbId })%>
                |
                <%: Html.ActionLink("Delete", "Delete", new { item.TvdbId })%>
            </td>--%>
            <td>
                <%: item.TvdbId.ToString()%>
            </td>
            <td>
                <%: Html.ActionLink(item.Title, "Details", new { item.TvdbId })%>
            </td>
            <td>
                <%: item.Status %>
            </td>
            <td>
                <%: item.Path %>
            </td>
        </tr>
        <% } %>
    </table>
</asp:Content>
