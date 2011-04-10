<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="NzbDrone.Web.Controllers" %>
<%
    Html.Telerik().Menu().Name("telerikGrid").Items(items =>
                                                        {
                                                            items.Add().Text("Add Series")
                                                                .Items(
                                                                    subItem =>
                                                                    subItem.Add().Text("New Series").Action
                                                                        <AddSeriesController>(c => c.AddNew()))
                                                                .Items(
                                                                    subItem =>
                                                                    subItem.Add().Text("Existing Series").Action
                                                                        <AddSeriesController>(c => c.AddExisting()));

                                                            items.Add().Text("Start RSS Sync").Action<SeriesController>(
                                                                c => c.RssSync());
                                                            items.Add().Text("Rename All").Action<SeriesController>(
                                                                c => c.RenameAll());
                                                        }).Render();
%>