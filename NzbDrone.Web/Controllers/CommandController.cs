using System.Web.Mvc;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.DownloadClients;
using NzbDrone.Web.Filters;
using NzbDrone.Web.Models;
using System;

namespace NzbDrone.Web.Controllers
{
    public class CommandController : Controller
    {
        private readonly JobProvider _jobProvider;
        private readonly SabProvider _sabProvider;
        private readonly SmtpProvider _smtpProvider;
        private readonly TwitterProvider _twitterProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly GrowlProvider _growlProvider;

        public CommandController(JobProvider jobProvider, SabProvider sabProvider,
                                    SmtpProvider smtpProvider, TwitterProvider twitterProvider,
                                    EpisodeProvider episodeProvider, GrowlProvider growlProvider)
        {
            _jobProvider = jobProvider;
            _sabProvider = sabProvider;
            _smtpProvider = smtpProvider;
            _twitterProvider = twitterProvider;
            _episodeProvider = episodeProvider;
            _growlProvider = growlProvider;
        }

        public JsonResult RssSync()
        {
            _jobProvider.QueueJob(typeof(RssSyncJob));
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult BacklogSearch()
        {
            _jobProvider.QueueJob(typeof(BacklogSearchJob));
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult RecentBacklogSearch()
        {
            _jobProvider.QueueJob(typeof(RecentBacklogSearchJob));
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult ScanDisk(int seriesId)
        {
            _jobProvider.QueueJob(typeof(DiskScanJob), seriesId);
            return JsonNotificationResult.Info("Queued");
        }

        public JsonResult UpdateInfo(int seriesId)
        {
            _jobProvider.QueueJob(typeof(UpdateInfoJob), seriesId);
            return JsonNotificationResult.Info("Queued");
        }

        [HttpPost]
        [JsonErrorFilter]
        public JsonResult GetSabnzbdCategories(string host, int port, string apiKey, string username, string password)
        {
            return new JsonResult { Data = _sabProvider.GetCategories(host, port, apiKey, username, password) };
        }

        [HttpPost]
        public JsonResult SendTestEmail(string server, int port, bool ssl, string username, string password, string fromAddress, string toAddresses)
        {
            if (_smtpProvider.SendTestEmail(server, port, ssl, username, password, fromAddress, toAddresses))
                JsonNotificationResult.Info("Successfull", "Test email sent.");

            return JsonNotificationResult.Opps("Couldn't send Email, please check your settings");
        }

        public JsonResult GetTwitterAuthorization()
        {
            var result = _twitterProvider.GetAuthorization();

            if (result == null)
                JsonNotificationResult.Opps("Couldn't get Twitter Authorization");

            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult VerifyTwitterAuthorization(string token, string verifier)
        {
            var result = _twitterProvider.GetAndSaveAccessToken(token, verifier);

            if (!result)
                JsonNotificationResult.Opps("Couldn't verify Twitter Authorization");

            return JsonNotificationResult.Info("Good News!", "Successfully verified Twitter Authorization.");

        }

        public JsonResult RegisterGrowl(string host, string password)
        {
            try
            {
                var split = host.Split(':');
                var hostname = split[0];
                var port = Convert.ToInt32(split[1]);

                _growlProvider.Register(hostname, port, password);
                _growlProvider.TestNotification(hostname, port, password);

                return JsonNotificationResult.Info("Good News!", "Registered and tested growl successfully");
            }
            catch(Exception ex)
            {
                return JsonNotificationResult.Opps("Couldn't register and test Growl");
            }
        }

        [HttpPost]
        public EmptyResult SaveSeasonIgnore(int seriesId, int seasonNumber, bool ignored)
        {
            _episodeProvider.SetSeasonIgnore(seriesId, seasonNumber, ignored);
            return new EmptyResult();
        }

        [HttpPost]
        public EmptyResult SaveEpisodeIgnore(int episodeId, bool ignored)
        {
            _episodeProvider.SetEpisodeIgnore(episodeId, ignored);
            return new EmptyResult();
        }
    }
}
