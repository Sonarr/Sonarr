using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace NzbDrone.Web.Helpers
{
    public static class LinkHelper
    {
        public static MvcHtmlString ImageActionLink(this AjaxHelper helper, string imageUrl, object imgAttributes, string actionName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
        {
            return ImageActionLink(helper, imageUrl, imgAttributes, actionName, null, routeValues, ajaxOptions, htmlAttributes);
        }

        public static MvcHtmlString ImageActionLink(this AjaxHelper helper, string imageUrl, object imgAttributes, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
        {
            var builder = new TagBuilder("img");
            builder.MergeAttribute("src", imageUrl);
            var imgAttributesDictionary = new RouteValueDictionary(imgAttributes);
            builder.MergeAttributes(imgAttributesDictionary);
            var link = helper.ActionLink("[replaceme]", actionName, controllerName, routeValues, ajaxOptions, htmlAttributes).ToHtmlString();
            return new MvcHtmlString(link.Replace("[replaceme]", builder.ToString(TagRenderMode.SelfClosing)));
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper helper, string imageUrl, object imgAttributes, string actionName, object routeValues, object htmlAttributes)
        {
            return ImageActionLink(helper, imageUrl, imgAttributes, actionName, null, routeValues, htmlAttributes);
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper helper, string imageUrl, object imgAttributes, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            var builder = new TagBuilder("img");
            builder.MergeAttribute("src", imageUrl);
            var imgAttributesDictionary = new RouteValueDictionary(imgAttributes);
            builder.MergeAttributes(imgAttributesDictionary);
            var link = helper.ActionLink("[replaceme]", actionName, controllerName, routeValues, htmlAttributes).ToHtmlString();
            return new MvcHtmlString(link.Replace("[replaceme]", builder.ToString(TagRenderMode.SelfClosing)));
        }
    }
}