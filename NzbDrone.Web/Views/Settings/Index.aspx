<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Web.Models.SettingsModel>" %>
<%@ Import Namespace="NzbDrone.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Index
</asp:Content>
<asp:Content ID="General" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Settings</h2>
    <% using (Html.BeginForm())
       { %>
    <div>
        <fieldset>
            <legend>General</legend>
            <div class="editor-label">
                <%: Html.LabelFor(m => m.RootPath) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(m => m.RootPath) %>
            </div>
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>
    </div>
    <% } %>
</asp:Content>
