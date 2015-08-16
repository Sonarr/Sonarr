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
        List<INotification> OnGrabMovieEnabled();
        List<INotification> OnDownloadEnabled();
        List<INotification> OnDownloadMovieEnabled();
        List<INotification> OnUpgradeEnabled();
        List<INotification> OnRenameEnabled();
        List<INotification> OnRenameMovieEnabled();
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

        public List<INotification> OnGrabMovieEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnGrabMovie).ToList();
        }

        public List<INotification> OnDownloadEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnDownload).ToList();
        }

        public List<INotification> OnDownloadMovieEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnDownloadMovie).ToList();
        }

        public List<INotification> OnUpgradeEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnUpgrade).ToList();
        }

        public List<INotification> OnRenameEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnRename).ToList();
        }

        public List<INotification> OnRenameMovieEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnRenameMovie).ToList();
        }

        public override NotificationDefinition GetProviderCharacteristics(INotification provider, NotificationDefinition definition)
        {
            definition = base.GetProviderCharacteristics(provider, definition);

            definition.SupportsOnGrab = provider.SupportsOnGrab;
            definition.SupportsOnDownload = provider.SupportsOnDownload;
            definition.SupportsOnDownloadMovie = provider.SupportsOnDownloadMovie;
            definition.SupportsOnUpgrade = provider.SupportsOnUpgrade;
            definition.SupportsOnRename = provider.SupportsOnRename;
            definition.SupportsOnRenameMovie = provider.SupportsOnRenameMovie;

            return definition;
        }
    }
}