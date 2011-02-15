<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Web.Models.SettingsModel>" %>

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
            $("#result").empty().html('Saving...');
            $("#form :input").attr("disabled", true);
        }

        function showResponse(responseText, statusText, xhr, $form) {
            $("#result").empty().html(responseText);
            $("#form :input").attr("disabled", false);
        }                        
    </script>    

<% using (Html.BeginForm("SaveGeneral", "Settings", FormMethod.Post, new { id = "form", name = "form" }))
   {%>
<%: Html.ValidationSummary(true, "Unable to save your settings. Please correct the errors and try again.") %>
<fieldset>
            <legend>General</legend>
            
            <div class="editor-label">
                <%= Html.LabelFor(model => model.TvFolder) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(model => model.TvFolder) %>
                <%= Html.ValidationMessageFor(model => model.TvFolder) %>
            </div>
                       
            <p>
                <input type="submit" id="save_button" value="Save" disabled="disabled" />
            </p>
        </fieldset>
<% } Html.EndForm();%>
<div id="result"></div>