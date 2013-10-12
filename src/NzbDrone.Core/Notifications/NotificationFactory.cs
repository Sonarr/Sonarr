using System.Collections.Generic;
using NLog;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationFactory : IProviderFactory<INotification, NotificationDefinition>
    {

    }

    public class NotificationFactory : ProviderFactory<INotification, NotificationDefinition>, INotificationFactory
    {
        public NotificationFactory(INotificationRepository providerRepository, IEnumerable<INotification> providers, Logger logger)
            : base(providerRepository, providers, logger)
        {

        }
    }
}