<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Web.Models.QualityModel>" %>
<%@ Import Namespace="NzbDrone.Core.Repository.Quality" %>

<script type="text/javascript">
    $(document).ready(function () {
        var options = {
            target: '#result',
            beforeSerialize: saveOrder,
            beforeSubmit: showRequest,
            success: showResponse,
            type: 'post',
            resetForm: false
        };
        $('#form').ajaxForm(options);
        $.simpledropdown("#dropdown1");

    });

    function saveOrder(jqForm, options) {
        //Save the order of the sortable

        var sortResult = $('#sortable').sortable("toArray");

        var firstPart = "Indexers_";
        var secondPart = "__Order";

        jQuery.each(sortResult, function (index, value) {
            var id = firstPart + value + secondPart;
            var newValue = index + 1;
            $("#" + id).val(newValue);
        });
    }

    function showRequest(formData, jqForm, options) {
        $("#result").empty().html('Saving...');
        $("#form :input").attr("disabled", true);
    }

    function showResponse(responseText, statusText, xhr, $form) {
        $("#result").empty().html(responseText);
        $("#form :input").attr("disabled", false);
    }  

    </script>

    <style type="text/css">
	#sortable { list-style-type: none; margin: 0; padding: 0; width: 30%; }
	#sortable li { margin: 0 5px 5px 5px; padding: 5px; font-size: 1.2em; height: 1.5em; }
	#sortable li { height: 1.5em; line-height: 1.2em; }
	.ui-state-highlight { height: 1.5em; line-height: 1.2em; }
	</style>

    <script type="text/javascript">
        $(function () {
            $("#sortable").sortable({
                placeholder: "ui-state-highlight"
            });
            $("#sortable").disableSelection();
        });
	</script>

    <% using (Html.BeginForm("SaveQuality", "Settings", FormMethod.Post, new { id = "form", name = "form" }))
       {%>
        <fieldset>
            <legend>Indexers</legend> 
                <%: Html.ValidationSummary(true, "Unable to save your settings. Please correct the errors and try again.") %>

                <div class="editor-label">
                    <%= Html.LabelFor(m => m.DefaultProfileId) %>
                </div>
                <div class="editor-field">
                    <%: Html.DropDownListFor(m => m.DefaultProfileId, Model.SelectList)%>
                    <%= Html.ValidationMessageFor(m => m.DefaultProfileId)%>
                </div>

                <div id="profile-editor">
                    <%= Html.ActionLink("Add a New Profile", "AddUserProfile", null, new { id = "addItem" }) %>
                    <div id="user-profiles">
                        <%for (int i = 0; i < Model.UserProfiles.Count(); i++){%>
                            <% Html.RenderPartial("UserProfileSection", Model.UserProfiles[i]); %>
                            <%--<%= Html.TextBoxFor(m => m.UserProfiles[i].ApiUrl, new { @style = "display:none" })%>--%>
                        <%}%>
                    </div>
                </div>
                <br />
                <input type="submit" class="button" value="Save" />
        </fieldset>

    <%}%>
<div id="result"></div>

<%--<script type="text/javascript">
    $('#add-profile').click(
        function () {
            <% Model.UserProfiles.Add(new QualityProfile{Name = "New Quality"}); %>
            $('<%: Html.LabelFor(m => m.UserProfiles.Last().Name) %><%: Html.TextBoxFor(m => m.UserProfiles.Last().Name) %><br/>')
            .prependTo($('#user-profiles'));
        });
    
</script>--%>

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
        $(this).parents("div.editorRow:first").remove();
        return false;
    });
</script>