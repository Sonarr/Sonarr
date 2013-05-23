using Nancy.Authentication.Basic;
using Nancy.Security;
using NzbDrone.Common;
using NzbDrone.Common.Model;

namespace NzbDrone.Api.Authentication
{
    public interface IAuthenticationService : IUserValidator
    {
        AuthenticationType AuthenticationType { get; }
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfigFileProvider _configFileProvider;
        private static readonly NzbDroneUser AnonymousUser = new NzbDroneUser { UserName = "Anonymous" };


        public AuthenticationService(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
        }

        public AuthenticationType AuthenticationType
        {
            get { return _configFileProvider.AuthenticationType; }
        }

        public IUserIdentity Validate(string username, string password)
        {
            if (AuthenticationType == AuthenticationType.Anonymous)
            {
                return AnonymousUser;
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
