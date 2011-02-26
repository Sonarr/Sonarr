<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NzbDrone.Core.Repository.Series>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit

    <script type="text/javascript">
        $(document).ready(function () {
            var options = {
                target: '#result',
                beforeSubmit: showRequest,
                success: showResponse,
                type: 'post',
                resetForm: false
            };
            $('#form').ajaxForm(options);
            $('#save_button').attr('disabled', '');
        });

        function showRequest(formData, jqForm, options) {
            $("#result").empty().html('Saving Series...');
            $("#form :input").attr("disabled", true);
        }

        function showResponse(responseText, statusText, xhr, $form) {
            $("#result").empty().html(responseText);
            $("#form :input").attr("disabled", false);
        }                
</script>

    

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2><%: Html.DisplayTextFor(model => model.Title) %></h2>

    <% Html.EnableClientValidation(); %>
    <% using (Html.BeginForm("Edit", "Series", FormMethod.Post, new { id = "form", name = "form" }))
        { %>
        
        <fieldset>
            <legend>Edit</legend>
                     
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Path) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.Path) %>
                <%: Html.ValidationMessageFor(model => model.Path) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.Monitored) %>
            </div>
            <div class="editor-field">
                <%: Html.CheckBoxFor(model => model.Monitored) %>
                <%: Html.ValidationMessageFor(model => model.Monitored) %>
            </div>

            <div class="editor-label">
                <%: Html.LabelFor(model => model.SeasonFolder) %>
            </div>
            <div class="editor-field">
                <%: Html.CheckBoxFor(model => model.SeasonFolder)%>
                <%: Html.ValidationMessageFor(model => model.SeasonFolder)%>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.QualityProfileId) %>
            </div>
            <div class="editor-field">
                <%: Html.DropDownListFor(model => model.QualityProfileId, (SelectList)ViewData["SelectList"])%>
                <%: Html.ValidationMessageFor(model => model.QualityProfileId) %>
            </div>

            <div class="hidden" style="display:none;">
                <%: Html.TextBoxFor(model => model.SeriesId) %>
                <%: Html.TextBoxFor(model => model.Title) %>
                <%: Html.TextBoxFor(model => model.CleanTitle) %>
                <%: Html.TextBoxFor(model => model.Status) %>
                <%: Html.TextBoxFor(model => model.Overview) %>
                <%: Html.TextBoxFor(model => model.AirsDayOfWeek) %>
                <%: Html.TextBoxFor(model => model.AirTimes) %>
                <%: Html.TextBoxFor(model => model.Language) %>
            </div>
            
            <p>
                <input type="submit" id="save_button" value="Save" disabled="disabled" />
            </p>
        </fieldset>
    <% } %>

    <div>
        <%: Html.ActionLink("Back to Show", "Details", new { seriesId = Model.SeriesId }) %> | 
        <%: Html.ActionLink("Back to List", "Index") %>
    </div>

    <div id="result"></div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="headerContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ActionMenu" runat="server">
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>

