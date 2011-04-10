using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Helpers
{
    public static class IsCurrentActionHelper
    {
        private static bool IsCurrentController(HtmlHelper helper, string actionName, string controllerName)
        {
            var currentControllerName = (string) helper.ViewContext.RouteData.Values["controller"];

            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }

        public static string CurrentActionLink(this HtmlHelper helper, string text, string actionName,
                                               string controllerName)
        {
            string result;

            if (IsCurrentController(helper, actionName, controllerName))
            {
                result = "<li class='current_page_item'>";
            }
            else
            {
                result = "<li>";
            }

            return result + helper.ActionLink(text, actionName, controllerName).ToHtmlString() + @"</li>";
        }
    }
}