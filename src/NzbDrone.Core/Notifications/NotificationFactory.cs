using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationFactory : IProviderFactory<INotification, NotificationDefinition>
    {
        List<INotification> OnGrabEnabled();
        List<INotification> OnDownloadEnabled();
    }

    public class NotificationFactory : ProviderFactory<INotification, NotificationDefinition>, INotificationFactory
    {
        private IEnumerable<INotification> _providers;

        public NotificationFactory(INotificationRepository providerRepository, IEnumerable<INotification> providers, IContainer container, Logger logger)
            : base(providerRepository, providers, container, logger)
        {
            _providers = providers;
        }

        public List<INotification> OnGrabEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnGrab).ToList();
        }

        public List<INotification> OnDownloadEnabled()
        {
            return GetAvailableProviders().Where(n => ((NotificationDefinition)n.Definition).OnDownload).ToList();
        }
    }
}