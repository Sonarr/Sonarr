using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Providers;

namespace NzbDrone.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationProvider _notifications;
        //
        // GET: /Notification/

        public NotificationController(INotificationProvider notificationProvider)
        {
            _notifications = notificationProvider;
        }

        [HttpGet]
        public JsonResult Index()
        {
            return Json(_notifications.ProgressNotifications, JsonRequestBehavior.AllowGet);
        }

    }
}
