using System.Linq;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Kometa;
using NzbDrone.Core.Localization;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IMetadata>))]
    public class MetadataCheck : HealthCheckBase
    {
        private readonly IMetadataFactory _metadataFactory;

        public MetadataCheck(IMetadataFactory metadataFactory, ILocalizationService localizationService)
            : base(localizationService)
        {
            _metadataFactory = metadataFactory;
        }

        public override HealthCheck Check()
        {
            var enabled = _metadataFactory.Enabled();

            if (enabled.Any(m => m.Definition.Implementation == nameof(KometaMetadata)))
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    $"{_localizationService.GetLocalizedString("MetadataKometaDeprecated")}");
            }

            return new HealthCheck(GetType());
        }
    }
}
