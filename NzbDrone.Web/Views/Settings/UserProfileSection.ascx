<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Repository.Quality.QualityProfile>" %>

    <div class="editor-label">
        <%= Html.LabelFor(m => m.Name) %>
    </div>
    <div class="editor-field">
        <%: Html.TextBoxFor(m => m.Name)%>
        <%= Html.ValidationMessageFor(m => m.Name)%>
    </div>       