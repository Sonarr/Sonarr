<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Web.Models.QualityModel>" %>

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

    <% using (Html.BeginForm("SaveIndexers", "Settings", FormMethod.Post, new { id = "form", name = "form" }))
       {%>
<%: Html.ValidationSummary(true, "Unable to save your settings. Please correct the errors and try again.") %>
    
<div id="dropdown1">
	<ul>
		<li>Quality</li>
		<li>
			<ul>
				<li><a href="#">Argentina</a></li>
				<li><a href="#">Brazil</a></li>
				<li><a href="#">Uruguay</a></li>
				<li><a href="#">France</a></li>
				<li><a href="#">United Kingdon</a></li>
			</ul>
		</li>
	</ul>
</div>

<br />
<br />
<br />
<br />
<br />
<br />
<br />
<br />


    <%}%>
<div id="result"></div>