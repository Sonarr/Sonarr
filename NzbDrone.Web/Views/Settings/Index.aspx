<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Web.Models.SettingsModel>" %>

<%@ Import Namespace="NzbDrone.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Settings
</asp:Content>
<asp:Content ID="General" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm())
       { %>
    <%: Html.ValidationSummary(true, "Unable to save your settings. Please correct the errors and try again.") %>
    <div>
        <fieldset>
            <legend>General</legend>
            <div class="editor-label">
                <%: Html.LabelFor(m => m.TvFolder) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(m => m.TvFolder) %>
                <%: Html.ValidationMessageFor(m => m.TvFolder) %>
            </div>
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>
    </div>
    <% } %>
</asp:Content>
