<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<NzbDrone.Core.Repository.Episode>" %>
<%@ Import Namespace="Telerik.Web.Mvc.UI" %>
<%: Model.Overview %>
<%:
     Html.Telerik().Grid(Model.Files)
            .Name("files_" + Model.EpisodeId)
            .Columns(columns =>
                         {
                             columns.Bound(c => c.Path);
                             columns.Bound(c => c.Quality);
                             columns.Bound(c => c.DateAdded);
                         })
            .Footer(false)
%>