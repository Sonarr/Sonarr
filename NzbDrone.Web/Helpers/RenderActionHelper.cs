using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace NzbDrone.Web.Helpers
{
    public static class RenderActionHelper
    {
        public static MvcHtmlString RenderAction(this AjaxHelper helper, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);

            var url = urlHelper.Action(actionName, controllerName, routeValues);

            var tagBuilder = new TagBuilder("div");
            if (ajaxOptions != null) tagBuilder.MergeAttributes<string, object>(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            tagBuilder.MergeAttribute("data-ajax-action-link", "true");
            tagBuilder.MergeAttribute("data-href", url);

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
    }
}