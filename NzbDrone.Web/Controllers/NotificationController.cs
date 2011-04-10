using System.Web.Mvc;
using NzbDrone.Core.Providers;

namespace NzbDrone.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationProvider _notifications;
        //
        // GET: /Notification/

        public NotificationController(NotificationProvider notificationProvider)
        {
            _notifications = notificationProvider;
        }

        [HttpGet]
        public JsonResult Index()
        {
            string message = string.Empty;
            if (_notifications.GetProgressNotifications.Count != 0)
            {
                message = _notifications.GetProgressNotifications[0].CurrentStatus;
            }

            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}