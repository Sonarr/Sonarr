using System.Linq;
using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace NzbDrone.Web.Helpers
{
    public static class IsCurrentActionHelper
    {
        private static bool IsCurrentController(HtmlHelper helper, string controllerName)
        {
            var currentControllerName = (string) helper.ViewContext.RouteData.Values["controller"];

            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }

        private static bool IsCurrentAction(HtmlHelper helper, string actionName, string controllerName)
        {
            var currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];
            var currentActionName = (string)helper.ViewContext.RouteData.Values["action"];

            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) &&
                    currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }

        public static string CurrentControllerLink(this HtmlHelper helper, string text, string actionName, string controllerName)
        {
            string result;

            if (IsCurrentController(helper, controllerName))
            {
                result = "<li class='current_page_item'>";
            }
            else
            {
                result = "<li>";
            }

            return result + helper.ActionLink(text, actionName, controllerName).ToHtmlString() + @"</li>";
        }

        public static string CurrentActionLink(this HtmlHelper helper, string text, string actionName, string controllerName)
        {
            string result;

            if (IsCurrentAction(helper, actionName, controllerName))
            {
                result = "<li class='current_action'>";
            }
            else
            {
                result = "<li>";
            }

            return result + helper.ActionLink(text, actionName, controllerName).ToHtmlString() + @"</li>";
        }
    }
}