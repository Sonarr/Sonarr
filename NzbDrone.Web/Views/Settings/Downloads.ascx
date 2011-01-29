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

            <br />
               
            <div>
                <span><%= Html.LabelFor(m => m.SyncFrequency) %></span>
                <%= Html.TextBoxFor(m => m.SyncFrequency)%>
                <%= Html.ValidationMessageFor(m => m.SyncFrequency)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.DownloadPropers)%></span>
                <%= Html.CheckBoxFor(m => m.DownloadPropers)%>
                <%= Html.ValidationMessageFor(m => m.DownloadPropers)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.Rentention)%></span>
                <%= Html.TextBoxFor(m => m.Rentention)%>
                <%= Html.ValidationMessageFor(m => m.Rentention)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.SabHost)%></span>
                <%= Html.TextBoxFor(m => m.SabHost)%>
                <%= Html.ValidationMessageFor(m => m.SabHost)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.SabPort)%></span>
                <%= Html.TextBoxFor(m => m.SabPort)%>
                <%= Html.ValidationMessageFor(m => m.SabPort)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.SabApiKey)%></span>
                <%= Html.TextBoxFor(m => m.SabApiKey)%>
                <%= Html.ValidationMessageFor(m => m.SabApiKey)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.SabUsername)%></span>
                <%= Html.TextBoxFor(m => m.SabUsername)%>
                <%= Html.ValidationMessageFor(m => m.SabUsername)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.SabPassword)%></span>
                <%= Html.TextBoxFor(m => m.SabPassword)%>
                <%= Html.ValidationMessageFor(m => m.SabPassword)%>
            </div>

            <br />

            <div>
                <span><%= Html.LabelFor(m => m.SabCategory)%></span>
                <%= Html.TextBoxFor(m => m.SabCategory)%>
                <%= Html.ValidationMessageFor(m => m.SabCategory)%>
            </div>

            <%--<div class="editor-label">
                <%= Html.DropDownListFor(m => m.SabPriority) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(m => m.SabCategory)%>
                <%= Html.ValidationMessageFor(m => m.SabCategory)%>
            </div>--%>

        <br />
        <p>
            <input type="submit" value="Save" />
        </p>
    </fieldset>

<% } %>
<div id="result"></div>