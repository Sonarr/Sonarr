<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>

<%@ Import Namespace="NLog" %>
<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    EPIC FAIL!!!
</asp:Content>
<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%:Model.Exception.Message%>
    </h2>
    <br />
    <%:Model.Exception.ToString()%>
</asp:Content>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            LogManager.GetLogger("EPICFAIL").FatalException("Unhandled Exception", Model.Exception);
        }
        catch (Exception)
        {
        }
    }
</script>
