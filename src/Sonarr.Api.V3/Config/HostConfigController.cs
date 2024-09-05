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
using Sonarr.Http.Authentication;
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

        private readonly IAuthenticationService _authService;

        public HostConfigController(IConfigFileProvider configFileProvider,
                                    IConfigService configService,
                                    IUserService userService,
                                    FileExistsValidator fileExistsValidator,
                                    IAuthenticationService authService)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _userService = userService;
            _authService = authService;

            SharedValidator.RuleFor(c => c.BindAddress)
                           .ValidIpAddress()
                           .NotListenAllIp4Address()
                           .When(c => c.BindAddress != "*" && c.BindAddress != "localhost");

            SharedValidator.RuleFor(c => c.Port).ValidPort();

            SharedValidator.RuleFor(c => c.UrlBase).ValidUrlBase();
            SharedValidator.RuleFor(c => c.InstanceName).StartsOrEndsWithSonarr();

            SharedValidator.RuleFor(c => c.Username).NotEmpty().When(c => c.AuthenticationMethod == AuthenticationType.Basic ||
                                                                          c.AuthenticationMethod == AuthenticationType.Forms);
            SharedValidator.RuleFor(c => c.Password).NotEmpty().When(c => c.AuthenticationMethod == AuthenticationType.Basic ||
                                                                          c.AuthenticationMethod == AuthenticationType.Forms);

            SharedValidator.RuleFor(c => c.PasswordConfirmation)
                .Must((resource, p) => IsMatchingPassword(resource)).WithMessage("Must match Password");

            SharedValidator.RuleFor(c => c.SslPort).ValidPort().When(c => c.EnableSsl);
            SharedValidator.RuleFor(c => c.SslPort).NotEqual(c => c.Port).When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.SslCertPath)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(fileExistsValidator)
                .Must((resource, path) => IsValidSslCertificate(resource)).WithMessage("Invalid SSL certificate file or password")
                .When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.LogSizeLimit).InclusiveBetween(1, 10);

            SharedValidator.RuleFor(c => c.Branch).NotEmpty().WithMessage("Branch name is required, 'main' is the default");
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

        private bool IsMatchingPassword(HostConfigResource resource)
        {
            var user = _userService.FindUser();

            if (user != null && user.Password == resource.Password)
            {
                return true;
            }

            if (resource.Password == resource.PasswordConfirmation)
            {
                return true;
            }

            return false;
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

            resource.Username = user?.Username ?? string.Empty;
            resource.Password = user?.Password ?? string.Empty;
            resource.PasswordConfirmation = string.Empty;

            return resource;
        }

        [RestPutById]
        public ActionResult<HostConfigResource> SaveHostConfig([FromBody] HostConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            if (resource.Username.IsNotNullOrWhiteSpace() && resource.Password.IsNotNullOrWhiteSpace())
            {
                var user = _userService.Upsert(resource.Username, resource.Password);
                _authService.SignInUser(HttpContext, user, true);
            }

            return Accepted(resource.Id);
        }
    }
}
