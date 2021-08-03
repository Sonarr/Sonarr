using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationFactory : IProviderFactory<INotification, NotificationDefinition>
    {
        List<INotification> OnGrabEnabled();
        List<INotification> OnDownloadEnabled();
        List<INotification> OnUpgradeEnabled();
        List<INotification> OnRenameEnabled();
        List<INotification> OnSeriesDeleteEnabled();
        List<INotification> OnEpisodeFileDeleteEnabled();
        List<INotification> OnEpisodeFileDeleteForUpgradeEnabled();
        List<INotification> OnHealthIssueEnabled();
        List<INotification> OnApplicationUpdateEnabled();
    }

    public class NotificationFactory : ProviderFactory<INotification, NotificationDefinition>, INotificationFactory
    {
        public NotificationFactory(INotificationRepository providerRepository, IEnumerable<INotification> providers, IContainer container, IEventAggregator eventAggregator, Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
        }

        public List<INotification> OnGrabEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnGrab).ToList();
        }

        public List<INotification> OnDownloadEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnDownload).ToList();
        }

        public List<INotification> OnUpgradeEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnUpgrade).ToList();
        }

        public List<INotification> OnRenameEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnRename).ToList();
        }

        public List<INotification> OnSeriesDeleteEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnSeriesDelete).ToList();
        }

        public List<INotification> OnEpisodeFileDeleteEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnEpisodeFileDelete).ToList();
        }

        public List<INotification> OnEpisodeFileDeleteForUpgradeEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnEpisodeFileDeleteForUpgrade).ToList();
        }

        public List<INotification> OnHealthIssueEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnHealthIssue).ToList();
        }

        public List<INotification> OnApplicationUpdateEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnApplicationUpdate).ToList();
        }

        public override void SetProviderCharacteristics(INotification provider, NotificationDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);

            definition.SupportsOnGrab = provider.SupportsOnGrab;
            definition.SupportsOnDownload = provider.SupportsOnDownload;
            definition.SupportsOnUpgrade = provider.SupportsOnUpgrade;
            definition.SupportsOnRename = provider.SupportsOnRename;
            definition.SupportsOnSeriesDelete = provider.SupportsOnSeriesDelete;
            definition.SupportsOnEpisodeFileDelete = provider.SupportsOnEpisodeFileDelete;
            definition.SupportsOnEpisodeFileDeleteForUpgrade = provider.SupportsOnEpisodeFileDeleteForUpgrade;
            definition.SupportsOnHealthIssue = provider.SupportsOnHealthIssue;
            definition.SupportsOnApplicationUpdate = provider.SupportsOnApplicationUpdate;
        }
    }
}
