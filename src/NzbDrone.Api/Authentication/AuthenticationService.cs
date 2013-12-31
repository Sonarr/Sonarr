using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Security;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Authentication
{
    public interface IAuthenticationService : IUserValidator
    {
        bool Enabled { get; }
        bool IsAuthenticated(NancyContext context);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfigFileProvider _configFileProvider;
        private static readonly NzbDroneUser AnonymousUser = new NzbDroneUser { UserName = "Anonymous" };

        public AuthenticationService(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
        }

        public IUserIdentity Validate(string username, string password)
        {
            if (!Enabled)
            {
                return AnonymousUser;
            }

            if (_configFileProvider.Username.Equals(username) &&
                _configFileProvider.Password.Equals(password))
            {
                return new NzbDroneUser { UserName = username };
            }

            return null;
        }

        public bool Enabled
        {
            get
            {
                return _configFileProvider.AuthenticationEnabled;
            }
        }

        public bool IsAuthenticated(NancyContext context)
        {
            if (context.CurrentUser == null && _configFileProvider.AuthenticationEnabled) return false;

            return true;
        }
    }
}
