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
                
            <div style="padding-top: 10px;">
                <div style="padding-left: 7px; margin-bottom: 5px;">
                    <a id="addItem" style="text-decoration:none;" href="<%: Url.Action("AddRootDir", "Settings") %>">
                    <img src="../../Content/Images/Plus.png" alt="Add New Profile" />
                    <h4 style="margin-left: 3px; display: inline; color: Black;">Add New Root Directory</h4></a>
                </div>

                <div id="root-dirs">
                    <%foreach (var item in Model.Directories) { %>
                        <% Html.RenderPartial("RootDir", item); %>
                    <% } %>
                </div>
            </div>

            <p>
                <input type="submit" id="save_button" value="Save" disabled="disabled" />
            </p>
        </fieldset>
<% } Html.EndForm();%>
<div id="result"></div>

<script type="text/javascript">

    $("#addItem").click(function () {
        $.ajax({
            url: this.href,
            cache: false,
            success: function (html) { $("#root-dirs").append(html); }
        });
        return false;
    });

    $("a.deleteRow").live("click", function () {
        $(this).parents("div.rootDirSection:first").remove();
        return false;
    });

    $(".defaultCheckbox").live("change", function () {
        var checked = $(this).attr('checked');

        if (checked) {
            var thisOne = this;
            $(".defaultCheckbox").attr('checked', false);
            $(this).attr('checked', true);
        }
        
        //Don't let the user uncheck a checkbox (Like a radio button)
        else {
            $(this).attr('checked', true);
        }
    });
</script>