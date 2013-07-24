using FluentValidation;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Api.Config
{
    public class NamingModule : NzbDroneRestModule<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;

        public NamingModule(INamingConfigService namingConfigService)
            : base("config/naming")
        {
            _namingConfigService = namingConfigService;
            GetResourceSingle = GetNamingConfig;

            UpdateResource = UpdateNamingConfig;

            SharedValidator.RuleFor(c => c.MultiEpisodeStyle).InclusiveBetween(0, 3);
            SharedValidator.RuleFor(c => c.NumberStyle).InclusiveBetween(0, 3);
            SharedValidator.RuleFor(c => c.Separator).Matches(@"\s|\s\-\s|\.");
        }

        private NamingConfigResource UpdateNamingConfig(NamingConfigResource resource)
        {
            return ToResource<NamingConfig>(_namingConfigService.Save, resource);
        }

        private NamingConfigResource GetNamingConfig()
        {
            return ToResource(_namingConfigService.GetConfig);
        }
    }
}