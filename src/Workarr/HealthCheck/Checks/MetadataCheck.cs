using Workarr.Extras.Metadata;
using Workarr.Extras.Metadata.Consumers.Kometa;
using Workarr.Localization;
using Workarr.ThingiProvider.Events;

namespace Workarr.HealthCheck.Checks
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
