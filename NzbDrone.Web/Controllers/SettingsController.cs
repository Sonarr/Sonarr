using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
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

        public SettingsController(ConfigProvider configProvider, IndexerProvider indexerProvider,
                                  QualityProvider qualityProvider, RootDirProvider rootDirProvider,
                                  AutoConfigureProvider autoConfigureProvider)
        {
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
            _qualityProvider = qualityProvider;
            _rootDirProvider = rootDirProvider;
            _autoConfigureProvider = autoConfigureProvider;
        }

        public ActionResult Index(string viewName)
        {
            if (viewName != null)
                ViewData["viewName"] = viewName;

            else
                return RedirectToAction("General");

            return View("Index");
        }

        public ActionResult General()
        {
            ViewData["viewName"] = "General";

            return View("Index", new SettingsModel
                                     {
                                         Directories = _rootDirProvider.GetAll()
                                     });
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

                                         NzbsOrgEnabled = _indexerProvider.GetSettings(typeof(NzbsOrgProvider)).Enable,
                                         NzbMatrixEnabled = _indexerProvider.GetSettings(typeof(NzbMatrixProvider)).Enable,
                                         NzbsRUsEnabled = _indexerProvider.GetSettings(typeof(NzbsRUsProvider)).Enable,
                                         NewzbinEnabled = _indexerProvider.GetSettings(typeof(NewzbinProvider)).Enable
                                     });
        }

        public ActionResult Downloads()
        {
            ViewData["viewName"] = "Downloads";

            var model = new DownloadSettingsModel
                            {
                                SyncFrequency = Convert.ToInt32(_configProvider.SyncFrequency),
                                DownloadPropers = Convert.ToBoolean(_configProvider.DownloadPropers),
                                Retention = Convert.ToInt32(_configProvider.Retention),
                                SabHost = _configProvider.SabHost,
                                SabPort = Convert.ToInt32(_configProvider.SabPort),
                                SabApiKey = _configProvider.SabApiKey,
                                SabUsername = _configProvider.SabUsername,
                                SabPassword = _configProvider.SabPassword,
                                SabTvCategory = _configProvider.SabTvCategory,
                                SabTvPriority =
                                    (SabnzbdPriorityType)
                                    Enum.Parse(typeof(SabnzbdPriorityType),
                                               _configProvider.SabTvPriority),
                                UseBlackHole = Convert.ToBoolean(_configProvider.UseBlackhole),
                                BlackholeDirectory = _configProvider.BlackholeDirectory
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

            var userProfiles = _qualityProvider.GetAllProfiles().Where(q => q.UserProfile).ToList();
            var profiles = _qualityProvider.GetAllProfiles().ToList();

            var defaultQualityQualityProfileId =
                Convert.ToInt32(_configProvider.DefaultQualityProfile);

            var selectList = new SelectList(profiles, "QualityProfileId", "Name");

            var model = new QualityModel
                            {
                                Profiles = profiles,
                                UserProfiles = userProfiles,
                                DefaultQualityProfileId = defaultQualityQualityProfileId,
                                SelectList = selectList
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

            model.ShowName = Convert.ToBoolean(_configProvider.GetValue("Sorting_ShowName", true, true));
            model.EpisodeName = Convert.ToBoolean(_configProvider.GetValue("Sorting_EpisodeName", true, true));
            model.ReplaceSpaces = Convert.ToBoolean(_configProvider.GetValue("Sorting_ReplaceSpaces", false, true));
            model.AppendQuality = Convert.ToBoolean(_configProvider.GetValue("Sorting_AppendQuality", false, true));
            model.UseAirByDate = Convert.ToBoolean(_configProvider.GetValue("Sorting_UseAirByDate", true, true));
            model.SeasonFolders = _configProvider.UseSeasonFolder;
            model.SeasonFolderFormat = _configProvider.GetValue("Sorting_SeasonFolderFormat", "Season %s", true);
            model.SeparatorStyle = Convert.ToInt32(_configProvider.GetValue("Sorting_SeparatorStyle", 0, true));
            model.NumberStyle = Convert.ToInt32(_configProvider.GetValue("Sorting_NumberStyle", 2, true));
            model.MultiEpisodeStyle = Convert.ToInt32(_configProvider.GetValue("Sorting_MultiEpisodeStyle", 0, true));

            model.SeparatorStyles = new SelectList(EpisodeSortingHelper.GetSeparatorStyles(), "Id", "Name");
            model.NumberStyles = new SelectList(EpisodeSortingHelper.GetNumberStyles(), "Id", "Name");
            model.MultiEpisodeStyles = new SelectList(EpisodeSortingHelper.GetMultiEpisodeStyles(), "Id", "Name");

            return View("Index", model);
        }

        public ViewResult AddUserProfile()
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
                                         UserProfile = true,
                                         Allowed = new List<QualityTypes> { QualityTypes.Unknown },
                                         Cutoff = QualityTypes.Unknown,
                                     };

            var id = _qualityProvider.Add(qualityProfile);
            qualityProfile.QualityProfileId = id;
            qualityProfile.Allowed = null;

            ViewData["ProfileId"] = id;

            return View("UserProfileSection", qualityProfile);
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

            return PartialView("UserProfileSection", profile);
        }

        public ViewResult AddRootDir()
        {
            var rootDir = new RootDir { Path = String.Empty };

            var id = _rootDirProvider.Add(rootDir);
            rootDir.Id = id;

            ViewData["RootDirId"] = id;

            return View("RootDir", rootDir);
        }

        public ActionResult GetRootDirView(RootDir rootDir)
        {
            ViewData["RootDirId"] = rootDir.Id;

            return PartialView("RootDir", rootDir);
        }

        public JsonResult DeleteRootDir(int rootDirId)
        {
            try
            {
                _rootDirProvider.Remove(rootDirId);
            }

            catch (Exception)
            {
                return new JsonResult { Data = "failed" };
            }

            return new JsonResult { Data = "ok" };
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

            return new QualityModel { DefaultQualityProfileId = defaultQualityQualityProfileId, SelectList = selectList };
        }

        public JsonResult DeleteQualityProfile(int profileId)
        {
            try
            {
                _qualityProvider.Delete(profileId);
            }

            catch (Exception)
            {
                return new JsonResult { Data = "failed" };
            }

            return new JsonResult { Data = "ok" };
        }

        public JsonResult AutoConfigureSab(string username, string password)
        {
            SabnzbdInfoModel info;

            try
            {
                //info = _autoConfigureProvider.AutoConfigureSab(username, password);
                info = new SabnzbdInfoModel
                           {
                               ApiKey = "123456",
                               Port = 2222
                           };
            }

            catch (Exception)
            {
                return new JsonResult { Data = "failed" };
            }

            return Json(info);
        }

        [HttpPost]
        public ActionResult SaveGeneral(SettingsModel data)
        {
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
                return Content(SETTINGS_FAILED);
            }

            return Content(SETTINGS_SAVED);
        }

        [HttpPost]
        public ActionResult SaveIndexers(IndexerSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                var nzbsOrgSettings = _indexerProvider.GetSettings(typeof(NzbsOrgProvider));
                nzbsOrgSettings.Enable = data.NzbsOrgEnabled;
                _indexerProvider.SaveSettings(nzbsOrgSettings);

                var nzbMatrixSettings = _indexerProvider.GetSettings(typeof(NzbMatrixProvider));
                nzbMatrixSettings.Enable = data.NzbMatrixEnabled;
                _indexerProvider.SaveSettings(nzbMatrixSettings);

                var nzbsRUsSettings = _indexerProvider.GetSettings(typeof(NzbsRUsProvider));
                nzbsRUsSettings.Enable = data.NzbsRUsEnabled;
                _indexerProvider.SaveSettings(nzbsRUsSettings);

                var newzbinSettings = _indexerProvider.GetSettings(typeof(NewzbinProvider));
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

                return Content(SETTINGS_SAVED);
            }

            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveDownloads(DownloadSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SyncFrequency = data.SyncFrequency.ToString();
                _configProvider.DownloadPropers = data.DownloadPropers.ToString();
                _configProvider.Retention = data.Retention.ToString();
                _configProvider.SabHost = data.SabHost;
                _configProvider.SabPort = data.SabPort.ToString();
                _configProvider.SabApiKey = data.SabApiKey;
                _configProvider.SabPassword = data.SabPassword;
                _configProvider.SabTvCategory = data.SabTvCategory;
                _configProvider.SabUsername = data.SabUsername;
                _configProvider.SabTvPriority = data.SabTvPriority.ToString();
                _configProvider.UseBlackhole = data.UseBlackHole;
                _configProvider.BlackholeDirectory = data.BlackholeDirectory;

                return Content(SETTINGS_SAVED);
            }

            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveQuality(QualityModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SetValue("DefaultQualityProfile", data.DefaultQualityProfileId.ToString());

                //Saves only the Default Quality, skips User Profiles since none exist
                if (data.UserProfiles == null)
                    return Content(SETTINGS_SAVED);

                foreach (var profile in data.UserProfiles)
                {
                    Logger.Debug(String.Format("Updating User Profile: {0}", profile));

                    profile.Allowed = new List<QualityTypes>();
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
                return Content(SETTINGS_SAVED);
            }

            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveNotifications(NotificationSettingsModel data)
        {
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

                return Content(SETTINGS_SAVED);
            }

            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveEpisodeSorting(EpisodeSortingModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SetValue("Sorting_ShowName", data.ShowName.ToString());
                _configProvider.SetValue("Sorting_EpisodeName", data.EpisodeName.ToString());
                _configProvider.SetValue("Sorting_ReplaceSpaces", data.ReplaceSpaces.ToString());
                _configProvider.SetValue("Sorting_AppendQuality", data.AppendQuality.ToString());
                _configProvider.SetValue("Sorting_UseAirByDate", data.UseAirByDate.ToString());
                _configProvider.SetValue("Sorting_SeasonFolder", data.SeasonFolders.ToString());
                _configProvider.SetValue("Sorting_SeasonFolderFormat", data.SeasonFolderFormat);
                _configProvider.SetValue("Sorting_SeparatorStyle", data.SeparatorStyle.ToString());
                _configProvider.SetValue("Sorting_NumberStyle", data.NumberStyle.ToString());
                _configProvider.SetValue("Sorting_MultiEpisodeStyle", data.MultiEpisodeStyle.ToString());

                return Content(SETTINGS_SAVED);
            }

            return Content(SETTINGS_FAILED);
        }
    }
}