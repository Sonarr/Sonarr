<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Repository.RootDir>" %>
<%@ Import Namespace="NzbDrone.Web.Helpers" %>
<% using (Html.BeginCollectionItem("Directories"))
   { %>
<%
       var idClean = ViewData.TemplateInfo.HtmlFieldPrefix.Replace('[', '_').Replace(']', '_');
           //string sortable1 = String.Format("{0}_sortable1", idClean);
%>
<style type="text/css">
    .root_dir_text
    {
        width: 300px;
    }
</style>
<div class="rootDirSection">
    <fieldset style="width: 350px; height: 16px; margin: 0px; margin-top: 0px; border-color: #CCCCCD;
        -khtml-border-radius: 8px; border-radius: 8px; -moz-border-radius: 8px; -webkit-border-radius: 8px;">
        <div>
            <%: Html.TextBoxFor(m => m.Path, new { @class="root_dir_text" }) %>
            <a href="#" class="deleteRow">
                <img src="../Content/Images/X.png" alt="Delete" /></a>
        </div>
        <div>
            <%: Html.ValidationMessageFor(m => m.Path) %>
        </div>
        <div class="hiddenProfileDetails">
            <%= Html.TextBoxFor(x => x.Id, new { @style = "display:none" })%>
        </div>
    </fieldset>
</div>
<% } %>