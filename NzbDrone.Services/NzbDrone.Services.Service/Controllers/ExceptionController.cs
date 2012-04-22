using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common.Contract;

namespace NzbDrone.Services.Service.Controllers
{
    public class ExceptionController : Controller
    {

        [HttpPost]
        public EmptyResult ReportExisting(ExistingExceptionReport existingExceptionReport)
        {
            return new EmptyResult();
        }

        [HttpPost]
        public JsonResult ReportNew(ExceptionReport exceptionReport)
        {
            return new JsonResult();
        }

 
    }
}