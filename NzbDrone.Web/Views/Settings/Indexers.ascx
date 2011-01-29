<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Web.Models.SettingsModel>" %>

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

<% using (Html.BeginForm("SaveIndexers", "Settings", FormMethod.Post, new { id = "form", name = "form" }))
       {%>
<%: Html.ValidationSummary(true, "Unable to save your settings. Please correct the errors and try again.") %>

    <fieldset>
        <legend>Indexers</legend> 
            <ul id="sortable">
                <%for (int i = 0; i < Model.Indexers.Count(); i++){%>
                    <li class="ui-state-default" id="<%= i %>">
                    <%= Html.CheckBoxFor(c => c.Indexers[i].Enabled) %><%= Html.DisplayTextFor(c => c.Indexers[i].IndexerName) %></li>
                <%}%>
            </ul>

            <%for (int i = 0; i < Model.Indexers.Count(); i++){%>
                <%= Html.TextBoxFor(m => m.Indexers[i].IndexerName, new { @style = "display:none" })%>
                <%= Html.TextBoxFor(m => m.Indexers[i].Order, new { @style = "display:none" })%>
                <%= Html.TextBoxFor(m => m.Indexers[i].RssUrl, new { @style = "display:none" })%>
                <%= Html.TextBoxFor(m => m.Indexers[i].ApiUrl, new { @style = "display:none" })%>
            <%}%>

            <%--NZBMatrix--%>
            <div class="editor-label">
                <%= Html.LabelFor(m => m.NzbMatrixUsername) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(m => m.NzbMatrixUsername)%>
                <%= Html.ValidationMessageFor(m => m.NzbMatrixUsername)%>
            </div>

            <div class="editor-label">
                <%= Html.LabelFor(m => m.NzbMatrixApiKey) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(m => m.NzbMatrixApiKey)%>
                <%= Html.ValidationMessageFor(m => m.NzbMatrixApiKey)%>
            </div>

            <br />
            <%--NZBs.Org--%>
            <div class="editor-label">
                <%= Html.LabelFor(m => m.NzbsOrgUId) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(m => m.NzbsOrgUId)%>
                <%= Html.ValidationMessageFor(m => m.NzbsOrgUId)%>
            </div>
            
            <div class="editor-label">
                <%= Html.LabelFor(m => m.NzbsOrgHash) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(m => m.NzbsOrgHash)%>
                <%= Html.ValidationMessageFor(m => m.NzbsOrgHash)%>
            </div>

            <br />
            <%--NZBsrus--%>
            <div class="editor-label">
                <%= Html.LabelFor(m => m.NzbsrusUId) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(m => m.NzbsrusUId)%>
                <%= Html.ValidationMessageFor(m => m.NzbsrusUId)%>
            </div>

            <div class="editor-label">
                <%= Html.LabelFor(m => m.NzbsrusHash) %>
            </div>
            <div class="editor-field">
                <%= Html.TextBoxFor(m => m.NzbsrusHash)%>
                <%= Html.ValidationMessageFor(m => m.NzbsrusHash)%>
            </div>
        <br />
            <input type="submit" value="Save" />
    </fieldset>

<% } %>
<div id="result"></div>