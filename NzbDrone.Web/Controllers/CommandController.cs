using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using NLog;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Twitter;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    public class CommandController : Controller
    {
        private readonly JobProvider _jobProvider;
        private readonly SabProvider _sabProvider;
        private readonly SmtpProvider _smtpProvider;
        private readonly TwitterProvider _twitterProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CommandController(JobProvider jobProvider, SabProvider sabProvider,
                                    SmtpProvider smtpProvider, TwitterProvider twitterProvider)
        {
            _jobProvider = jobProvider;
            _sabProvider = sabProvider;
            _smtpProvider = smtpProvider;
            _twitterProvider = twitterProvider;
        }

        public JsonResult RssSync()
        {
            _jobProvider.QueueJob(typeof(RssSyncJob));
            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult BacklogSearch()
        {
            _jobProvider.QueueJob(typeof(BacklogSearchJob));
            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult ScanDisk(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(DiskScanJob), seriesId);

            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult UpdateInfo(int seriesId)
        {
            //Syncs the episodes on disk for the specified series
            _jobProvider.QueueJob(typeof(UpdateInfoJob), seriesId);

            return new JsonResult { Data = "ok", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult GetSabnzbdCategories(string host, int port, string apiKey, string username, string password)
        {
            try
            {
                return new JsonResult {Data = _sabProvider.GetCategories(host, port, apiKey, username, password)};
            }

            catch (Exception ex)
            {
                Logger.Warn("Unable to get Categories from SABnzbd");
                Logger.DebugException(ex.Message, ex);
                return Json(new NotificationResult { Title = "Failed", Text = "Unable to get SABnzbd Categories", NotificationType = NotificationType.Error });
            }
        }

        [HttpPost]
        public JsonResult SendTestEmail(string server, int port, bool ssl, string username, string password, string fromAddress, string toAddresses)
        {
            if (_smtpProvider.SendTestEmail(server, port, ssl, username, password, fromAddress, toAddresses))
                return Json(new NotificationResult { Title = "Successfully sent test email." });

            return Json(new NotificationResult { Title = "Failed", Text = "Unable to send Email, please check your settings", NotificationType = NotificationType.Error });
        }

        public JsonResult GetTwitterAuthorization()
        {
            var result = _twitterProvider.GetAuthorization();

            if (result == null)
                return Json(new NotificationResult { Title = "Failed", Text = "Unable to get Twitter Authorization", NotificationType = NotificationType.Error }, JsonRequestBehavior.AllowGet);

            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult VerifyTwitterAuthorization(string token, string verifier)
        {
            var result = _twitterProvider.GetAndSaveAccessToken(token, verifier);

            if (!result)
                return Json(new NotificationResult { Title = "Failed", Text = "Unable to verify Twitter Authorization", NotificationType = NotificationType.Error }, JsonRequestBehavior.AllowGet);

            return Json(new NotificationResult { Title = "Successfully verified Twitter Authorization." }, JsonRequestBehavior.AllowGet);
        }
    }
}
