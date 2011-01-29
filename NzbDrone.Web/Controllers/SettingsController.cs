using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NLog;
using NzbDrone.Core.Providers;  
using NzbDrone.Web.Models;

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SettingsController : Controller
    {
        private IConfigProvider _configProvider;
        private IIndexerProvider _indexerProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SettingsController(IConfigProvider configProvider, IIndexerProvider indexerProvider)
        {
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
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
                                         TvFolder = _configProvider.SeriesRoot
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
                Rentention = Convert.ToInt32(_configProvider.GetValue("Rentention", "500", true)),
                SabHost = _configProvider.GetValue("SabHost", "localhost", false),
                SabPort = Convert.ToInt32(_configProvider.GetValue("SabPort", "8080", true)),
                SabApiKey = _configProvider.GetValue("SabApiKey", String.Empty, false),
                SabUsername = _configProvider.GetValue("SabUsername", String.Empty, false),
                SabPassword = _configProvider.GetValue("SabPassword", String.Empty, false),
                SabCategory = _configProvider.GetValue("SabCategory", String.Empty, false),
                //SabPriority = _configProvider.GetValue("SabPriority", String.Empty, false)
            });
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
                if (Request.IsAjaxRequest())
                    return Content("Settings Saved.");

                return Content("Settings Saved.");
                Logger.Error("");
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
                _configProvider.SeriesRoot = data.TvFolder;
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
                foreach (var indexer in data.Indexers)
                {
                    _indexerProvider.Update(indexer);
                }

                _configProvider.SetValue("NzbMatrixUsername", data.NzbMatrixUsername);
                _configProvider.SetValue("NzbMatrixApiKey", data.NzbMatrixApiKey);
                _configProvider.SetValue("NzbsOrgUId", data.NzbsOrgUId);
                _configProvider.SetValue("NzbsOrgHash", data.NzbsOrgHash);
                _configProvider.SetValue("NzbsrusUId", data.NzbsrusUId);
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

                if (data.Rentention > 0)
                    _configProvider.SetValue("Retention", data.Rentention.ToString());

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
        public ActionResult SortedList(List<object > items)
        {
            return Content("Settings Saved.");
        }  
    }
}
