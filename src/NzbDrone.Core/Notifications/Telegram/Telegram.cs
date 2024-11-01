using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class Telegram : NotificationBase<TelegramSettings>
    {
        private readonly ITelegramProxy _proxy;

        public Telegram(ITelegramProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Telegram";
        public override string Link => "https://telegram.org/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_GRABBED_TITLE_BRANDED : EPISODE_GRABBED_TITLE;
            var links = GetLinks(grabMessage.Series);

            _proxy.SendNotification(title, grabMessage.Message, links, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_DOWNLOADED_TITLE_BRANDED : EPISODE_DOWNLOADED_TITLE;
            var links = GetLinks(message.Series);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_DOWNLOADED_TITLE_BRANDED : EPISODE_DOWNLOADED_TITLE;
            var links = GetLinks(message.Series);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? EPISODE_DELETED_TITLE_BRANDED : EPISODE_DELETED_TITLE;
            var links = GetLinks(deleteMessage.Series);

            _proxy.SendNotification(title, deleteMessage.Message, links, Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? SERIES_ADDED_TITLE_BRANDED : SERIES_ADDED_TITLE;
            var links = GetLinks(message.Series);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? SERIES_DELETED_TITLE_BRANDED : SERIES_DELETED_TITLE;
            var links = GetLinks(deleteMessage.Series);

            _proxy.SendNotification(title, deleteMessage.Message, links, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var title = Settings.IncludeAppNameInTitle ? HEALTH_ISSUE_TITLE_BRANDED : HEALTH_ISSUE_TITLE;

            _proxy.SendNotification(title, healthCheck.Message, new List<TelegramLink>(), Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var title = Settings.IncludeAppNameInTitle ? HEALTH_RESTORED_TITLE_BRANDED : HEALTH_RESTORED_TITLE;

            _proxy.SendNotification(title, $"The following issue is now resolved: {previousCheck.Message}", new List<TelegramLink>(), Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? APPLICATION_UPDATE_TITLE_BRANDED : APPLICATION_UPDATE_TITLE;

            _proxy.SendNotification(title, updateMessage.Message, new List<TelegramLink>(), Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED : MANUAL_INTERACTION_REQUIRED_TITLE;
            var links = GetLinks(message.Series);

            _proxy.SendNotification(title, message.Message, links, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }

        private List<TelegramLink> GetLinks(Series series)
        {
            var links = new List<TelegramLink>();

            if (series == null)
            {
                return links;
            }

            foreach (var link in Settings.MetadataLinks)
            {
                var linkType = (MetadataLinkType)link;

                if (linkType == MetadataLinkType.Imdb && series.ImdbId.IsNotNullOrWhiteSpace())
                {
                    links.Add(new TelegramLink("IMDb", $"https://www.imdb.com/title/{series.ImdbId}"));
                }

                if (linkType == MetadataLinkType.Tvdb && series.TvdbId > 0)
                {
                    links.Add(new TelegramLink("TVDb", $"http://www.thetvdb.com/?tab=series&id={series.TvdbId}"));
                }

                if (linkType == MetadataLinkType.Trakt && series.TvdbId > 0)
                {
                    links.Add(new TelegramLink("Trakt", $"http://trakt.tv/search/tvdb/{series.TvdbId}?id_type=show"));
                }

                if (linkType == MetadataLinkType.Tvmaze && series.TvMazeId > 0)
                {
                    links.Add(new TelegramLink("TVMaze", $"http://www.tvmaze.com/shows/{series.TvMazeId}/_"));
                }
            }

            return links;
        }
    }
}
