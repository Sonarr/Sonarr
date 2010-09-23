<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<NzbDrone.Core.Repository.Series>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	SeriesView
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>SeriesView</h2>

    <table>
        <tr>
            <th></th>
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
                Overview
            </th>
            <th>
                AirTimes
            </th>
            <th>
                Language
            </th>
            <th>
                Path
            </th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
                <%: Html.ActionLink("Edit", "Edit", new { /* id=item.PrimaryKey */ }) %> |
                <%: Html.ActionLink("Details", "Details", new { /* id=item.PrimaryKey */ })%> |
                <%: Html.ActionLink("Delete", "Delete", new { /* id=item.PrimaryKey */ })%>
            </td>
            <td>
                <%: item.Id %>
            </td>
            <td>
                <%: item.SeriesName %>
            </td>
            <td>
                <%: item.Status %>
            </td>
            <td>
                <%: item.Overview %>
            </td>
            <td>
                <%: item.AirTimes %>
            </td>
            <td>
                <%: item.Language %>
            </td>
            <td>
                <%: item.Path %>
            </td>
        </tr>
    
    <% } %>

    </table>

    <p>
        <%: Html.ActionLink("Create New", "Create") %>
    </p>

</asp:Content>

