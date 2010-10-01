<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<String>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Unmapped
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>        The following folders aren't currently mapped to any Series.</h2>
    <h3> Please Re-sync your folders. 
    If the problem persists try renaming your folders to something more similar to the name of series they contain.</h3>
    <table>
        <tr>
            <th>
                Folder
            </th>
        </tr>
        <% foreach (var item in Model)
           { %>
        <tr>
            <td>
                <%: item %>
            </td>
        </tr>
        <% } %>
    </table>
</asp:Content>
