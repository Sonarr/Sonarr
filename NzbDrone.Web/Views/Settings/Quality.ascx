<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Web.Models.QualityModel>" %>
<%@ Import Namespace="NzbDrone.Core.Repository.Quality" %>
<%@ Import Namespace="NzbDrone.Web.Helpers" %>

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

    <% Html.EnableClientValidation(); %>

    <% using (Html.BeginForm("SaveQuality", "Settings", FormMethod.Post, new { id = "form", name = "form" }))
       {%>
        <fieldset>
            <legend>Quality</legend> 
                <%--<%: Html.ValidationSummary(true, "Unable to save your settings. Please correct the errors and try again.") %>--%>

                <div class="rightSide" style="float: right; width: 65%;">
                    <div id="defaultQualityDiv" style="float: left; margin: 30px;">
                        
                        <div class="config-group" style="width: 250px; margin-bottom: 5px; margin-left: 5px;">
                            <div class="config-title"><%= Html.LabelFor(m => m.DefaultProfileId)%></div>
                            <div class="config-value"><%: Html.DropDownListFor(m => m.DefaultProfileId, Model.SelectList)%></div>
                            <div class="config-validation"><%= Html.ValidationMessageFor(m => m.DefaultProfileId)%></div>
                        </div>
                    </div>
                </div>

                <div id="leftSide" style="width:35%;">
                    <div style="padding-top: 10px;">
                        <div style="padding-left: 7px; margin-bottom: 5px;">
                            <a id="addItem" style="text-decoration:none;" href="<%: Url.Action("AddUserProfile", "Settings") %>">
                            <img src="../../Content/Images/Plus.png" alt="Add New Profile" />
                            <h4 style="margin-left: 3px; display: inline; color: Black;">Add New Profile</h4></a>
                        </div>

                        <div id="user-profiles">
                            <%foreach (var item in Model.UserProfiles) { %>
                                <% Html.RenderPartial("UserProfileSection", item); %>
                            <% } %>
                        </div>
                    </div>

                    <div style="margin-top: 10px; padding-left: 5px;">
                        <input type="submit" id="save_button" value="Save" disabled="disabled" />
                    </div>
                </div>

        </fieldset>

    <%}%>
<div id="result"></div>

<script type="text/javascript">

    $("#addItem").click(function () {
        $.ajax({
            url: this.href,
            cache: false,
            success: function (html) { $("#user-profiles").prepend(html); }
        });
        return false;
    });

    $("a.deleteRow").live("click", function () {
        $(this).parents("div.userProfileSectionEditor:first").remove();
        return false;
    });
</script>