using NzbDrone.Common.Serializer;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationSettingsProvider
    {
        TSetting Get<TSetting>(INotification indexer) where TSetting : IProviderConfig, new();
    }

    public class NotificationSettingsProvider : INotificationSettingsProvider
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationSettingsProvider(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public TSetting Get<TSetting>(INotification indexer) where TSetting : IProviderConfig, new()
        {
            var indexerDef = _notificationRepository.Find(indexer.Name);

            if (indexerDef == null || string.IsNullOrWhiteSpace(indexerDef.Settings))
            {
                return new TSetting();
            }

            return Json.Deserialize<TSetting>(indexerDef.Settings);
        }
    }
}