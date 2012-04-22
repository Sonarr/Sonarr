using System.Linq;
using System.Web.Mvc;

namespace NzbDrone.Services.Service.Controllers
{
    public class ExceptionController : Controller
    {

        [HttpPost]
        public EmptyResult ReportExisting()
        {
            return new EmptyResult();
        }

        [HttpPost]
        public JsonResult ReportNew()
        {
            return new JsonResult();
        }

 
    }
}