using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
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
        private readonly MediaServerUpdateQueue<PlexServer, bool> _updateQueue;
        private readonly Logger _logger;

        public PlexServer(IPlexServerService plexServerService, IPlexTvService plexTvService, ICacheManager cacheManager, Logger logger)
        {
            _plexServerService = plexServerService;
            _plexTvService = plexTvService;
            _updateQueue = new MediaServerUpdateQueue<PlexServer, bool>(cacheManager);
            _logger = logger;
        }

        public override string Link => "https://www.plex.tv/";
        public override string Name => "Plex Media Server";

        public override void OnDownload(DownloadMessage message)
        {
            UpdateIfEnabled(message.Series);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
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

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            UpdateIfEnabled(message.Series);
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
            _plexTvService.Ping(Settings.AuthToken);

            if (Settings.UpdateLibrary)
            {
                _logger.Debug("Scheduling library update for series {0} {1}", series.Id, series.Title);
                _updateQueue.Add(Settings.Host, series, false);
            }
        }

        public override void ProcessQueue()
        {
            _updateQueue.ProcessQueue(Settings.Host, (items) =>
            {
                if (Settings.UpdateLibrary)
                {
                    _logger.Debug("Performing library update for {0} series", items.Count);
                    _plexServerService.UpdateLibrary(items.Select(i => i.Series), Settings);
                }
            });
        }

        public override ValidationResult Test()
        {
            _plexTvService.Ping(Settings.AuthToken);

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

            if (action == "servers")
            {
                Settings.Validate().Filter("AuthToken").ThrowOnError();

                if (Settings.AuthToken.IsNullOrWhiteSpace())
                {
                    return new { };
                }

                var servers = _plexTvService.GetServers(Settings.AuthToken);
                var options = servers.SelectMany(s =>
                {
                    var result = new List<FieldSelectStringOption>();

                    s.Connections.ForEach(c =>
                    {
                        var isSecure = c.Protocol == "https";
                        var additionalProperties = new Dictionary<string, object>();
                        var hints = new List<string>();

                        additionalProperties.Add("host", c.Host);
                        additionalProperties.Add("port", c.Port);
                        additionalProperties.Add("useSsl", isSecure);
                        hints.Add(c.Local ? "Local" : "Remote");

                        if (isSecure)
                        {
                            hints.Add("Secure");
                        }

                        result.Add(new FieldSelectStringOption
                        {
                            Value = c.Uri,
                            Name = $"{s.Name} ({c.Host})",
                            Hint = string.Join(", ", hints),
                            AdditionalProperties = additionalProperties
                        });

                        if (isSecure)
                        {
                            var uri = $"http://{c.Address}:{c.Port}";
                            var insecureAdditionalProperties = new Dictionary<string, object>();

                            insecureAdditionalProperties.Add("host", c.Address);
                            insecureAdditionalProperties.Add("port", c.Port);
                            insecureAdditionalProperties.Add("useSsl", false);

                            result.Add(new FieldSelectStringOption
                            {
                                Value = uri,
                                Name = $"{s.Name} ({c.Address})",
                                Hint = c.Local ? "Local" : "Remote",
                                AdditionalProperties = insecureAdditionalProperties
                            });
                        }
                    });

                    return result;
                });

                return new
                {
                    options
                };
            }

            return new { };
        }
    }
}
