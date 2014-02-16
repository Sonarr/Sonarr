using System.Linq;
using System.Reflection;
using FluentValidation;
using NzbDrone.Core.Configuration;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Config
{
    public class HostConfigModule : NzbDroneRestModule<HostConfigResource>
    {
        private readonly IConfigFileProvider _configFileProvider;

        public HostConfigModule(ConfigFileProvider configFileProvider)
            : base("/config/host")
        {
            _configFileProvider = configFileProvider;

            GetResourceSingle = GetHostConfig;
            GetResourceById = GetHostConfig;
            UpdateResource = SaveHostConfig;

            SharedValidator.RuleFor(c => c.Branch).NotEmpty().WithMessage("Branch name is required, 'master' is the default");
            SharedValidator.RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            
            SharedValidator.RuleFor(c => c.Username).NotEmpty().When(c => c.AuthenticationEnabled);
            SharedValidator.RuleFor(c => c.Password).NotEmpty().When(c => c.AuthenticationEnabled);

            SharedValidator.RuleFor(c => c.SslPort).InclusiveBetween(1, 65535).When(c => c.EnableSsl);
            SharedValidator.RuleFor(c => c.SslCertHash).NotEmpty().When(c => c.EnableSsl);
        }

        private HostConfigResource GetHostConfig()
        {
            var resource = new HostConfigResource();
            resource.InjectFrom(_configFileProvider);
            resource.Id = 1;

            return resource;
        }

        private HostConfigResource GetHostConfig(int id)
        {
            return GetHostConfig();
        }

        private void SaveHostConfig(HostConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configFileProvider.SaveConfigDictionary(dictionary);
        }
    }
}