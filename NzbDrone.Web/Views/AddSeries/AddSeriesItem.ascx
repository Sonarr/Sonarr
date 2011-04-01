<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<SelectList>" %>
<div style="position: relative; padding: 10px" id="div_<%:ViewData["guid"] %>">
    <div style="position: relative; left: 0; width: 50%;">
        <%=ViewData["path"].ToString() %>
    </div>
    <div style="position: relative; right: 0; width: 50%;">
        <%
            Html.Telerik().ComboBox()
                  .Name(ViewData["guid"].ToString())
                // .AutoFill(true)
                  .BindTo(Model)
                // .DataBinding(b => b.Ajax().Select("TvDbLookup", "AddSeries"))
                .DataBinding(binding => binding.Ajax().Select("_textLookUp", "AddSeries").Delay(400).Cache(false))

                  .Filterable(f => f.FilterMode(AutoCompleteFilterMode.Contains))
                  .HighlightFirstMatch(true)
                  .HtmlAttributes(new { style = "width:70%; align:right" })
                  .SelectedIndex(0)
                  .Render();
        %>
        <button class="listButton" onclick="addSeries('<%:ViewData["guid"] %>','<%= ViewData["javaPath"].ToString()%>' )">
            Add</button>
    </div>
</div>
<script type="text/javascript" language="javascript">


    var addSeriesUrl = '<%= Url.Action("AddSeries", "AddSeries") %>';


    function addSeries(guid, path) {
        var qualityProfileId = $("#qualityProfileId").val();
        var comboBox = $("#" + guid).data("tComboBox");
        sendToServer(comboBox.value(), path, qualityProfileId);
        $("#div_" + guid).hide();
    }

    function sendToServer(id, path, quality) {
        $.ajax({
            type: "POST",
            url: addSeriesUrl,
            data: jQuery.param({ path: path, seriesId: id, qualityProfileId: quality }),
            error: function (req, status, error) {
                alert("Sorry! We could not add " + this.name + "at this time. " + error);
            }
        });
    }

</script>
