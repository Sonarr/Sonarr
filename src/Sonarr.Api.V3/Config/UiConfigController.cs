using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using Sonarr.Http;
using Sonarr.Http.Authentication;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/ui")]
    public class UiConfigController : ConfigController<UiConfigResource>
    {
        private readonly IConfigFileProvider _configFileProvider;

        public UiConfigController(IConfigFileProvider configFileProvider, IConfigService configService)
            : base(configService)
        {
            _configFileProvider = configFileProvider;
            SharedValidator.RuleFor(c => c.UILanguage).Custom((value, context) =>
            {
                if (!Language.All.Any(o => o.Id == value))
                {
                    context.AddFailure("Invalid UI Language value");
                }
            });

            SharedValidator.RuleFor(c => c.UILanguage)
                           .GreaterThanOrEqualTo(1)
                           .WithMessage("The UI Language value cannot be less than 1");
        }

        [HttpGet]
        [Produces("application/json")]
        [RequiresPermission(SonarrRole.ReadOnly)]
        public override UiConfigResource GetConfig()
        {
            return base.GetConfig();
        }

        [RestPutById]
        [RequiresPermission(SonarrRole.Admin)]
        public override ActionResult<UiConfigResource> SaveConfig([FromBody] UiConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            return Accepted(resource.Id);
        }

        protected override UiConfigResource ToResource(IConfigService model)
        {
            return UiConfigResourceMapper.ToResource(_configFileProvider, model);
        }
    }
}
