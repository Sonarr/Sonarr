using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Options;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Authentication
{
    public class InitialUserService : IHandle<ApplicationStartedEvent>
    {
        private const string DefaultUsername = "sonarr";
        private const int GeneratedPasswordLength = 20;

        private readonly IUserService _userService;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;
        private readonly AuthOptions _authOptions;
        private readonly Logger _logger;

        private static readonly Logger _authLogger = LogManager.GetLogger("Auth");

        public InitialUserService(IUserService userService,
                                  IConfigFileProvider configFileProvider,
                                  IAppFolderInfo appFolderInfo,
                                  IDiskProvider diskProvider,
                                  IOptions<AuthOptions> authOptions,
                                  Logger logger)
        {
            _userService = userService;
            _configFileProvider = configFileProvider;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            _authOptions = authOptions.Value;
            _logger = logger;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            try
            {
                CreateInitialUser();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create the initial user, sign in will not be possible until a user is created");
            }
        }

        private void CreateInitialUser()
        {
            if (_userService.FindUser() != null)
            {
                return;
            }

            if (MigrateFromConfigFile())
            {
                return;
            }

            if (_configFileProvider.AuthenticationMethod != AuthenticationType.Forms)
            {
                return;
            }

            var username = _authOptions.InitialUsername.IsNullOrWhiteSpace() ? DefaultUsername : _authOptions.InitialUsername;
            var password = _authOptions.InitialPassword;
            var isGenerated = password.IsNullOrWhiteSpace();

            if (isGenerated)
            {
                password = SecretGenerator.Generate(GeneratedPasswordLength);
            }

            _userService.Add(username, password);

            if (isGenerated)
            {
                _authLogger.Warn("Authentication is enabled and no user existed, created '{0}' with the password: {1}", username, password);
                _authLogger.Warn("Change this password from Settings: General once you have signed in.");
            }
            else
            {
                _logger.Info("Authentication is enabled and no user existed, created '{0}' with the configured password", username);
            }
        }

        private bool MigrateFromConfigFile()
        {
            var configFile = _appFolderInfo.GetConfigPath();

            if (!_diskProvider.FileExists(configFile))
            {
                return false;
            }

            var xDoc = XDocument.Load(configFile);
            var config = xDoc.Descendants("Config").Single();
            var usernameElement = config.Descendants("Username").FirstOrDefault();
            var passwordElement = config.Descendants("Password").FirstOrDefault();

            var username = usernameElement?.Value;
            var password = passwordElement?.Value;

            if (username.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace())
            {
                return false;
            }

            _userService.Add(username, password);

            return true;
        }
    }
}
