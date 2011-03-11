<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<%@ Import Namespace="Telerik.Web.Mvc.UI" %>

<% Html.Telerik().Menu().Name("telerikGrid").Items(items =>
   {
       items.Add().Text("Add Series").Action("Add", "Series");
       items.Add().Text("Start RSS Sync").Action("RssSync", "Series");
       items.Add().Text("Rename All").Action("RenameAll", "Series");
   }).Render();
%>