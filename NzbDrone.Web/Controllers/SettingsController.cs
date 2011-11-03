using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SettingsController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly AutoConfigureProvider _autoConfigureProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly QualityTypeProvider _qualityTypeProvider;
        private readonly RootDirProvider _rootDirProvider;
        private readonly ConfigFileProvider _configFileProvider;

        public SettingsController(ConfigProvider configProvider, IndexerProvider indexerProvider,
                                  QualityProvider qualityProvider, AutoConfigureProvider autoConfigureProvider,
                                  SeriesProvider seriesProvider, ExternalNotificationProvider externalNotificationProvider,
                                  QualityTypeProvider qualityTypeProvider, RootDirProvider rootDirProvider,
                                  ConfigFileProvider configFileProvider)
        {
            _externalNotificationProvider = externalNotificationProvider;
            _qualityTypeProvider = qualityTypeProvider;
            _rootDirProvider = rootDirProvider;
            _configFileProvider = configFileProvider;
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
            _qualityProvider = qualityProvider;
            _autoConfigureProvider = autoConfigureProvider;
            _seriesProvider = seriesProvider;
        }

        public ActionResult Test()
        {
            return View();
        }

        public JsonResult TestResults(string q)
        {
            var results = new List<TvDbSearchResultModel>();
            results.Add(new TvDbSearchResultModel { Id = 1, Title = "30 Rock", FirstAired = DateTime.Today.ToShortDateString() });
            results.Add(new TvDbSearchResultModel { Id = 2, Title = "The Office", FirstAired = DateTime.Today.AddDays(-1).ToShortDateString() });

            return Json(results, JsonRequestBehavior.AllowGet );
        }

        public ActionResult Index()
        {
            return RedirectToAction("Indexers");
        }

        public ActionResult Indexers()
        {
            return View(new IndexerSettingsModel
                            {
                                NzbMatrixUsername = _configProvider.NzbMatrixUsername,
                                NzbMatrixApiKey = _configProvider.NzbMatrixApiKey,

                                NzbsrusUId = _configProvider.NzbsrusUId,
                                NzbsrusHash = _configProvider.NzbsrusHash,

                                NzbsOrgHash = _configProvider.NzbsOrgHash,
                                NzbsOrgUId = _configProvider.NzbsOrgUId,

                                NewzbinUsername = _configProvider.NewzbinUsername,
                                NewzbinPassword = _configProvider.NewzbinPassword,

                                NzbsOrgEnabled = _indexerProvider.GetSettings(typeof(NzbsOrg)).Enable,
                                NzbMatrixEnabled = _indexerProvider.GetSettings(typeof(NzbMatrix)).Enable,
                                NzbsRUsEnabled = _indexerProvider.GetSettings(typeof(NzbsRUs)).Enable,
                                NewzbinEnabled = _indexerProvider.GetSettings(typeof(Newzbin)).Enable
                            });
        }

        public ActionResult Sabnzbd()
        {
            var tvCategory = _configProvider.SabTvCategory;
            var tvCategorySelectList = new SelectList(new[] { tvCategory });

            var model = new SabnzbdSettingsModel
                            {
                                SabHost = _configProvider.SabHost,
                                SabPort = _configProvider.SabPort,
                                SabApiKey = _configProvider.SabApiKey,
                                SabUsername = _configProvider.SabUsername,
                                SabPassword = _configProvider.SabPassword,
                                SabTvCategory = tvCategory,
                                SabTvPriority = _configProvider.SabTvPriority,
                                SabDropDirectory = _configProvider.SabDropDirectory,
                                SabTvCategorySelectList = tvCategorySelectList
                            };

            return View(model);
        }

        public ActionResult Quality()
        {
            var qualityTypes = new List<QualityTypes>();

            foreach (QualityTypes qual in Enum.GetValues(typeof(QualityTypes)))
            {
                qualityTypes.Add(qual);
            }

            ViewData["Qualities"] = qualityTypes;

            var profiles = _qualityProvider.All().ToList();

            foreach (var qualityProfile in profiles)
            {
                qualityProfile.AllowedString = string.Join(",", qualityProfile.Allowed);
            }

            var defaultQualityQualityProfileId = Convert.ToInt32(_configProvider.DefaultQualityProfile);
            var qualityProfileSelectList = new SelectList(profiles, "QualityProfileId", "Name");
            var qualityTypesFromDb = _qualityTypeProvider.All();

            var model = new QualityModel
                            {
                                Profiles = profiles,
                                DefaultQualityProfileId = defaultQualityQualityProfileId,
                                QualityProfileSelectList = qualityProfileSelectList,
                                SdtvMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 1).MaxSize,
                                DvdMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 2).MaxSize,
                                HdtvMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 4).MaxSize,
                                WebdlMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 5).MaxSize,
                                Bluray720pMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 6).MaxSize,
                                Bluray1080pMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 7).MaxSize
                            };

            return View(model);
        }

        public ActionResult Notifications()
        {
            var model = new NotificationSettingsModel
                            {
                                XbmcEnabled = _externalNotificationProvider.GetSettings(typeof(Xbmc)).Enable,
                                XbmcNotifyOnGrab = _configProvider.XbmcNotifyOnGrab,
                                XbmcNotifyOnDownload = _configProvider.XbmcNotifyOnDownload,
                                XbmcUpdateLibrary = _configProvider.XbmcUpdateLibrary,
                                XbmcCleanLibrary = _configProvider.XbmcCleanLibrary,
                                XbmcHosts = _configProvider.XbmcHosts,
                                XbmcUsername = _configProvider.XbmcUsername,
                                XbmcPassword = _configProvider.XbmcPassword,
                                SmtpEnabled = _externalNotificationProvider.GetSettings(typeof(Smtp)).Enable,
                                SmtpNotifyOnGrab = _configProvider.SmtpNotifyOnGrab,
                                SmtpNotifyOnDownload = _configProvider.SmtpNotifyOnGrab,
                                SmtpServer = _configProvider.SmtpServer,
                                SmtpPort = _configProvider.SmtpPort,
                                SmtpUseSsl = _configProvider.SmtpUseSsl,
                                SmtpUsername = _configProvider.SmtpUsername,
                                SmtpPassword = _configProvider.SmtpPassword,
                                SmtpFromAddress = _configProvider.SmtpFromAddress,
                                SmtpToAddresses = _configProvider.SmtpToAddresses,
                                TwitterEnabled = _externalNotificationProvider.GetSettings(typeof(Twitter)).Enable,
                                TwitterNotifyOnGrab = _configProvider.TwitterNotifyOnGrab,
                                TwitterNotifyOnDownload = _configProvider.TwitterNotifyOnDownload,
                                GrowlEnabled = _externalNotificationProvider.GetSettings(typeof(Growl)).Enable,
                                GrowlNotifyOnGrab = _configProvider.GrowlNotifyOnGrab,
                                GrowlNotifyOnDownload = _configProvider.GrowlNotifyOnDownload,
                                GrowlHost = _configProvider.GrowlHost,
                                GrowlPassword = _configProvider.GrowlPassword,
                                ProwlEnabled = _externalNotificationProvider.GetSettings(typeof(Prowl)).Enable,
                                ProwlNotifyOnGrab = _configProvider.ProwlNotifyOnGrab,
                                ProwlNotifyOnDownload = _configProvider.ProwlNotifyOnDownload,
                                ProwlApiKeys = _configProvider.ProwlApiKeys,
                                ProwlPriority = _configProvider.ProwlPriority,
                                ProwlPrioritySelectList = GetProwlPrioritySelectList()
                            };

            return View(model);
        }

        public ActionResult EpisodeSorting()
        {
            var model = new EpisodeSortingModel();

            model.SeriesName = _configProvider.SortingIncludeSeriesName;
            model.EpisodeName = _configProvider.SortingIncludeEpisodeTitle;
            model.ReplaceSpaces = _configProvider.SortingReplaceSpaces;
            model.AppendQuality = _configProvider.SortingAppendQuality;
            model.SeasonFolders = _configProvider.UseSeasonFolder;
            model.SeasonFolderFormat = _configProvider.SortingSeasonFolderFormat;
            model.SeparatorStyle = _configProvider.SortingSeparatorStyle;
            model.NumberStyle = _configProvider.SortingNumberStyle;
            model.MultiEpisodeStyle = _configProvider.SortingMultiEpisodeStyle;

            model.SeparatorStyles = new SelectList(EpisodeSortingHelper.GetSeparatorStyles(), "Id", "Name");
            model.NumberStyles = new SelectList(EpisodeSortingHelper.GetNumberStyles(), "Id", "Name");
            model.MultiEpisodeStyles = new SelectList(EpisodeSortingHelper.GetMultiEpisodeStyles(), "Id", "Name");

            return View(model);
        }

        public ActionResult System()
        {
            var selectedAuthenticationType = _configFileProvider.AuthenticationType;
            var authenticationTypes = new List<AuthenticationType>();

            foreach (AuthenticationType authenticationType in Enum.GetValues(typeof(AuthenticationType)))
            {
                authenticationTypes.Add(authenticationType);
            }

            var authTypeSelectList = new SelectList(authenticationTypes, selectedAuthenticationType);

            var model = new SystemSettingsModel();
            model.Port = _configFileProvider.Port;
            model.LaunchBrowser = _configFileProvider.LaunchBrowser;
            model.AuthenticationType = selectedAuthenticationType;
            model.AuthTypeSelectList = authTypeSelectList;

            return View(model);
        }

        public ViewResult AddProfile()
        {
            var qualityTypes = new List<QualityTypes>();

            foreach (QualityTypes qual in Enum.GetValues(typeof(QualityTypes)))
            {
                qualityTypes.Add(qual);
            }

            ViewData["Qualities"] = qualityTypes;

            var qualityProfile = new QualityProfile
                                     {
                                         Name = "New Profile",
                                         Allowed = new List<QualityTypes> { QualityTypes.Unknown },
                                         Cutoff = QualityTypes.Unknown
                                     };

            var id = _qualityProvider.Add(qualityProfile);
            qualityProfile.QualityProfileId = id;
            qualityProfile.AllowedString = "Unknown";

            ViewData["ProfileId"] = id;

            return View("QualityProfileItem", qualityProfile);
        }

        public ActionResult GetQualityProfileView(QualityProfile profile)
        {
            var qualityTypes = new List<QualityTypes>();

            foreach (QualityTypes qual in Enum.GetValues(typeof(QualityTypes)))
            {
                qualityTypes.Add(qual);
            }

            ViewData["Qualities"] = qualityTypes;
            ViewData["ProfileId"] = profile.QualityProfileId;

            return PartialView("QualityProfileItem", profile);
        }

        public ActionResult SubMenu()
        {
            return PartialView();
        }

        public QualityModel GetUpdatedProfileList()
        {
            var profiles = _qualityProvider.All().ToList();
            var defaultQualityQualityProfileId =
                Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", profiles[0].QualityProfileId));
            var selectList = new SelectList(profiles, "QualityProfileId", "Name");

            return new QualityModel { DefaultQualityProfileId = defaultQualityQualityProfileId, QualityProfileSelectList = selectList };
        }

        public JsonResult DeleteQualityProfile(int profileId)
        {
            try
            {
                if (_seriesProvider.GetAllSeries().Where(s => s.QualityProfileId == profileId).Count() != 0)
                    return new JsonResult { Data = "Unable to delete Profile, it is still in use." };

                _qualityProvider.Delete(profileId);
            }

            catch (Exception)
            {
                return new JsonResult { Data = "failed" };
            }

            return new JsonResult { Data = "ok" };
        }

        public JsonResult AutoConfigureSab()
        {
            try
            {
                var info = _autoConfigureProvider.AutoConfigureSab();
                return Json(info, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                return new JsonResult { Data = "failed" };
            }
        }

        [HttpPost]
        public JsonResult SaveIndexers(IndexerSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                var nzbsOrgSettings = _indexerProvider.GetSettings(typeof(NzbsOrg));
                nzbsOrgSettings.Enable = data.NzbsOrgEnabled;
                _indexerProvider.SaveSettings(nzbsOrgSettings);

                var nzbMatrixSettings = _indexerProvider.GetSettings(typeof(NzbMatrix));
                nzbMatrixSettings.Enable = data.NzbMatrixEnabled;
                _indexerProvider.SaveSettings(nzbMatrixSettings);

                var nzbsRUsSettings = _indexerProvider.GetSettings(typeof(NzbsRUs));
                nzbsRUsSettings.Enable = data.NzbsRUsEnabled;
                _indexerProvider.SaveSettings(nzbsRUsSettings);

                var newzbinSettings = _indexerProvider.GetSettings(typeof(Newzbin));
                newzbinSettings.Enable = data.NewzbinEnabled;
                _indexerProvider.SaveSettings(newzbinSettings);

                _configProvider.NzbsOrgUId = data.NzbsOrgUId;
                _configProvider.NzbsOrgHash = data.NzbsOrgHash;

                _configProvider.NzbMatrixUsername = data.NzbMatrixUsername;
                _configProvider.NzbMatrixApiKey = data.NzbMatrixApiKey;

                _configProvider.NzbsrusUId = data.NzbsrusUId;
                _configProvider.NzbsrusHash = data.NzbsrusHash;

                _configProvider.NewzbinUsername = data.NewzbinUsername;
                _configProvider.NewzbinPassword = data.NewzbinPassword;

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        [HttpPost]
        public JsonResult SaveSabnzbd(SabnzbdSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                //Check to see if the TV Directory matches any RootDirs (Ignoring Case), if it does, return an error to the user
                //This prevents a user from finding a way to delete their entire TV Library
                var rootDirs = _rootDirProvider.GetAll();
                if (rootDirs.Any(r => r.Path.Equals(data.SabDropDirectory, StringComparison.InvariantCultureIgnoreCase)))
                    Json(new NotificationResult { Title = "Failed", Text = "Invalid TV Directory", NotificationType = NotificationType.Error });

                _configProvider.SabHost = data.SabHost;
                _configProvider.SabPort = data.SabPort;
                _configProvider.SabApiKey = data.SabApiKey;
                _configProvider.SabPassword = data.SabPassword;
                _configProvider.SabTvCategory = data.SabTvCategory;
                _configProvider.SabUsername = data.SabUsername;
                _configProvider.SabTvPriority = data.SabTvPriority;
                _configProvider.SabDropDirectory = data.SabDropDirectory;


                return GetSuccessResult();
            }

            return
                Json(new NotificationResult() { Title = "Failed", Text = "Invalid request data.", NotificationType = NotificationType.Error });
        }

        [HttpPost]
        public JsonResult SaveQuality(QualityModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.DefaultQualityProfile = data.DefaultQualityProfileId;

                //Saves only the Default Quality, skips User Profiles since none exist
                if (data.Profiles == null)
                    return GetSuccessResult();

                foreach (var profile in data.Profiles)
                {
                    Logger.Debug(String.Format("Updating Profile: {0}", profile));

                    profile.Allowed = new List<QualityTypes>();

                    //Remove the extra comma from the end
                    profile.AllowedString = profile.AllowedString.Trim(',');

                    foreach (var quality in profile.AllowedString.Split(','))
                    {
                        var qType = (QualityTypes)Enum.Parse(typeof(QualityTypes), quality);
                        profile.Allowed.Add(qType);
                    }

                    //If the Cutoff value selected is not in the allowed list then use the last allowed value, this should be validated on submit
                    if (!profile.Allowed.Contains(profile.Cutoff))
                        return GetInvalidModelResult();
                    //profile.Cutoff = profile.Allowed.Last();

                    _qualityProvider.Update(profile);
                }

                var qualityTypesFromDb = _qualityTypeProvider.All();

                qualityTypesFromDb.Single(q => q.QualityTypeId == 1).MaxSize = data.SdtvMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 2).MaxSize = data.DvdMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 4).MaxSize = data.HdtvMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 5).MaxSize = data.WebdlMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 6).MaxSize = data.Bluray720pMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 7).MaxSize = data.Bluray1080pMaxSize;

                _qualityTypeProvider.UpdateAll(qualityTypesFromDb);

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        [HttpPost]
        public JsonResult SaveNotifications(NotificationSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                //XBMC Enabled
                var xbmcSettings = _externalNotificationProvider.GetSettings(typeof(Xbmc));
                xbmcSettings.Enable = data.XbmcEnabled;
                _externalNotificationProvider.SaveSettings(xbmcSettings);

                _configProvider.XbmcNotifyOnGrab = data.XbmcNotifyOnGrab;
                _configProvider.XbmcNotifyOnDownload = data.XbmcNotifyOnDownload;
                _configProvider.XbmcUpdateLibrary = data.XbmcUpdateLibrary;
                _configProvider.XbmcCleanLibrary = data.XbmcCleanLibrary;
                _configProvider.XbmcHosts = data.XbmcHosts;
                _configProvider.XbmcUsername = data.XbmcUsername;
                _configProvider.XbmcPassword = data.XbmcPassword;

                //SMTP
                var smtpSettings = _externalNotificationProvider.GetSettings(typeof (Smtp));
                smtpSettings.Enable = data.SmtpEnabled;
                _externalNotificationProvider.SaveSettings(smtpSettings);

                _configProvider.SmtpNotifyOnGrab = data.SmtpNotifyOnGrab;
                _configProvider.SmtpNotifyOnDownload = data.SmtpNotifyOnDownload;
                _configProvider.SmtpServer = data.SmtpServer;
                _configProvider.SmtpPort = data.SmtpPort;
                _configProvider.SmtpUseSsl = data.SmtpUseSsl;
                _configProvider.SmtpUsername = data.SmtpUsername;
                _configProvider.SmtpPassword = data.SmtpPassword;
                _configProvider.SmtpFromAddress = data.SmtpFromAddress;
                _configProvider.SmtpToAddresses = data.SmtpToAddresses;

                //Twitter
                var twitterSettings = _externalNotificationProvider.GetSettings(typeof(Twitter));
                twitterSettings.Enable = data.TwitterEnabled;
                _externalNotificationProvider.SaveSettings(twitterSettings);

                _configProvider.TwitterNotifyOnGrab = data.TwitterNotifyOnGrab;
                _configProvider.TwitterNotifyOnDownload = data.TwitterNotifyOnDownload;

                //Growl
                var growlSettings = _externalNotificationProvider.GetSettings(typeof(Growl));
                growlSettings.Enable = data.GrowlEnabled;
                _externalNotificationProvider.SaveSettings(growlSettings);

                _configProvider.GrowlNotifyOnGrab = data.GrowlNotifyOnGrab;
                _configProvider.GrowlNotifyOnDownload = data.GrowlNotifyOnDownload;
                _configProvider.GrowlHost = data.GrowlHost;
                _configProvider.GrowlPassword = data.GrowlPassword;

                //Prowl
                var prowlSettings = _externalNotificationProvider.GetSettings(typeof(Prowl));
                prowlSettings.Enable = data.ProwlEnabled;
                _externalNotificationProvider.SaveSettings(prowlSettings);

                _configProvider.ProwlNotifyOnGrab = data.ProwlNotifyOnGrab;
                _configProvider.ProwlNotifyOnDownload = data.ProwlNotifyOnDownload;
                _configProvider.ProwlApiKeys = data.ProwlApiKeys;
                _configProvider.ProwlPriority = data.ProwlPriority;

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        [HttpPost]
        public JsonResult SaveEpisodeSorting(EpisodeSortingModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SortingIncludeSeriesName = data.SeriesName;
                _configProvider.SortingIncludeEpisodeTitle = data.EpisodeName;
                _configProvider.SortingReplaceSpaces = data.ReplaceSpaces;
                _configProvider.SortingAppendQuality = data.AppendQuality;
                _configProvider.UseSeasonFolder = data.SeasonFolders;
                _configProvider.SortingSeasonFolderFormat = data.SeasonFolderFormat;
                _configProvider.SortingSeparatorStyle = data.SeparatorStyle;
                _configProvider.SortingNumberStyle = data.NumberStyle;
                _configProvider.SortingMultiEpisodeStyle = data.MultiEpisodeStyle;

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        [HttpPost]
        public JsonResult SaveSystem(SystemSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                _configFileProvider.Port = data.Port;
                _configFileProvider.LaunchBrowser = data.LaunchBrowser;
                _configFileProvider.AuthenticationType = data.AuthenticationType;

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        private JsonResult GetSuccessResult()
        {
            return Json(new NotificationResult() { Title = "Settings Saved" });
        }

        private JsonResult GetInvalidModelResult()
        {
            return Json(new NotificationResult() { Title = "Unable to save setting", Text = "Invalid post data", NotificationType = NotificationType.Error });
        }

        private SelectList GetProwlPrioritySelectList()
        {
            var list = new List<ProwlPrioritySelectListModel>();
            list.Add(new ProwlPrioritySelectListModel{ Name = "Very Low", Value = -2 });
            list.Add(new ProwlPrioritySelectListModel { Name = "Moderate", Value = -1 });
            list.Add(new ProwlPrioritySelectListModel { Name = "Normal", Value = 0 });
            list.Add(new ProwlPrioritySelectListModel { Name = "High", Value = 1 });
            list.Add(new ProwlPrioritySelectListModel { Name = "Emergency", Value = 2 });

            return new SelectList(list, "Value", "Name");
        }
    }
}