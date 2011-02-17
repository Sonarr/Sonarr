using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _settingsSaved = "Settings Saved.";
        private string _settingsFailed = "Error Saving Settings, please fix any errors";

        public SettingsController(IConfigProvider configProvider, IIndexerProvider indexerProvider, IQualityProvider qualityProvider)
        {
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
            _qualityProvider = qualityProvider;
        }

        public ActionResult Index(string viewName)
        {
            if (viewName != null)
                ViewData["viewName"] = viewName;

            else
                ViewData["viewName"] = "General";

            return View("Index", new SettingsModel
                                     {
                                         TvFolder = _configProvider.SeriesRoot
                                     });
        }

        public ActionResult General()
        {
            ViewData["viewName"] = "General";
            return View("Index", new SettingsModel
                                     {
                                         TvFolder = _configProvider.SeriesRoot,
                                         Quality = Convert.ToInt32(_configProvider.GetValue("Quality", "1", true)),
                                     });
        }

        public ActionResult Indexers()
        {
            ViewData["viewName"] = "Indexers";
            return View("Index", new IndexerSettingsModel
                                     {
                                         NzbMatrixUsername = _configProvider.GetValue("NzbMatrixUsername", String.Empty, false),
                                         NzbMatrixApiKey = _configProvider.GetValue("NzbMatrixApiKey", String.Empty, false),
                                         NzbsOrgUId = _configProvider.GetValue("NzbsOrgUId", String.Empty, false),
                                         NzbsOrgHash = _configProvider.GetValue("NzbsOrgHash", String.Empty, false),
                                         NzbsrusUId = _configProvider.GetValue("NzbsrusUId", String.Empty, false),
                                         NzbsrusHash = _configProvider.GetValue("NzbsrusHash", String.Empty, false),
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
                                SabHost = _configProvider.GetValue("SabHost", "localhost", false),
                                SabPort = Convert.ToInt32(_configProvider.GetValue("SabPort", "8080", true)),
                                SabApiKey = _configProvider.GetValue("SabApiKey", String.Empty, false),
                                SabUsername = _configProvider.GetValue("SabUsername", String.Empty, false),
                                SabPassword = _configProvider.GetValue("SabPassword", String.Empty, false),
                                SabCategory = _configProvider.GetValue("SabCategory", String.Empty, false),
                                SabPriority = (SabnzbdPriorityType)Enum.Parse(typeof(SabnzbdPriorityType), _configProvider.GetValue("SabPriority", "Normal", true)),
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
            if (ModelState.IsValid)
            {
                _configProvider.SeriesRoot = data.TvFolder;
                return Content(_settingsSaved);
            }

            return Content(_settingsFailed);
        }

        [HttpPost]
        public ActionResult SaveIndexers(IndexerSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                //Todo: Only allow indexers to be enabled if user information has been provided
                foreach (var indexer in data.Indexers)
                    _indexerProvider.Update(indexer);

                _configProvider.SetValue("NzbMatrixUsername", data.NzbMatrixUsername);
                _configProvider.SetValue("NzbMatrixApiKey", data.NzbMatrixApiKey);
                _configProvider.SetValue("NzbsOrgUId", data.NzbsOrgUId);
                _configProvider.SetValue("NzbsOrgHash", data.NzbsOrgHash);
                _configProvider.SetValue("NzbsrusUId", data.NzbsrusUId);
                _configProvider.SetValue("NzbsrusHash", data.NzbsrusHash);

                return Content(_settingsSaved);
            }

            return Content(_settingsFailed);
        }

        [HttpPost]
        public ActionResult SaveDownloads(DownloadSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SetValue("SyncFrequency", data.SyncFrequency.ToString());
                _configProvider.SetValue("DownloadPropers", data.DownloadPropers.ToString());
                _configProvider.SetValue("Retention", data.Retention.ToString());
                _configProvider.SetValue("SabHost", data.SabHost);
                _configProvider.SetValue("SabPort", data.SabPort.ToString());
                _configProvider.SetValue("SabApiKey", data.SabApiKey);
                _configProvider.SetValue("SabUsername", data.SabUsername);
                _configProvider.SetValue("SabPassword", data.SabPassword);
                _configProvider.SetValue("SabCategory", data.SabCategory);
                _configProvider.SetValue("SabPriority", data.SabPriority.ToString());

                return Content(_settingsSaved);
            }

            return Content(_settingsFailed);
        }

        [HttpPost]
        public ActionResult SaveQuality(QualityModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SetValue("DefaultQualityProfile", data.DefaultQualityProfileId.ToString());

                //Saves only the Default Quality, skips User Profiles since none exist
                if (data.UserProfiles == null)
                    return Content(_settingsSaved);

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

                    return Content(_settingsSaved);
                }
            }

            return Content(_settingsFailed);
        }
    }
}
