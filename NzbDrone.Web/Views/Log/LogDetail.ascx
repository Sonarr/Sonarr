<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Instrumentation.Log>" %>
<ul>
    <li>
        <%: Model.Logger %>
    </li>
    <li>
        <%: Model.ExceptionType%>
    </li>
    <li>
        <%: Model.ExceptionMessage%>
    </li>
    <li>
        <%: Model.ExceptionString%>
    </li>
</ul>
