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
        private const string SETTINGS_SAVED = "Settings Saved.";
        private const string SETTINGS_FAILED = "Error Saving Settings, please fix any errors";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly RootDirProvider _rootDirProvider;
        private readonly AutoConfigureProvider _autoConfigureProvider;
        private readonly NotificationProvider _notificationProvider;
        private readonly DiskProvider _diskProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;

        public SettingsController(ConfigProvider configProvider, IndexerProvider indexerProvider,
                                  QualityProvider qualityProvider, RootDirProvider rootDirProvider,
                                  AutoConfigureProvider autoConfigureProvider, NotificationProvider notificationProvider,
                                  DiskProvider diskProvider, SeriesProvider seriesProvider,
                                  ExternalNotificationProvider externalNotificationProvider)
        {
            _externalNotificationProvider = externalNotificationProvider;
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
            _qualityProvider = qualityProvider;
            _rootDirProvider = rootDirProvider;
            _autoConfigureProvider = autoConfigureProvider;
            _notificationProvider = notificationProvider;
            _diskProvider = diskProvider;
            _seriesProvider = seriesProvider;
        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult TestPartial()
        {
            return View();
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
            var sabDropDir = _configProvider.SabDropDirectory;
            var selectList = new SelectList(new List<string> {sabDropDir}, sabDropDir);

            var model = new SabnzbdSettingsModel
                            {
                                SabHost = _configProvider.SabHost,
                                SabPort =_configProvider.SabPort,
                                SabApiKey = _configProvider.SabApiKey,
                                SabUsername = _configProvider.SabUsername,
                                SabPassword = _configProvider.SabPassword,
                                SabTvCategory = _configProvider.SabTvCategory,
                                SabTvPriority = _configProvider.SabTvPriority,
                                SabDropDirectory = sabDropDir,
                                SabDropDirectorySelectList = selectList
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

            var model = new QualityModel
                            {
                                Profiles = profiles,
                                DefaultQualityProfileId = defaultQualityQualityProfileId,
                                QualityProfileSelectList = qualityProfileSelectList
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
                                XbmcPassword = _configProvider.XbmcPassword
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
        public ActionResult SaveIndexers(IndexerSettingsModel data)
        {
            var progressNotification = new ProgressNotification("Settings");
            _notificationProvider.Register(progressNotification);

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

                progressNotification.CurrentMessage = SETTINGS_SAVED;
                progressNotification.Status = ProgressNotificationStatus.Completed;
                return Content(SETTINGS_SAVED);
            }

            progressNotification.CurrentMessage = SETTINGS_FAILED;
            progressNotification.Status = ProgressNotificationStatus.Completed;
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveSabnzbd(SabnzbdSettingsModel data)
        {
            var progressNotification = new ProgressNotification("Settings");
            _notificationProvider.Register(progressNotification);

            if (ModelState.IsValid)
            {
                _configProvider.SabHost = data.SabHost;
                _configProvider.SabPort = data.SabPort;
                _configProvider.SabApiKey = data.SabApiKey;
                _configProvider.SabPassword = data.SabPassword;
                _configProvider.SabTvCategory = data.SabTvCategory;
                _configProvider.SabUsername = data.SabUsername;
                _configProvider.SabTvPriority = data.SabTvPriority;
                _configProvider.SabDropDirectory = data.SabDropDirectory;

                progressNotification.CurrentMessage = SETTINGS_SAVED;
                progressNotification.Status = ProgressNotificationStatus.Completed;
                return Content(SETTINGS_SAVED);
            }

            progressNotification.CurrentMessage = SETTINGS_FAILED;
            progressNotification.Status = ProgressNotificationStatus.Completed;
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveQuality(QualityModel data)
        {
            var progressNotification = new ProgressNotification("Settings");
            _notificationProvider.Register(progressNotification);

            if (ModelState.IsValid)
            {
                _configProvider.DefaultQualityProfile = data.DefaultQualityProfileId;

                //Saves only the Default Quality, skips User Profiles since none exist
                if (data.Profiles == null)
                    return Content(SETTINGS_SAVED);

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
                        return Content("Error Saving Settings, please fix any errors");
                    //profile.Cutoff = profile.Allowed.Last();

                    _qualityProvider.Update(profile);
                }

                progressNotification.CurrentMessage = SETTINGS_SAVED;
                progressNotification.Status = ProgressNotificationStatus.Completed;
                return Content(SETTINGS_SAVED);
            }

            progressNotification.CurrentMessage = SETTINGS_FAILED;
            progressNotification.Status = ProgressNotificationStatus.Completed;
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveNotifications(NotificationSettingsModel data)
        {
            var progressNotification = new ProgressNotification("Settings");
            _notificationProvider.Register(progressNotification);

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

                progressNotification.CurrentMessage = SETTINGS_SAVED;
                progressNotification.Status = ProgressNotificationStatus.Completed;
                return Content(SETTINGS_SAVED);
            }

            progressNotification.CurrentMessage = SETTINGS_FAILED;
            progressNotification.Status = ProgressNotificationStatus.Completed;
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveEpisodeSorting(EpisodeSortingModel data)
        {
            var progressNotification = new ProgressNotification("Settings");
            _notificationProvider.Register(progressNotification);

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

                progressNotification.CurrentMessage = SETTINGS_SAVED;
                progressNotification.Status = ProgressNotificationStatus.Completed;
                return Content(SETTINGS_SAVED);
            }

            progressNotification.CurrentMessage = SETTINGS_FAILED;
            progressNotification.Status = ProgressNotificationStatus.Completed;
            return Content(SETTINGS_FAILED);
        }
    }
}