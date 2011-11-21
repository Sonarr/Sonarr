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
        private readonly NotificationProvider _notificationProvider;

        //
        // GET: /Notification/

        public NotificationController(NotificationProvider notificationProvider)
        {
            _notificationProvider = notificationProvider;
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
            var notification = _notificationProvider.GetCurrent();

            if (notification != null)
                return notification.CurrentMessage;

            return string.Empty;
        }
    }
}