using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<INotification>))]
    [CheckOn(typeof(ProviderDeletedEvent<INotification>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<INotification>))]
    public class NotificationStatusCheck : HealthCheckBase
    {
        private readonly INotificationFactory _providerFactory;
        private readonly INotificationStatusService _providerStatusService;

        public NotificationStatusCheck(INotificationFactory providerFactory, INotificationStatusService providerStatusService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _providerFactory = providerFactory;
            _providerStatusService = providerStatusService;
        }

        public override HealthCheck Check()
        {
            var enabledProviders = _providerFactory.GetAvailableProviders();
            var backOffProviders = enabledProviders.Join(_providerStatusService.GetBlockedProviders(),
                    i => i.Definition.Id,
                    s => s.ProviderId,
                    (i, s) => new { Provider = i, Status = s })
                .ToList();

            if (backOffProviders.Empty())
            {
                return new HealthCheck(GetType());
            }

            if (backOffProviders.Count == enabledProviders.Count)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("NotificationStatusAllClientHealthCheckMessage"),
                    "#notifications-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Warning,
                string.Format(_localizationService.GetLocalizedString("NotificationStatusSingleClientHealthCheckMessage"), string.Join(", ", backOffProviders.Select(v => v.Provider.Definition.Name))),
                "#notifications-are-unavailable-due-to-failures");
        }
    }
}
