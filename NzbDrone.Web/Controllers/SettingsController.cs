using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NLog;
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
            return View("Index", new SettingsModel
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
            return View("Index", new SettingsModel
            {
                //Sync Frequency
                //Download Propers?
                //Retention
                //SAB Host/IP
                //SAB Port
                //SAB APIKey
                //SAB Username
                //SAB Password
                //SAB Category
                //SAB Priority

                SyncFrequency = Convert.ToInt32(_configProvider.GetValue("SyncFrequency", "15", true)),
                DownloadPropers = Convert.ToBoolean(_configProvider.GetValue("DownloadPropers", "false", true)),
                Retention = Convert.ToInt32(_configProvider.GetValue("Retention", "500", true)),
                SabHost = _configProvider.GetValue("SabHost", "localhost", false),
                SabPort = Convert.ToInt32(_configProvider.GetValue("SabPort", "8080", true)),
                SabApiKey = _configProvider.GetValue("SabApiKey", String.Empty, false),
                SabUsername = _configProvider.GetValue("SabUsername", String.Empty, false),
                SabPassword = _configProvider.GetValue("SabPassword", String.Empty, false),
                SabCategory = _configProvider.GetValue("SabCategory", String.Empty, false),
                //SabPriority = _configProvider.GetValue("SabPriority", String.Empty, false)
            });
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
            
            var defaultQualityProfileId = Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", profiles[0].ProfileId, true));

            var selectList = new SelectList(profiles, "ProfileId", "Name");

            var model = new QualityModel
                                     {
                                         Profiles = profiles,
                                         UserProfiles = userProfiles,
                                         DefaultProfileId = defaultQualityProfileId,
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

        [HttpPost]
        public ActionResult Index(SettingsModel data)
        {
            try
            {
                _configProvider.SeriesRoot = data.TvFolder;
                _configProvider.SetValue("NzbMatrixUsername", data.NzbMatrixUsername);
                _configProvider.SetValue("NzbMatrixApiKey", data.NzbMatrixApiKey);
                _configProvider.SetValue("NzbsOrgUId", data.NzbsOrgUId);
                _configProvider.SetValue("NzbsOrgHash", data.NzbsOrgHash);
            }
            catch (Exception)
            {
                Logger.Error("Error saving settings.");

                if (Request.IsAjaxRequest())
                    return Content("Error saving settings.");

                return Content("Error saving settings.");
            }


            if (Request.IsAjaxRequest())
                return Content("Settings Saved.");

            return Content("Settings Saved.");
        }

        [HttpPost]
        public ActionResult SaveGeneral(SettingsModel data)
        {
            try
            {
                if (data.TvFolder != null)
                    _configProvider.SeriesRoot = data.TvFolder;

                //if (data.Quality != null)
                //    _configProvider.SetValue("Quality", data.Quality);
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
                if (Request.IsAjaxRequest())
                    return Content("Error Saving Settings, please fix any errors");

                return Content("Error Saving Settings, please fix any errors");
            }
            
            if (Request.IsAjaxRequest())
                return Content("Settings Saved.");

            return Content("Settings Saved.");
        }

        [HttpPost]
        public ActionResult SaveIndexers(SettingsModel data)
        {
            try
            {
                //Todo: Only allow indexers to be enabled if user information has been provided
                foreach (var indexer in data.Indexers)
                {
                    indexer.ApiUrl = String.Empty; //TODO: Remove this and use a Real API URL
                    _indexerProvider.Update(indexer);
                }

                if (data.NzbMatrixUsername != null)
                    _configProvider.SetValue("NzbMatrixUsername", data.NzbMatrixUsername);

                if (data.NzbMatrixApiKey != null)
                    _configProvider.SetValue("NzbMatrixApiKey", data.NzbMatrixApiKey);

                if (data.NzbsOrgUId != null)
                    _configProvider.SetValue("NzbsOrgUId", data.NzbsOrgUId);

                if (data.NzbsOrgHash != null)
                    _configProvider.SetValue("NzbsOrgHash", data.NzbsOrgHash);

                if (data.NzbsrusUId != null)
                    _configProvider.SetValue("NzbsrusUId", data.NzbsrusUId);

                if (data.NzbsrusHash != null)
                    _configProvider.SetValue("NzbsrusHash", data.NzbsrusHash);

            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
                if (Request.IsAjaxRequest())
                    return Content("Error Saving Settings, please fix any errors");

                return Content("Error Saving Settings, please fix any errors");
            }

            if (Request.IsAjaxRequest())
                return Content("Settings Saved.");

            return Content("Settings Saved.");
        }

        [HttpPost]
        public ActionResult SaveDownloads(SettingsModel data)
        {
            try
            {
                if (data.SyncFrequency > 15)
                    _configProvider.SetValue("SyncFrequency", data.SyncFrequency.ToString());

                _configProvider.SetValue("DownloadPropers", data.DownloadPropers.ToString());

                if (data.Retention > 0)
                    _configProvider.SetValue("Retention", data.Retention.ToString());

                if (data.SabHost != null)
                    _configProvider.SetValue("SabHost", data.SabHost);

                if (data.SabPort > 0)
                    _configProvider.SetValue("SabPort", data.SabPort.ToString());

                if (data.SabApiKey != null)
                    _configProvider.SetValue("SabApiKey", data.SabApiKey);

                if (data.SabUsername != null)
                    _configProvider.SetValue("SabUsername", data.SabUsername);

                if (data.SabPassword != null)
                    _configProvider.SetValue("SabPassword", data.SabPassword);

                if (data.SabCategory != null)
                    _configProvider.SetValue("SabCategory", data.SabCategory);

                //if (data.SabPriority != null)
                //    _configProvider.SetValue("SabPriority", data.SabPriority.ToString());
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
                if (Request.IsAjaxRequest())
                    return Content("Error Saving Settings, please fix any errors");

                return Content("Error Saving Settings, please fix any errors");
            }

            if (Request.IsAjaxRequest())
                return Content("Settings Saved.");

            return Content("Settings Saved.");
        }

        [HttpPost]
        public ActionResult SaveQuality(QualityModel data)
        {
            try
            {
                _configProvider.SetValue("DefaultQualityProfile", data.DefaultProfileId.ToString());

                foreach (var dbProfile in _qualityProvider.GetAllProfiles().Where(q => q.UserProfile))
                {
                    if (!data.UserProfiles.Exists(p => p.ProfileId == dbProfile.ProfileId))
                        _qualityProvider.Delete(dbProfile.ProfileId);
                }


                foreach (var profile in data.UserProfiles)
                {
                    Logger.Debug(String.Format("Updating User Profile: {0}", profile));

                    profile.Allowed = new List<QualityTypes>();
                    foreach (var quality in profile.AllowedString.Split(','))
                    {
                        var qType = (QualityTypes)Enum.Parse(typeof (QualityTypes), quality);
                        profile.Allowed.Add(qType);
                    }

                    //If the Cutoff value selected is not in the allowed list then use the last allowed value, this should be validated on submit
                    if (!profile.Allowed.Contains(profile.Cutoff))
                        throw new InvalidOperationException("Invalid Cutoff Value");
                        //profile.Cutoff = profile.Allowed.Last();

                    if (profile.ProfileId > 0)
                        _qualityProvider.Update(profile);

                    else
                        _qualityProvider.Add(profile);
                }
            }

            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
                if (Request.IsAjaxRequest())
                    return Content("Error Saving Settings, please fix any errors");

                return Content("Error Saving Settings, please fix any errors");
            }

            if (Request.IsAjaxRequest())
                return Content("Settings Saved.");

            return Content("Settings Saved.");
        }

        [HttpPost]
        public ActionResult SortedList(List<object > items)
        {
            return Content("Settings Saved.");
        }  
    }
}
