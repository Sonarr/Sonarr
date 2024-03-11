using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class Telegram : NotificationBase<TelegramSettings>
    {
        private const string ImdbUrlFormat = "https://www.imdb.com/title/{0}";
        private const string TvdbUrlFormat = "http://www.thetvdb.com/?tab=series&id={0}";
        private const string TraktUrlFormat = "http://trakt.tv/search/tvdb/{0}?id_type=show";
        private const string TvMazeUrlFormat = "http://www.tvmaze.com/shows/{0}/_";

        private readonly ITelegramProxy _proxy;
        private readonly Dictionary<MetadataLinkType, Func<string, string, string>> _formatLinkFromIdMethods;

        private string FormatImdbLinkFromId(string message, string id)
        {
            return $"[{message}]({string.Format(ImdbUrlFormat, id)})";
        }

        private string FormatTvdbLinkFromId(string message, string id)
        {
            return $"[{message}]({string.Format(TvdbUrlFormat, id)})";
        }

        private string FormatTraktLinkFromId(string message, string id)
        {
            return $"[{message}]({string.Format(TraktUrlFormat, id)})";
        }

        private string FormatTVMazeLinkFromId(string message, string id)
        {
            return $"[{message}]({string.Format(TvMazeUrlFormat, id)})";
        }

        private string GetIdByType(Series series, MetadataLinkType linkType)
        {
            switch (linkType)
            {
                case MetadataLinkType.Imdb:
                    return series.ImdbId;
                case MetadataLinkType.Tvdb:
                    return series.TvdbId.ToString();
                case MetadataLinkType.Trakt:
                    return series.TvdbId.ToString();
                case MetadataLinkType.Tvmaze:
                    return series.TvMazeId.ToString();
                default:
                    throw new ArgumentException($"Unsupported link type: {linkType}", nameof(linkType));
            }
        }

        public Telegram(ITelegramProxy proxy)
        {
            _proxy = proxy;
            _formatLinkFromIdMethods = new Dictionary<MetadataLinkType, Func<string, string, string>>
            {
                { MetadataLinkType.Imdb, FormatImdbLinkFromId },
                { MetadataLinkType.Tvdb, FormatTvdbLinkFromId },
                { MetadataLinkType.Trakt, FormatTraktLinkFromId },
                { MetadataLinkType.Tvmaze, FormatTVMazeLinkFromId }
            };
        }

        private string FormatMessageWithLink(string message, Series series)
        {
            if (Settings.SendMetadataLink && _formatLinkFromIdMethods.TryGetValue(Settings.MetadataLinkType, out var formatMethod))
            {
                message = formatMethod(message, GetIdByType(series, Settings.MetadataLinkType));
            }

            return message;
        }

        public override string Name => "Telegram";
        public override string Link => "https://telegram.org/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(EPISODE_GRABBED_TITLE, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendNotification(EPISODE_DOWNLOADED_TITLE, message.Message, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(EPISODE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var text = FormatMessageWithLink(message.Message, message.Series);
            _proxy.SendNotification(SERIES_ADDED_TITLE, text, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var text = FormatMessageWithLink(deleteMessage.Message, deleteMessage.Series);
            _proxy.SendNotification(SERIES_DELETED_TITLE, text, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            _proxy.SendNotification(HEALTH_RESTORED_TITLE, $"The following issue is now resolved: {previousCheck.Message}", Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE, updateMessage.Message, Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            _proxy.SendNotification(MANUAL_INTERACTION_REQUIRED_TITLE, message.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
