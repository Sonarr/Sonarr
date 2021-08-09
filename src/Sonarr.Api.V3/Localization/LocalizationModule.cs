using NzbDrone.Core.Localization;
using Sonarr.Http;

namespace Sonarr.Api.V3.Localization
{
    public class LocalizationModule : SonarrRestModule<LocalizationResource>
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationModule(ILocalizationService localizationService)
        {
            _localizationService = localizationService;

            Get("/", x => GetLocalizationDictionary());
        }

        private LocalizationResource GetLocalizationDictionary()
        {
            return _localizationService.GetLocalizationDictionary().ToResource();
        }
    }
}
