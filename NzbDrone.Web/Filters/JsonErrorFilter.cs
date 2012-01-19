using System.Linq;
using System.Web.Mvc;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Filters
{
    public class JsonErrorFilter : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = JsonNotificationResult.Opps(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
        }


    }
}