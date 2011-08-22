using System;
using System.Diagnostics;
using System.Threading;
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
        public JsonResult Comet(string message)
        {
            MiniProfiler.Stop(true);

            var currentMessage = GetCurrentMessage();

            while (message == currentMessage)
            {
                Thread.Sleep(250);
                currentMessage = GetCurrentMessage();
            }

            return Json(currentMessage, JsonRequestBehavior.AllowGet);
        }

        private string GetCurrentMessage()
        {
            var notes = _notifications.ProgressNotifications;

            if (_notifications.ProgressNotifications.Count != 0)
                return _notifications.ProgressNotifications[0].CurrentMessage;


            return string.Empty;
        }
    }
}