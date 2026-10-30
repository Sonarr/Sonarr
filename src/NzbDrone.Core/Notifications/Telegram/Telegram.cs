using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class Telegram : NotificationBase<TelegramSettings>
    {
        private readonly ITelegramProxy _proxy;
        private readonly IConfigFileProvider _configFileProvider;

        public Telegram(ITelegramProxy proxy, IConfigFileProvider configFileProvider)
        {
            _proxy = proxy;
            _configFileProvider = configFileProvider;
        }

        public override string Name => "Telegram";
        public override string Link => "https://telegram.org/";

        private string InstanceName => _configFileProvider.InstanceName;

        public override void OnGrab(GrabMessage grabMessage)
        {
            var title = GetTitle(EPISODE_GRABBED_TITLE, EPISODE_GRABBED_TITLE_BRANDED);
            var links = GetLinks(grabMessage.Series);

            _proxy.SendNotification(
                BuildEpisodeMessage(title, grabMessage.Message, grabMessage.Series, grabMessage.Episode?.Episodes, grabMessage.Quality, grabMessage.Episode?.Release?.Size, links),
                links,
                Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var title = GetTitle(EPISODE_DOWNLOADED_TITLE, EPISODE_DOWNLOADED_TITLE_BRANDED);
            var links = GetLinks(message.Series);

            _proxy.SendNotification(
                BuildEpisodeMessage(title, message.Message, message.Series, message.EpisodeInfo?.Episodes, message.EpisodeFile?.Quality ?? message.EpisodeInfo?.Quality, message.EpisodeFile?.Size, links),
                links,
                Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            var title = GetTitle(EPISODE_DOWNLOADED_TITLE, EPISODE_DOWNLOADED_TITLE_BRANDED);
            var links = GetLinks(message.Series);
            var size = message.EpisodeFiles?.Sum(f => f.Size);

            _proxy.SendNotification(
                BuildEpisodeMessage(title, message.Message, message.Series, message.Episodes, message.ReleaseQuality, size, links),
                links,
                Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var title = GetTitle(EPISODE_DELETED_TITLE, EPISODE_DELETED_TITLE_BRANDED);
            var links = GetLinks(deleteMessage.Series);
            var episodeFile = deleteMessage.EpisodeFile;

            _proxy.SendNotification(
                BuildEpisodeMessage(title, deleteMessage.Message, deleteMessage.Series, episodeFile?.Episodes?.Value, episodeFile?.Quality, episodeFile?.Size, links),
                links,
                Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var title = GetTitle(SERIES_ADDED_TITLE, SERIES_ADDED_TITLE_BRANDED);
            var links = GetLinks(message.Series);

            _proxy.SendNotification(BuildSeriesMessage(title, message.Message, message.Series, links), links, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var title = GetTitle(SERIES_DELETED_TITLE, SERIES_DELETED_TITLE_BRANDED);
            var links = GetLinks(deleteMessage.Series);

            _proxy.SendNotification(BuildSeriesMessage(title, deleteMessage.Message, deleteMessage.Series, links), links, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var title = GetTitle(HEALTH_ISSUE_TITLE, HEALTH_ISSUE_TITLE_BRANDED);

            SendSimpleNotification(title, healthCheck.Message);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var title = GetTitle(HEALTH_RESTORED_TITLE, HEALTH_RESTORED_TITLE_BRANDED);

            SendSimpleNotification(title, $"The following issue is now resolved: {previousCheck.Message}");
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var title = GetTitle(APPLICATION_UPDATE_TITLE, APPLICATION_UPDATE_TITLE_BRANDED);

            SendSimpleNotification(title, updateMessage.Message);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var title = GetTitle(MANUAL_INTERACTION_REQUIRED_TITLE, MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED);
            var links = GetLinks(message.Series);

            _proxy.SendNotification(
                BuildEpisodeMessage(title, message.Message, message.Series, message.Episode?.Episodes, message.Quality, message.Release?.Size, links),
                links,
                Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }

        private string GetTitle(string title, string brandedTitle)
        {
            var result = Settings.IncludeAppNameInTitle ? brandedTitle : title;

            return Settings.IncludeInstanceNameInTitle ? $"{result} - {InstanceName}" : result;
        }

        private List<NotificationMetadataLink> GetLinks(Series series)
        {
            return NotificationMetadataLinkGenerator.GenerateLinks(series, Settings.MetadataLinks);
        }

        private void SendSimpleNotification(string title, string message)
        {
            var links = new List<NotificationMetadataLink>();

            _proxy.SendNotification(TelegramMessageBuilder.BuildSimpleMessage(title, message, links), links, Settings);
        }

        private TelegramMessage BuildEpisodeMessage(string title, string message, Series series, List<Episode> episodes, QualityModel quality, long? size, List<NotificationMetadataLink> links)
        {
            if (!Settings.DetailedMessage || series == null || episodes.Empty())
            {
                return TelegramMessageBuilder.BuildSimpleMessage(title, message, links);
            }

            return TelegramMessageBuilder.BuildEpisodeMessage(title, series, episodes, quality, size, links, Settings);
        }

        private TelegramMessage BuildSeriesMessage(string title, string message, Series series, List<NotificationMetadataLink> links)
        {
            if (!Settings.DetailedMessage || series == null)
            {
                return TelegramMessageBuilder.BuildSimpleMessage(title, message, links);
            }

            return TelegramMessageBuilder.BuildSeriesMessage(title, series, links, Settings);
        }
    }
}
