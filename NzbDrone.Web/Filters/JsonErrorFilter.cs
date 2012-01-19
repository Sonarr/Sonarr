using System.Linq;
using System.Web.Mvc;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Filters
{
    public class JsonErrorFilter : FilterAttribute, IExceptionFilter
    {
        private readonly string _errorTitle;

        public JsonErrorFilter(string errorTitle)
        {
            _errorTitle = errorTitle;
        }

        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = NotificationResult.Error(_errorTitle, filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
        }


    }
}