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

        public SettingsController(ConfigProvider configProvider, IndexerProvider indexerProvider,
                                  QualityProvider qualityProvider, RootDirProvider rootDirProvider,
                                  AutoConfigureProvider autoConfigureProvider, NotificationProvider notificationProvider,
                                  DiskProvider diskProvider, SeriesProvider seriesProvider)
        {
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

        public ActionResult Index(string viewName)
        {
            if (viewName != null)
                ViewData["viewName"] = viewName;

            else
                return RedirectToAction("Indexers");

            return View("Index");
        }

        public ActionResult Indexers()
        {
            ViewData["viewName"] = "Indexers";

            return View("Index", new IndexerSettingsModel
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
            ViewData["viewName"] = "Sabnzbd";

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

            return View("Index", model);
        }

        public ActionResult Quality()
        {
            ViewData["viewName"] = "Quality";

            var qualityTypes = new List<QualityTypes>();

            foreach (QualityTypes qual in Enum.GetValues(typeof(QualityTypes)))
            {
                qualityTypes.Add(qual);
            }

            ViewData["Qualities"] = qualityTypes;

            var profiles = _qualityProvider.GetAllProfiles().ToList();
            var defaultQualityQualityProfileId = Convert.ToInt32(_configProvider.DefaultQualityProfile);
            var qualityProfileSelectList = new SelectList(profiles, "QualityProfileId", "Name");

            var model = new QualityModel
                            {
                                Profiles = profiles,
                                DefaultQualityProfileId = defaultQualityQualityProfileId,
                                QualityProfileSelectList = qualityProfileSelectList
                            };

            return View("Index", model);
        }

        public ActionResult Notifications()
        {
            ViewData["viewName"] = "Notifications";

            var model = new NotificationSettingsModel
                            {
                                XbmcEnabled = Convert.ToBoolean(_configProvider.GetValue("XbmcEnabled", false, true)),
                                XbmcNotifyOnGrab = Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnGrab", false, true)),
                                XbmcNotifyOnDownload = Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnDownload", false, true)),
                                XbmcNotifyOnRename = Convert.ToBoolean(_configProvider.GetValue("XbmcNotifyOnRename", false, true)),
                                XbmcNotificationImage = Convert.ToBoolean(_configProvider.GetValue("XbmcNotificationImage", false, true)),
                                XbmcDisplayTime = Convert.ToInt32(_configProvider.GetValue("XbmcDisplayTime", 3, true)),
                                XbmcUpdateOnDownload = Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnDownload ", false, true)),
                                XbmcUpdateOnRename = Convert.ToBoolean(_configProvider.GetValue("XbmcUpdateOnRename", false, true)),
                                XbmcFullUpdate = Convert.ToBoolean(_configProvider.GetValue("XbmcFullUpdate", false, true)),
                                XbmcCleanOnDownload = Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnDownload", false, true)),
                                XbmcCleanOnRename = Convert.ToBoolean(_configProvider.GetValue("XbmcCleanOnRename", false, true)),
                                XbmcHosts = _configProvider.GetValue("XbmcHosts", "localhost:80", true),
                                XbmcUsername = _configProvider.GetValue("XbmcUsername", String.Empty, true),
                                XbmcPassword = _configProvider.GetValue("XbmcPassword", String.Empty, true)
                            };

            return View("Index", model);
        }

        public ActionResult EpisodeSorting()
        {
            ViewData["viewName"] = "EpisodeSorting";

            var model = new EpisodeSortingModel();

            model.SeriesName = _configProvider.SeriesName;
            model.EpisodeName = _configProvider.EpisodeName;
            model.ReplaceSpaces = _configProvider.ReplaceSpaces;
            model.AppendQuality = _configProvider.AppendQuality;
            model.SeasonFolders = _configProvider.UseSeasonFolder;
            model.SeasonFolderFormat = _configProvider.SeasonFolderFormat;
            model.SeparatorStyle = _configProvider.SeparatorStyle;
            model.NumberStyle = _configProvider.NumberStyle;
            model.MultiEpisodeStyle = _configProvider.MultiEpisodeStyle;

            model.SeparatorStyles = new SelectList(EpisodeSortingHelper.GetSeparatorStyles(), "Id", "Name");
            model.NumberStyles = new SelectList(EpisodeSortingHelper.GetNumberStyles(), "Id", "Name");
            model.MultiEpisodeStyles = new SelectList(EpisodeSortingHelper.GetMultiEpisodeStyles(), "Id", "Name");

            return View("Index", model);
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
                                         Cutoff = QualityTypes.Unknown,
                                     };

            var id = _qualityProvider.Add(qualityProfile);
            qualityProfile.QualityProfileId = id;

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
            var profiles = _qualityProvider.GetAllProfiles().ToList();
            var defaultQualityQualityProfileId =
                Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", profiles[0].QualityProfileId, true));
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
        public ActionResult SaveGeneral(SettingsModel data)
        {
            var basicNotification = new BasicNotification();
            basicNotification.Type = BasicNotificationType.Info;
            basicNotification.AutoDismiss = true;

            try
            {
                foreach (var dir in data.Directories)
                {
                    _rootDirProvider.Update(dir);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Failed to save Root Dirs");
                Logger.DebugException(ex.Message, ex);

                basicNotification.Title = SETTINGS_FAILED;
                _notificationProvider.Register(basicNotification);
                return Content(SETTINGS_FAILED);
            }


            basicNotification.Title = SETTINGS_SAVED;
            _notificationProvider.Register(basicNotification);

            return Content(SETTINGS_SAVED);
        }

        [HttpPost]
        public ActionResult SaveIndexers(IndexerSettingsModel data)
        {
            var basicNotification = new BasicNotification();
            basicNotification.Type = BasicNotificationType.Info;
            basicNotification.AutoDismiss = true;

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

                basicNotification.Title = SETTINGS_SAVED;
                _notificationProvider.Register(basicNotification);
                return Content(SETTINGS_SAVED);
            }

            basicNotification.Title = SETTINGS_FAILED;
            _notificationProvider.Register(basicNotification);
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveSabnzbd(SabnzbdSettingsModel data)
        {
            var basicNotification = new BasicNotification();
            basicNotification.Type = BasicNotificationType.Info;
            basicNotification.AutoDismiss = true;

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

                basicNotification.Title = SETTINGS_SAVED;
                _notificationProvider.Register(basicNotification);
                return Content(SETTINGS_SAVED);
            }

            basicNotification.Title = SETTINGS_FAILED;
            _notificationProvider.Register(basicNotification);
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveQuality(QualityModel data)
        {
            var basicNotification = new BasicNotification();
            basicNotification.Type = BasicNotificationType.Info;
            basicNotification.AutoDismiss = true;

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
                basicNotification.Title = SETTINGS_SAVED;
                _notificationProvider.Register(basicNotification);
                return Content(SETTINGS_SAVED);
            }

            basicNotification.Title = SETTINGS_FAILED;
            _notificationProvider.Register(basicNotification);
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveNotifications(NotificationSettingsModel data)
        {
            var basicNotification = new BasicNotification();
            basicNotification.Type = BasicNotificationType.Info;
            basicNotification.AutoDismiss = true;

            if (ModelState.IsValid)
            {
                _configProvider.SetValue("XbmcEnabled", data.XbmcEnabled.ToString());
                _configProvider.SetValue("XbmcNotifyOnGrab", data.XbmcNotifyOnGrab.ToString());
                _configProvider.SetValue("XbmcNotifyOnDownload", data.XbmcNotifyOnDownload.ToString());
                _configProvider.SetValue("XbmcNotifyOnRename", data.XbmcNotifyOnRename.ToString());
                _configProvider.SetValue("XbmcNotificationImage", data.XbmcNotificationImage.ToString());
                _configProvider.SetValue("XbmcDisplayTime", data.XbmcDisplayTime.ToString());
                _configProvider.SetValue("XbmcUpdateOnDownload", data.XbmcUpdateOnDownload.ToString());
                _configProvider.SetValue("XbmcUpdateOnRename", data.XbmcUpdateOnRename.ToString());
                _configProvider.SetValue("XbmcFullUpdate", data.XbmcFullUpdate.ToString());
                _configProvider.SetValue("XbmcCleanOnDownload", data.XbmcCleanOnDownload.ToString());
                _configProvider.SetValue("XbmcCleanOnRename", data.XbmcCleanOnRename.ToString());
                _configProvider.SetValue("XbmcHosts", data.XbmcHosts);
                _configProvider.SetValue("XbmcUsername", data.XbmcUsername);
                _configProvider.SetValue("XbmcPassword", data.XbmcPassword);

                basicNotification.Title = SETTINGS_SAVED;
                _notificationProvider.Register(basicNotification);
                return Content(SETTINGS_SAVED);
            }

            basicNotification.Title = SETTINGS_FAILED;
            _notificationProvider.Register(basicNotification);
            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveEpisodeSorting(EpisodeSortingModel data)
        {
            var basicNotification = new BasicNotification();
            basicNotification.Type = BasicNotificationType.Info;
            basicNotification.AutoDismiss = true;

            if (ModelState.IsValid)
            {
                _configProvider.SeriesName = data.SeriesName;
                _configProvider.EpisodeName = data.EpisodeName;
                _configProvider.ReplaceSpaces = data.ReplaceSpaces;
                _configProvider.AppendQuality = data.AppendQuality;
                _configProvider.UseSeasonFolder = data.SeasonFolders;
                _configProvider.SeasonFolderFormat = data.SeasonFolderFormat;
                _configProvider.SeparatorStyle = data.SeparatorStyle;
                _configProvider.NumberStyle = data.NumberStyle;
                _configProvider.MultiEpisodeStyle = data.MultiEpisodeStyle;

                basicNotification.Title = SETTINGS_SAVED;
                _notificationProvider.Register(basicNotification);
                return Content(SETTINGS_SAVED);
            }

            basicNotification.Title = SETTINGS_FAILED;
            _notificationProvider.Register(basicNotification);
            return Content(SETTINGS_FAILED);
        }
    }
}