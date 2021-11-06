using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/host")]
    public class HostConfigController : RestController<HostConfigResource>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IUserService _userService;

        public HostConfigController(IConfigFileProvider configFileProvider,
                                    IConfigService configService,
                                    IUserService userService,
                                    FileExistsValidator fileExistsValidator)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _userService = userService;

            SharedValidator.RuleFor(c => c.BindAddress)
                           .ValidIp4Address()
                           .NotListenAllIp4Address()
                           .When(c => c.BindAddress != "*");

            SharedValidator.RuleFor(c => c.Port).ValidPort();

            SharedValidator.RuleFor(c => c.UrlBase).ValidUrlBase();

            SharedValidator.RuleFor(c => c.Username).NotEmpty().When(c => c.AuthenticationMethod != AuthenticationType.None);
            SharedValidator.RuleFor(c => c.Password).NotEmpty().When(c => c.AuthenticationMethod != AuthenticationType.None);

            SharedValidator.RuleFor(c => c.SslPort).ValidPort().When(c => c.EnableSsl);
            SharedValidator.RuleFor(c => c.SslPort).NotEqual(c => c.Port).When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.SslCertPath)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(fileExistsValidator)
                .Must((resource, path) => IsValidSslCertificate(resource)).WithMessage("Invalid SSL certificate file or password")
                .When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.Branch).NotEmpty().WithMessage("Branch name is required, 'master' is the default");
            SharedValidator.RuleFor(c => c.UpdateScriptPath).IsValidPath().When(c => c.UpdateMechanism == UpdateMechanism.Script);

            SharedValidator.RuleFor(c => c.BackupFolder).IsValidPath().When(c => Path.IsPathRooted(c.BackupFolder));
            SharedValidator.RuleFor(c => c.BackupInterval).InclusiveBetween(1, 7);
            SharedValidator.RuleFor(c => c.BackupRetention).InclusiveBetween(1, 90);
        }

        private bool IsValidSslCertificate(HostConfigResource resource)
        {
            X509Certificate2 cert;
            try
            {
                cert = new X509Certificate2(resource.SslCertPath, resource.SslCertPassword, X509KeyStorageFlags.DefaultKeySet);
            }
            catch
            {
                return false;
            }

            return cert != null;
        }

        protected override HostConfigResource GetResourceById(int id)
        {
            return GetHostConfig();
        }

        [HttpGet]
        public HostConfigResource GetHostConfig()
        {
            var resource = _configFileProvider.ToResource(_configService);
            resource.Id = 1;

            var user = _userService.FindUser();
            if (user != null)
            {
                resource.Username = user.Username;
                resource.Password = user.Password;
            }

            return resource;
        }

        [RestPutById]
        public ActionResult<HostConfigResource> SaveHostConfig(HostConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            if (resource.Username.IsNotNullOrWhiteSpace() && resource.Password.IsNotNullOrWhiteSpace())
            {
                _userService.Upsert(resource.Username, resource.Password);
            }

            return Accepted(resource.Id);
        }
    }
}
