using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NLog;
using NzbDrone.Core;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SettingsController : Controller
    {
        private IConfigProvider _configProvider;
        private IIndexerProvider _indexerProvider;
        private IQualityProvider _qualityProvider;
        private IRootDirProvider _rootDirProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string SETTINGS_SAVED = "Settings Saved.";
        private const string SETTINGS_FAILED = "Error Saving Settings, please fix any errors";

        public SettingsController(IConfigProvider configProvider, IIndexerProvider indexerProvider,
            IQualityProvider qualityProvider, IRootDirProvider rootDirProvider)
        {
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
            _qualityProvider = qualityProvider;
            _rootDirProvider = rootDirProvider;
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
                                         NzbMatrixUsername = _configProvider.GetValue("NzbMatrixUsername", String.Empty, true),
                                         NzbMatrixApiKey = _configProvider.GetValue("NzbMatrixApiKey", String.Empty, true),
                                         NzbsOrgUId = _configProvider.GetValue("NzbsOrgUId", String.Empty, true),
                                         NzbsOrgHash = _configProvider.GetValue("NzbsOrgHash", String.Empty, true),
                                         NzbsrusUId = _configProvider.GetValue("NzbsrusUId", String.Empty, true),
                                         NzbsrusHash = _configProvider.GetValue("NzbsrusHash", String.Empty, true),
                                         Indexers = _indexerProvider.AllIndexers()
                                     });
        }

        public ActionResult Downloads()
        {
            ViewData["viewName"] = "Downloads";

            var model = new DownloadSettingsModel
                            {
                                SyncFrequency = Convert.ToInt32(_configProvider.GetValue("SyncFrequency", "15", true)),
                                DownloadPropers = Convert.ToBoolean(_configProvider.GetValue("DownloadPropers", "false", true)),
                                Retention = Convert.ToInt32(_configProvider.GetValue("Retention", "500", true)),
                                SabHost = _configProvider.GetValue("SabHost", "localhost", true),
                                SabPort = Convert.ToInt32(_configProvider.GetValue("SabPort", "8080", true)),
                                SabApiKey = _configProvider.GetValue("SabApiKey", String.Empty, true),
                                SabUsername = _configProvider.GetValue("SabUsername", String.Empty, true),
                                SabPassword = _configProvider.GetValue("SabPassword", String.Empty, true),
                                SabTvCategory = _configProvider.GetValue("SabTvCategory", String.Empty, true),
                                SabTvPriority = (SabnzbdPriorityType)Enum.Parse(typeof(SabnzbdPriorityType), _configProvider.GetValue("SabTvPriority", "Normal", true)),
                                UseBlackHole = Convert.ToBoolean(_configProvider.GetValue("UseBlackHole", true, true)),
                                BlackholeDirectory = _configProvider.GetValue("BlackholeDirectory", String.Empty, true)
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

            var defaultQualityQualityProfileId = Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", profiles[0].QualityProfileId, true));

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

            return View("UserProfileSection", new QualityProfile { Name = "New Profile", UserProfile = true });
        }

        public ViewResult AddRootDir()
        {
            return View("RootDir", new RootDir());
        }

        public ActionResult SubMenu()
        {
            return PartialView();
        }

        public QualityModel GetUpdatedProfileList()
        {
            var profiles = _qualityProvider.GetAllProfiles().ToList();
            var defaultQualityQualityProfileId = Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", profiles[0].QualityProfileId, true));
            var selectList = new SelectList(profiles, "QualityProfileId", "Name");

            return new QualityModel { DefaultQualityProfileId = defaultQualityQualityProfileId, SelectList = selectList };
        }

        [HttpPost]
        public ActionResult SaveGeneral(SettingsModel data)
        {
            if (data.Directories != null && data.Directories.Count > 0)
            {
                //If the Javascript was beaten we need to return an error

                /*   
                 * Kay.one: I can't see what its doing, all it does it dooesn't let me s.
                 * if (!data.Directories.Exists(d => d.Default))
                         return Content(SETTINGS_FAILED);*/

                var currentRootDirs = _rootDirProvider.GetAll();

                foreach (var currentRootDir in currentRootDirs)
                {
                    var closureRootDir = currentRootDir;
                    if (!data.Directories.Exists(d => d.Id == closureRootDir.Id))
                        _rootDirProvider.Remove(closureRootDir.Id);
                }

                foreach (var dir in data.Directories)
                {
                    if (dir.Id == 0)
                        _rootDirProvider.Add(dir);

                    else
                        _rootDirProvider.Update(dir);
                }

                return Content(SETTINGS_SAVED);
            }

            return Content(SETTINGS_FAILED);
        }

        [HttpPost]
        public ActionResult SaveIndexers(IndexerSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                //Todo: Only allow indexers to be enabled if user information has been provided
                foreach (var indexer in data.Indexers)
                    _indexerProvider.Update(indexer);

                _configProvider.NzbMatrixUsername = data.NzbMatrixUsername;
                _configProvider.NzbMatrixApiKey = data.NzbMatrixApiKey;
                _configProvider.NzbsOrgUId = data.NzbsOrgUId;
                _configProvider.NzbsOrgHash = data.NzbsOrgHash;
                _configProvider.NzbsrusUId = data.NzbsrusUId;
                _configProvider.NzbsrusHash = data.NzbsrusHash;

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
                _configProvider.UseBlackhole = data.UseBlackHole.ToString();
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

                foreach (var dbProfile in _qualityProvider.GetAllProfiles().Where(q => q.UserProfile))
                {
                    if (!data.UserProfiles.Exists(p => p.QualityProfileId == dbProfile.QualityProfileId))
                        _qualityProvider.Delete(dbProfile.QualityProfileId);
                }

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

                    if (profile.QualityProfileId > 0)
                        _qualityProvider.Update(profile);

                    else
                        _qualityProvider.Add(profile);

                    return Content(SETTINGS_SAVED);
                }
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
