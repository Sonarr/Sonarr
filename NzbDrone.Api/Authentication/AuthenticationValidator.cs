using Nancy.Authentication.Basic;
using Nancy.Security;
using NzbDrone.Common;
using NzbDrone.Common.Model;

namespace NzbDrone.Api.Authentication
{
    public class AuthenticationValidator : IUserValidator
    {
        private readonly IConfigFileProvider _configFileProvider;

        public AuthenticationValidator(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
        }

        public IUserIdentity Validate(string username, string password)
        {
            if (_configFileProvider.AuthenticationType == AuthenticationType.Anonymous)
            {
                return new NzbDroneUser { UserName = "Anonymous" };
            }

            if (_configFileProvider.BasicAuthUsername.Equals(username) &&
                _configFileProvider.BasicAuthPassword.Equals(password))
            {
                return new NzbDroneUser { UserName = username };
            }

            return null;
        }
    }
}
