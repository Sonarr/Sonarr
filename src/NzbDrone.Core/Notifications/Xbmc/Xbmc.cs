using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class Xbmc : NotificationBase<XbmcSettings>
    {
        private readonly IXbmcService _xbmcService;
        private readonly MediaServerUpdateQueue<Xbmc, bool> _updateQueue;
        private readonly Logger _logger;

        public Xbmc(IXbmcService xbmcService, ICacheManager cacheManager, Logger logger)
        {
            _xbmcService = xbmcService;
            _updateQueue = new MediaServerUpdateQueue<Xbmc, bool>(cacheManager);
            _logger = logger;
        }

        public override string Link => "https://kodi.tv/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string header = "Sonarr - Grabbed";

            Notify(Settings, header, grabMessage.Message);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string header = "Sonarr - Downloaded";

            Notify(Settings, header, message.Message);
            UpdateAndClean(message.Series, message.OldFiles.Any());
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            const string header = "Sonarr - Imported";

            Notify(Settings, header, message.Message);
            UpdateAndClean(message.Series);
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            UpdateAndClean(series);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            const string header = "Sonarr - Deleted";

            Notify(Settings, header, deleteMessage.Message);
            UpdateAndClean(deleteMessage.Series, true);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            const string header = "Sonarr - Added";

            Notify(Settings, header, message.Message);
            UpdateAndClean(message.Series, true);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            if (deleteMessage.DeletedFiles)
            {
                const string header = "Sonarr - Deleted";

                Notify(Settings, header, deleteMessage.Message);
                UpdateAndClean(deleteMessage.Series, true);
            }
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            Notify(Settings, HEALTH_ISSUE_TITLE_BRANDED, healthCheck.Message);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            Notify(Settings, HEALTH_RESTORED_TITLE_BRANDED, $"The following issue is now resolved: {previousCheck.Message}");
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            Notify(Settings, APPLICATION_UPDATE_TITLE_BRANDED, updateMessage.Message);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            Notify(Settings, MANUAL_INTERACTION_REQUIRED_TITLE, message.Message);
        }

        public override string Name => "Kodi";

        public override void ProcessQueue()
        {
            _updateQueue.ProcessQueue(Settings.Host, (items) =>
            {
                _logger.Debug("Performing library update for {0} series", items.Count);

                items.ForEach(item =>
                {
                    try
                    {
                        if (Settings.UpdateLibrary)
                        {
                            _xbmcService.Update(Settings, item.Series);
                        }

                        if (item.Info.Contains(true) && Settings.CleanLibrary)
                        {
                            _xbmcService.Clean(Settings);
                        }
                    }
                    catch (SocketException ex)
                    {
                        var logMessage = string.Format("Unable to connect to Kodi Host: {0}:{1}", Settings.Host, Settings.Port);
                        _logger.Debug(ex, logMessage);
                    }
                });
            });
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_xbmcService.Test(Settings, "Success! Kodi has been successfully configured!"));

            return new ValidationResult(failures);
        }

        private void Notify(XbmcSettings settings, string header, string message)
        {
            try
            {
                if (Settings.Notify)
                {
                    _xbmcService.Notify(Settings, header, message);
                }
            }
            catch (SocketException ex)
            {
                var logMessage = string.Format("Unable to connect to Kodi Host: {0}:{1}", Settings.Host, Settings.Port);
                _logger.Debug(ex, logMessage);
            }
        }

        private void UpdateAndClean(Series series, bool clean = true)
        {
            if (Settings.UpdateLibrary || Settings.CleanLibrary)
            {
                _logger.Debug("Scheduling library update for series {0} {1}", series.Id, series.Title);
                _updateQueue.Add(Settings.Host, series, clean);
            }
        }
    }
}
