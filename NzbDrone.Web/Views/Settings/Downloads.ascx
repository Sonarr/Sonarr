<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Web.Models.DownloadSettingsModel>" %>

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

    <script src="/Scripts/MicrosoftAjax.js" type="text/javascript"></script>
    <script src="/Scripts/MicrosoftMvcValidation.js" type="text/javascript"></script>
    <% Html.EnableClientValidation(); %>

<% using (Html.BeginForm("SaveDownloads", "Settings", FormMethod.Post, new { id = "form", name = "form" }))
       {%>
<%: Html.ValidationSummary(true, "Unable to save your settings. Please correct the errors and try again.") %>

    <fieldset>
        <legend>Download Settings</legend>
        <%--//Sync Frequency
        //Download Propers?
        //Retention
        //SAB Host/IP
        //SAB Port
        //SAB APIKey
        //SAB Username
        //SAB Password
        //SAB Category
        //SAB Priority--%>

            <fieldset class="sub-field">
                <legend>Usenet Variables</legend>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SyncFrequency) %></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.SyncFrequency)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SyncFrequency)%></div>
                </div>
                    
                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.DownloadPropers)%></div>
                        <div class="config-value"><%= Html.CheckBoxFor(m => m.DownloadPropers)%></div>
                    
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.DownloadPropers)%></div>
                </div>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.Retention)%></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.Retention)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.Retention)%></div>
                </div>
            </fieldset>

            <fieldset class="sub-field">
                <legend>SABnzbd</legend>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SabHost)%></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.SabHost)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SabHost)%></div>
                </div>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SabPort)%></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.SabPort)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SabPort)%></div>
                </div>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SabApiKey)%></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.SabApiKey)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SabApiKey)%></div>
                </div>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SabUsername)%></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.SabUsername)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SabUsername)%></div>
                </div>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SabPassword)%></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.SabPassword)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SabPassword)%></div>
                </div>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SabCategory)%></div>
                        <div class="config-value"><%= Html.TextBoxFor(m => m.SabCategory)%></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SabCategory)%></div>
                </div>

                <div class="config-section">
                    <div class="config-group">
                        <div class="config-title"><%= Html.LabelFor(m => m.SabPriority) %></div>
                        <div class="config-value"><%= Html.DropDownListFor(m => m.SabPriority, Model.PrioritySelectList) %></div>
                    </div>
                    <div class="config-validation"><%= Html.ValidationMessageFor(m => m.SabCategory)%></div>
                </div>
            </fieldset>

            <input type="submit" value="Save" class="submitButton"/>
    
    <% } %>
    </fieldset>
<div id="result"></div>