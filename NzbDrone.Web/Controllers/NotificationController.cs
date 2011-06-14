using System.Web.Mvc;
using MvcMiniProfiler;
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

            var basic = _notifications.BasicNotifications;

            if (basic.Count != 0)
            {
                message = basic[0].Title;

                if (basic[0].AutoDismiss)
                    _notifications.Dismiss(basic[0].Id);
            }

            else
            {
                if (_notifications.GetProgressNotifications.Count != 0)
                    message = _notifications.GetProgressNotifications[0].CurrentMessage;
            }


            if (MiniProfiler.Current.DurationMilliseconds < 100)
            {
                MiniProfiler.Stop(true);
            }

            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}