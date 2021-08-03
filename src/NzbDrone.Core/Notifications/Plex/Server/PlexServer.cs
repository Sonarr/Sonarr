using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Notifications.Plex.PlexTv;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexServerService _plexServerService;
        private readonly IPlexTvService _plexTvService;
        private readonly Logger _logger;

        private class PlexUpdateQueue
        {
            public Dictionary<int, Series> Pending { get; } = new Dictionary<int, Series>();
            public bool Refreshing { get; set; }
        }

        private readonly ICached<PlexUpdateQueue> _pendingSeriesCache;

        public PlexServer(IPlexServerService plexServerService, IPlexTvService plexTvService, ICacheManager cacheManager, Logger logger)
        {
            _plexServerService = plexServerService;
            _plexTvService = plexTvService;
            _logger = logger;

            _pendingSeriesCache = cacheManager.GetRollingCache<PlexUpdateQueue>(GetType(), "pendingSeries", TimeSpan.FromDays(1));
        }

        public override string Link => "https://www.plex.tv/";
        public override string Name => "Plex Media Server";

        public override void OnDownload(DownloadMessage message)
        {
            UpdateIfEnabled(message.Series);
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            UpdateIfEnabled(series);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            UpdateIfEnabled(deleteMessage.Series);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            if (deleteMessage.DeletedFiles)
            {
                UpdateIfEnabled(deleteMessage.Series);
            }
        }

        private void UpdateIfEnabled(Series series)
        {
            if (Settings.UpdateLibrary)
            {
                _logger.Debug("Scheduling library update for series {0} {1}", series.Id, series.Title);
                var queue = _pendingSeriesCache.Get(Settings.Host, () => new PlexUpdateQueue());
                lock (queue)
                {
                    queue.Pending[series.Id] = series;
                }
            }
        }

        public override void ProcessQueue()
        {
            PlexUpdateQueue queue = _pendingSeriesCache.Find(Settings.Host);
            if (queue == null)
            {
                return;
            }

            lock (queue)
            {
                if (queue.Refreshing)
                {
                    return;
                }

                queue.Refreshing = true;
            }

            try
            {
                while (true)
                {
                    List<Series> refreshingSeries;
                    lock (queue)
                    {
                        if (queue.Pending.Empty())
                        {
                            queue.Refreshing = false;
                            return;
                        }

                        refreshingSeries = queue.Pending.Values.ToList();
                        queue.Pending.Clear();
                    }

                    if (Settings.UpdateLibrary)
                    {
                        _logger.Debug("Performing library update for {0} series", refreshingSeries.Count);
                        _plexServerService.UpdateLibrary(refreshingSeries, Settings);
                    }
                }
            }
            catch
            {
                lock (queue)
                {
                    queue.Refreshing = false;
                }

                throw;
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexServerService.Test(Settings));

            return new ValidationResult(failures);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                return _plexTvService.GetPinUrl();
            }
            else if (action == "continueOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["callbackUrl"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam callbackUrl invalid.");
                }

                if (query["id"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam id invalid.");
                }

                if (query["code"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam code invalid.");
                }

                return _plexTvService.GetSignInUrl(query["callbackUrl"], Convert.ToInt32(query["id"]), query["code"]);
            }
            else if (action == "getOAuthToken")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["pinId"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam pinId invalid.");
                }

                var authToken = _plexTvService.GetAuthToken(Convert.ToInt32(query["pinId"]));

                return new
                       {
                           authToken
                       };
            }

            return new { };
        }
    }
}
