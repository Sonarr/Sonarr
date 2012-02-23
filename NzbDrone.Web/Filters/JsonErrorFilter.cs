using System.Linq;
using System.Web.Mvc;
using NLog;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Filters
{
    public class JsonErrorFilter : FilterAttribute, IExceptionFilter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = JsonNotificationResult.Oops(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
            logger.FatalException(filterContext.RequestContext.HttpContext.Request.RawUrl, filterContext.Exception);
        }


    }
}