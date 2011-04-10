<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<String>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Add Existing Series
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
        if (Model.Count() == 0)
            Html.DisplayText("No Series to Add");
%>

    <%:Html.DropDownList("masterDropbox", (SelectList) ViewData["qualities"],
                                                new {style = "width: 100px;", id = "masterDropboxId"})%>

    <%:
                @Html.Telerik().DropDownList().Name("tester").BindTo((SelectList) ViewData["qualities"]).HtmlAttributes(
                    new {style = "width: 100px", @class = "qualityDropbox"})%>

    
        
    <%
        foreach (var path in Model)
        {
            Html.RenderAction("RenderPartial", "AddSeries", new {path});
        }

%>

    <script type="text/javascript">

        $("#masterDropboxId").change(function () {
            var selectedQuality = $('#masterDropboxId').get(0).selectedIndex;
            //$(".qualityDropbox").data("tComboBox").value(selectedQuality);
            //$(".qualityDropbox").data("tDropDownList").val(selectedQuality);

            var comboBox = $(".qualityDropbox").data("tDropDownList");
            comboBox.select(selectedQuality);

            
        });
        

    </script>

</asp:Content>