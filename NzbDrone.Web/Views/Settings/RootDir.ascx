<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Repository.RootDir>" %>
<%@ Import Namespace="NzbDrone.Web.Helpers" %>
<%
    using (Html.BeginCollectionItem("Directories"))
    {%>
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
    <fieldset>
        <div>
            <%:Html.TextBoxFor(m => m.Path, new {@class = "root_dir_text"})%>
            <a href="#" class="deleteRow">
                <img src="../Content/Images/X.png" alt="Delete" /></a>
        </div>
        <div>
            <%:Html.ValidationMessageFor(m => m.Path)%>
        </div>
        <div class="hiddenProfileDetails">
            <%=Html.TextBoxFor(x => x.Id, new {@style = "display:none"})%>
        </div>
    </fieldset>
</div>
<%
    }%>