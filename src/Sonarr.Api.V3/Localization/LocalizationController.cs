using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Localization;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Localization
{
    [V3ApiController]
    public class LocalizationController : RestController<LocalizationResource>
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        protected override LocalizationResource GetResourceById(int id)
        {
            return GetLocalization();
        }

        [HttpGet]
        [Produces("application/json")]
        public LocalizationResource GetLocalization()
        {
            return _localizationService.GetLocalizationDictionary().ToResource();
        }
    }
}
