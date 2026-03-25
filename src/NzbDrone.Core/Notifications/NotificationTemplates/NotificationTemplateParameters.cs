namespace NzbDrone.Core.Notifications.NotificationTemplates
{
    public class NotificationTemplateParameters
    {
        public NotificationTemplateParameters()
        {
        }

        public string FallbackTitle { get; set; }
        public string FallbackBody { get; set; }
        public GrabMessage GrabMessage { get; set; }
        public SeriesAddMessage SeriesAddMessage { get; set; }
        public EpisodeDeleteMessage EpisodeDeleteMessage { get; set; }
        public SeriesDeleteMessage SeriesDeleteMessage { get; set; }
        public ImportCompleteMessage ImportCompleteMessage { get; set; }
        public DownloadMessage DownloadMessage { get; set; }
        public HealthCheck.HealthCheck HealthCheck { get; set; }
        public ApplicationUpdateMessage ApplicationUpdateMessage { get; set; }
        public ManualInteractionRequiredMessage ManualInteractionRequiredMessage { get; set; }
    }
}
