using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Web.Filters;
using NzbDrone.Web.Models;
using QualityModel = NzbDrone.Web.Models.QualityModel;

namespace NzbDrone.Web.Controllers
{
    [HandleError]
    public class SettingsController : Controller
    {
        private readonly ConfigProvider _configProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly QualityProvider _qualityProvider;
        private readonly AutoConfigureProvider _autoConfigureProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;
        private readonly QualityTypeProvider _qualityTypeProvider;
        private readonly ConfigFileProvider _configFileProvider;
        private readonly NewznabProvider _newznabProvider;
        private readonly MetadataProvider _metadataProvider;
        private readonly JobProvider _jobProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SettingsController(ConfigProvider configProvider, IndexerProvider indexerProvider,
                                    QualityProvider qualityProvider, AutoConfigureProvider autoConfigureProvider,
                                    SeriesProvider seriesProvider, ExternalNotificationProvider externalNotificationProvider,
                                    QualityTypeProvider qualityTypeProvider, ConfigFileProvider configFileProvider, 
                                    NewznabProvider newznabProvider, MetadataProvider metadataProvider,
                                    JobProvider jobProvider)
        {
            _externalNotificationProvider = externalNotificationProvider;
            _qualityTypeProvider = qualityTypeProvider;
            _configFileProvider = configFileProvider;
            _newznabProvider = newznabProvider;
            _metadataProvider = metadataProvider;
            _jobProvider = jobProvider;
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;
            _qualityProvider = qualityProvider;
            _autoConfigureProvider = autoConfigureProvider;
            _seriesProvider = seriesProvider;
        }


        public ActionResult Index()
        {
            return RedirectToAction("Naming", "Settings");
        }

        public ActionResult Naming()
        {
            var model = new EpisodeNamingModel();

            model.SeriesName = _configProvider.SortingIncludeSeriesName;
            model.EpisodeName = _configProvider.SortingIncludeEpisodeTitle;
            model.ReplaceSpaces = _configProvider.SortingReplaceSpaces;
            model.AppendQuality = _configProvider.SortingAppendQuality;
            model.SeasonFolders = _configProvider.UseSeasonFolder;
            model.SeasonFolderFormat = _configProvider.SortingSeasonFolderFormat;
            model.SeparatorStyle = _configProvider.SortingSeparatorStyle;
            model.NumberStyle = _configProvider.SortingNumberStyle;
            model.MultiEpisodeStyle = _configProvider.SortingMultiEpisodeStyle;
            model.SceneName = _configProvider.SortingUseSceneName;

            model.SeparatorStyles = new SelectList(EpisodeSortingHelper.GetSeparatorStyles(), "Id", "Name");
            model.NumberStyles = new SelectList(EpisodeSortingHelper.GetNumberStyles(), "Id", "Name");
            model.MultiEpisodeStyles = new SelectList(EpisodeSortingHelper.GetMultiEpisodeStyles(), "Id", "Name");

            //Metadata
            model.MetadataXbmcEnabled = _metadataProvider.GetSettings(typeof(Core.Providers.Metadata.Xbmc)).Enable;
            model.MetadataUseBanners = _configProvider.MetadataUseBanners;

            return View(model);
        }

        public ActionResult Indexers()
        {
            return View(new IndexerSettingsModel
                            {
                                Retention = _configProvider.Retention,

                                NzbsrusUId = _configProvider.NzbsrusUId,
                                NzbsrusHash = _configProvider.NzbsrusHash,

                                FileSharingTalkUid = _configProvider.FileSharingTalkUid,
                                FileSharingTalkSecret = _configProvider.FileSharingTalkSecret,

                                NzbsRUsEnabled = _indexerProvider.GetSettings(typeof(NzbsRUs)).Enable,
                                NewznabEnabled = _indexerProvider.GetSettings(typeof(Newznab)).Enable,
                                WomblesEnabled = _indexerProvider.GetSettings(typeof(Wombles)).Enable,
                                FileSharingTalkEnabled = _indexerProvider.GetSettings(typeof(FileSharingTalk)).Enable,
                                NzbIndexEnabled = _indexerProvider.GetSettings(typeof(NzbIndex)).Enable,
                                NzbClubEnabled = _indexerProvider.GetSettings(typeof(NzbClub)).Enable,

                                RssSyncInterval = _configProvider.RssSyncInterval,

                                NewznabDefinitions = _newznabProvider.All(),
                            });
        }

        public ActionResult DownloadClient()
        {
            var tvCategory = _configProvider.SabTvCategory;
            var tvCategorySelectList = new SelectList(new[] { tvCategory });

            var downloadClientTypes = new List<KeyValuePair<int, string>>();

            foreach (DownloadClientType downloadClientType in Enum.GetValues(typeof(DownloadClientType)))
                downloadClientTypes.Add(new KeyValuePair<int, string>((int)downloadClientType, downloadClientType.ToString()));

            var model = new DownloadClientSettingsModel
                            {
                                SabHost = _configProvider.SabHost,
                                SabPort = _configProvider.SabPort,
                                SabApiKey = _configProvider.SabApiKey,
                                SabUsername = _configProvider.SabUsername,
                                SabPassword = _configProvider.SabPassword,
                                SabTvCategory = tvCategory,
                                SabBacklogTvPriority = _configProvider.SabBacklogTvPriority,
                                SabRecentTvPriority = _configProvider.SabRecentTvPriority,
                                DownloadClientDropDirectory = _configProvider.DownloadClientTvDirectory,
                                SabTvCategorySelectList = tvCategorySelectList,
                                DownloadClient = (int)_configProvider.DownloadClient,
                                BlackholeDirectory = _configProvider.BlackholeDirectory,
                                DownloadClientSelectList = new SelectList(downloadClientTypes, "Key", "Value"),
                                PneumaticDirectory = _configProvider.PneumaticDirectory
                            };

            return View(model);
        }

        public ActionResult Quality()
        {
            var profiles = _qualityProvider.All();

            var defaultQualityQualityProfileId = Convert.ToInt32(_configProvider.DefaultQualityProfile);
            var qualityProfileSelectList = new SelectList(profiles, "QualityProfileId", "Name");
            var qualityTypesFromDb = _qualityTypeProvider.All();

            var model = new QualityModel
                            {
                                DefaultQualityProfileId = defaultQualityQualityProfileId,
                                QualityProfileSelectList = qualityProfileSelectList,
                                SdtvMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 1).MaxSize,
                                DvdMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 2).MaxSize,
                                HdtvMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 4).MaxSize,
                                Webdl720pMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 5).MaxSize,
                                Webdl1080pMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 3).MaxSize,
                                Bluray720pMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 6).MaxSize,
                                Bluray1080pMaxSize = qualityTypesFromDb.Single(q => q.QualityTypeId == 7).MaxSize
                            };

            ViewData["Profiles"] = profiles.Select(s => s.QualityProfileId).ToList();

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
                                XbmcUpdateWhenPlaying = _configProvider.XbmcUpdateWhenPlaying,
                                SmtpEnabled = _externalNotificationProvider.GetSettings(typeof(Smtp)).Enable,
                                SmtpNotifyOnGrab = _configProvider.SmtpNotifyOnGrab,
                                SmtpNotifyOnDownload = _configProvider.SmtpNotifyOnDownload,
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
                                ProwlPrioritySelectList = GetProwlPrioritySelectList(),
                                PlexEnabled = _externalNotificationProvider.GetSettings(typeof(Plex)).Enable,
                                PlexNotifyOnGrab = _configProvider.PlexNotifyOnGrab,
                                PlexNotifyOnDownload = _configProvider.PlexNotifyOnDownload,
                                PlexUpdateLibrary = _configProvider.PlexUpdateLibrary,
                                PlexServerHost = _configProvider.PlexServerHost,
                                PlexClientHosts = _configProvider.PlexClientHosts,
                                PlexUsername = _configProvider.PlexUsername,
                                PlexPassword = _configProvider.PlexPassword,
                            };

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
            model.RecycleBin = _configProvider.RecycleBin;

            return View(model);
        }

        public ActionResult Misc()
        {
            var model = new MiscSettingsModel();
            model.EnableBacklogSearching = _configProvider.EnableBacklogSearching;
            model.AutoIgnorePreviouslyDownloadedEpisodes = _configProvider.AutoIgnorePreviouslyDownloadedEpisodes;
            model.AllowedReleaseGroups = _configProvider.AllowedReleaseGroups;

            return View(model);
        }

        public PartialViewResult AddProfile()
        {
            var qualityProfile = new QualityProfile
                                     {
                                         Name = "New Profile",
                                         Allowed = new List<QualityTypes> { QualityTypes.Unknown },
                                         Cutoff = QualityTypes.Unknown
                                     };

            var qualityProfileId = _qualityProvider.Add(qualityProfile);

            return GetQualityProfileView(qualityProfileId);
        }

        public PartialViewResult GetQualityProfileView(int profileId)
        {
            var profile = _qualityProvider.Get(profileId);

            var model = new QualityProfileModel();
            model.QualityProfileId = profile.QualityProfileId;
            model.Name = profile.Name;
            model.Allowed = profile.Allowed;
            model.Sdtv = profile.Allowed.Contains(QualityTypes.SDTV);
            model.Dvd = profile.Allowed.Contains(QualityTypes.DVD);
            model.Hdtv = profile.Allowed.Contains(QualityTypes.HDTV);
            model.Webdl720p = profile.Allowed.Contains(QualityTypes.WEBDL720p);
            model.Webdl1080p = profile.Allowed.Contains(QualityTypes.WEBDL1080p);
            model.Bluray720p = profile.Allowed.Contains(QualityTypes.Bluray720p);
            model.Bluray1080p = profile.Allowed.Contains(QualityTypes.Bluray1080p);
            model.Cutoff = (int)profile.Cutoff;

            model.SdtvId = QualityTypes.SDTV.Id;
            model.DvdId = QualityTypes.DVD.Id;
            model.HdtvId = QualityTypes.HDTV.Id;
            model.Webdl720pId = QualityTypes.WEBDL720p.Id;
            model.Webdl1080pId = QualityTypes.WEBDL1080p.Id;
            model.Bluray720pId = QualityTypes.Bluray720p.Id;
            model.Bluray1080pId = QualityTypes.Bluray1080p.Id;

            return PartialView("QualityProfileItem", model);
        }

        [HttpPost]
        [JsonErrorFilter]
        public JsonResult DeleteQualityProfile(int profileId)
        {
            if (_seriesProvider.GetAllSeries().Where(s => s.QualityProfileId == profileId).Count() != 0)
                return JsonNotificationResult.Oops("Profile is still in use.");

            _qualityProvider.Delete(profileId);

            return Json("ok");
        }

        public PartialViewResult AddNewznabProvider()
        {
            var newznab = new NewznabDefinition
            {
                Enable = false,
                Name = "Newznab Provider"
            };

            var id = _newznabProvider.Save(newznab);
            newznab.Id = id;

            ViewData["ProviderId"] = id;

            return PartialView("NewznabProvider", newznab);
        }

        public ActionResult GetNewznabProviderView(NewznabDefinition provider)
        {
            ViewData["ProviderId"] = provider.Id;

            return PartialView("NewznabProvider", provider);
        }

        [JsonErrorFilter]
        public EmptyResult DeleteNewznabProvider(int providerId)
        {
            _newznabProvider.Delete(providerId);
            return new EmptyResult();
        }

        public QualityModel GetUpdatedProfileList()
        {
            var profiles = _qualityProvider.All().ToList();
            var defaultQualityQualityProfileId =
                Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", profiles[0].QualityProfileId));
            var selectList = new SelectList(profiles, "QualityProfileId", "Name");

            return new QualityModel { DefaultQualityProfileId = defaultQualityQualityProfileId, QualityProfileSelectList = selectList };
        }

        public JsonResult AutoConfigureSab()
        {
            try
            {
                var info = _autoConfigureProvider.AutoConfigureSab();

                if (info != null)
                    return Json(info, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
            }

            return JsonNotificationResult.Error("Auto-Configure Failed", "Please enter your SAB Settings Manually");
        }

        [HttpPost]
        public JsonResult SaveIndexers(IndexerSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.Retention = data.Retention;

                var nzbsRUsSettings = _indexerProvider.GetSettings(typeof(NzbsRUs));
                nzbsRUsSettings.Enable = data.NzbsRUsEnabled;
                _indexerProvider.SaveSettings(nzbsRUsSettings);

                var newznabSettings = _indexerProvider.GetSettings(typeof(Newznab));
                newznabSettings.Enable = data.NewznabEnabled;
                _indexerProvider.SaveSettings(newznabSettings);

                var womblesSettings = _indexerProvider.GetSettings(typeof(Wombles));
                womblesSettings.Enable = data.WomblesEnabled;
                _indexerProvider.SaveSettings(womblesSettings);

                var fileSharingTalkSettings = _indexerProvider.GetSettings(typeof(FileSharingTalk));
                fileSharingTalkSettings.Enable = data.FileSharingTalkEnabled;
                _indexerProvider.SaveSettings(fileSharingTalkSettings);

                var nzbIndexSettings = _indexerProvider.GetSettings(typeof(NzbIndex));
                nzbIndexSettings.Enable = data.NzbIndexEnabled;
                _indexerProvider.SaveSettings(nzbIndexSettings);

                var nzbClubSettings = _indexerProvider.GetSettings(typeof(NzbClub));
                nzbClubSettings.Enable = data.NzbClubEnabled;
                _indexerProvider.SaveSettings(nzbClubSettings);

                _configProvider.NzbsrusUId = data.NzbsrusUId;
                _configProvider.NzbsrusHash = data.NzbsrusHash;

                _configProvider.FileSharingTalkUid = data.FileSharingTalkUid;
                _configProvider.FileSharingTalkSecret = data.FileSharingTalkSecret;

                //Save the interval to config and immediately apply it the the job (to avoid a restart)
                _configProvider.RssSyncInterval = data.RssSyncInterval;

                var rssSyncJob = _jobProvider.GetDefinition(typeof(RssSyncJob));
                rssSyncJob.Interval = data.RssSyncInterval;
                _jobProvider.SaveDefinition(rssSyncJob);

                try
                {
                    if (data.NewznabDefinitions != null)
                        _newznabProvider.SaveAll(data.NewznabDefinitions);
                }
                catch(Exception)
                {
                    return JsonNotificationResult.Oops("Invalid Nzbnab Indexer found, please check your settings");
                }

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        [HttpPost]
        public JsonResult SaveDownloadClient(DownloadClientSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.SabHost = data.SabHost;
                _configProvider.SabPort = data.SabPort;
                _configProvider.SabApiKey = data.SabApiKey;
                _configProvider.SabPassword = data.SabPassword;
                _configProvider.SabTvCategory = data.SabTvCategory;
                _configProvider.SabUsername = data.SabUsername;
                _configProvider.SabBacklogTvPriority = data.SabBacklogTvPriority;
                _configProvider.SabRecentTvPriority = data.SabRecentTvPriority;
                _configProvider.DownloadClientTvDirectory = data.DownloadClientDropDirectory;
                _configProvider.BlackholeDirectory = data.BlackholeDirectory;
                _configProvider.DownloadClient = (DownloadClientType)data.DownloadClient;
                _configProvider.PneumaticDirectory = data.PneumaticDirectory;

                return GetSuccessResult();
            }

            return JsonNotificationResult.Oops("Invalid Data");
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

                foreach (var profileModel in data.Profiles)
                {
                    Logger.Debug(String.Format("Updating Profile: {0}", profileModel));

                    var profile = new QualityProfile();
                    profile.QualityProfileId = profileModel.QualityProfileId;
                    profile.Name = profileModel.Name;
                    profile.Cutoff = (QualityTypes)profileModel.Cutoff;

                    profile.Allowed = new List<QualityTypes>();

                    if (profileModel.Sdtv)
                        profile.Allowed.Add(QualityTypes.SDTV);

                    if (profileModel.Dvd)
                        profile.Allowed.Add(QualityTypes.DVD);

                    if (profileModel.Hdtv)
                        profile.Allowed.Add(QualityTypes.HDTV);

                    if (profileModel.Webdl720p)
                        profile.Allowed.Add(QualityTypes.WEBDL720p);

                    if (profileModel.Webdl1080p)
                        profile.Allowed.Add(QualityTypes.WEBDL1080p);

                    if (profileModel.Bluray720p)
                        profile.Allowed.Add(QualityTypes.Bluray720p);

                    if (profileModel.Bluray1080p)
                        profile.Allowed.Add(QualityTypes.Bluray1080p);

                    //If the Cutoff value selected is not in the allowed list then return an error
                    if (!profile.Allowed.Contains(profile.Cutoff))
                        return GetInvalidModelResult();

                    _qualityProvider.Update(profile);
                }

                var qualityTypesFromDb = _qualityTypeProvider.All();

                qualityTypesFromDb.Single(q => q.QualityTypeId == 1).MaxSize = data.SdtvMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 2).MaxSize = data.DvdMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 4).MaxSize = data.HdtvMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 5).MaxSize = data.Webdl720pMaxSize;
                qualityTypesFromDb.Single(q => q.QualityTypeId == 3).MaxSize = data.Webdl1080pMaxSize;
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
                _configProvider.XbmcUpdateWhenPlaying = data.XbmcUpdateWhenPlaying;

                //SMTP
                var smtpSettings = _externalNotificationProvider.GetSettings(typeof(Smtp));
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

                //Plex
                var plexSettings = _externalNotificationProvider.GetSettings(typeof(Plex));
                plexSettings.Enable = data.PlexEnabled;
                _externalNotificationProvider.SaveSettings(plexSettings);

                _configProvider.PlexNotifyOnGrab = data.PlexNotifyOnGrab;
                _configProvider.PlexNotifyOnDownload = data.PlexNotifyOnDownload;
                _configProvider.PlexUpdateLibrary = data.PlexUpdateLibrary;
                _configProvider.PlexServerHost = data.PlexServerHost;
                _configProvider.PlexClientHosts = data.PlexClientHosts;
                _configProvider.PlexUsername = data.PlexUsername;
                _configProvider.PlexPassword = data.PlexPassword;

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        [HttpPost]
        public JsonResult SaveNaming(EpisodeNamingModel data)
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
                _configProvider.SortingUseSceneName = data.SceneName;

                //Metadata
                _configProvider.MetadataUseBanners = data.MetadataUseBanners;
                
                //Xbmc
                var xbmc = _metadataProvider.GetSettings(typeof(Core.Providers.Metadata.Xbmc));
                xbmc.Enable = data.MetadataXbmcEnabled;
                _metadataProvider.SaveSettings(xbmc);

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
                _configProvider.RecycleBin = data.RecycleBin;

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        [HttpPost]
        public JsonResult SaveMisc(MiscSettingsModel data)
        {
            if (ModelState.IsValid)
            {
                _configProvider.EnableBacklogSearching = data.EnableBacklogSearching;
                _configProvider.AutoIgnorePreviouslyDownloadedEpisodes = data.AutoIgnorePreviouslyDownloadedEpisodes;
                _configProvider.AllowedReleaseGroups = data.AllowedReleaseGroups;

                return GetSuccessResult();
            }

            return GetInvalidModelResult();
        }

        private JsonResult GetSuccessResult()
        {
            return JsonNotificationResult.Info("Settings Saved");
        }

        private JsonResult GetInvalidModelResult()
        {
            return JsonNotificationResult.Oops("Invalid post data");
        }

        private SelectList GetProwlPrioritySelectList()
        {
            var list = new List<ProwlPrioritySelectListModel>();
            list.Add(new ProwlPrioritySelectListModel { Name = "Very Low", Value = -2 });
            list.Add(new ProwlPrioritySelectListModel { Name = "Moderate", Value = -1 });
            list.Add(new ProwlPrioritySelectListModel { Name = "Normal", Value = 0 });
            list.Add(new ProwlPrioritySelectListModel { Name = "High", Value = 1 });
            list.Add(new ProwlPrioritySelectListModel { Name = "Emergency", Value = 2 });

            return new SelectList(list, "Value", "Name");
        }
    }
}