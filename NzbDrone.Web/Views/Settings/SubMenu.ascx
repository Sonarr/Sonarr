<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>

    <%
        Html.Telerik().Menu().Name("Menu").Items(items => {items.Add().Text("General").Action("General", "Settings"); })
                                                .Items(items => items.Add().Text("Indexers").Action("Indexers", "Settings"))
                                                .Items(items => items.Add().Text("Downloads").Action("Downloads", "Settings"))
                                                .Render();
    %>

        