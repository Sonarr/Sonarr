<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Repository.Series>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    SeriesView
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        SeriesView</h2>
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
                <%: Html.ActionLink(item.SeriesName, "Details", new { item.TvdbId })%>
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
    <p>
        <%: Html.ActionLink("Create New", "Create") %>
        <%: Html.ActionLink("Sync With Disk", "Sync") %>
    </p>
</asp:Content>
