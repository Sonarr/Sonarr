using System;
using System.Text;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Cookies;
using Nancy.Cryptography;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using Sonarr.Http.Extensions;
using Sonarr.Http.Extensions.Pipelines;

namespace Sonarr.Http.Authentication
{
    public class EnableAuthInNancy : IRegisterNancyPipeline
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfigService _configService;
        private readonly IConfigFileProvider _configFileProvider;
        private FormsAuthenticationConfiguration FormsAuthConfig;

        public EnableAuthInNancy(IAuthenticationService authenticationService,
                                 IConfigService configService,
                                 IConfigFileProvider configFileProvider)
        {
            _authenticationService = authenticationService;
            _configService = configService;
            _configFileProvider = configFileProvider;
        }

        public int Order => 10;

        public void Register(IPipelines pipelines)
        {
            if (_configFileProvider.AuthenticationMethod == AuthenticationType.Forms)
            {
                RegisterFormsAuth(pipelines);
                pipelines.AfterRequest.AddItemToEndOfPipeline((Action<NancyContext>)SlidingAuthenticationForFormsAuth);
            }

            else if (_configFileProvider.AuthenticationMethod == AuthenticationType.Basic)
            {
                pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(_authenticationService, "Sonarr"));
            }

            pipelines.BeforeRequest.AddItemToEndOfPipeline((Func<NancyContext, Response>)RequiresAuthentication);
            pipelines.AfterRequest.AddItemToEndOfPipeline((Action<NancyContext>)RemoveLoginHooksForApiCalls);
        }

        private Response RequiresAuthentication(NancyContext context)
        {
            Response response = null;

            if (!_authenticationService.IsAuthenticated(context))
            {
                response = new Response { StatusCode = HttpStatusCode.Unauthorized };
            }

            return response;
        }

        private void RegisterFormsAuth(IPipelines pipelines)
        {
            FormsAuthentication.FormsAuthenticationCookieName = "SonarrAuth";

            var cryptographyConfiguration = new CryptographyConfiguration(
                    new RijndaelEncryptionProvider(new PassphraseKeyGenerator(_configService.RijndaelPassphrase, Encoding.ASCII.GetBytes(_configService.RijndaelSalt))),
                    new DefaultHmacProvider(new PassphraseKeyGenerator(_configService.HmacPassphrase, Encoding.ASCII.GetBytes(_configService.HmacSalt)))
                );

            FormsAuthConfig = new FormsAuthenticationConfiguration

            {
                RedirectUrl = _configFileProvider.UrlBase + "/login",
                UserMapper = _authenticationService,
                Path = GetCookiePath(),
                CryptographyConfiguration = cryptographyConfiguration
            };

            FormsAuthentication.Enable(pipelines, FormsAuthConfig);
        }

        private void RemoveLoginHooksForApiCalls(NancyContext context)
        {
            if (context.Request.IsApiRequest())
            {
                if ((context.Response.StatusCode == HttpStatusCode.SeeOther &&
                     context.Response.Headers["Location"].StartsWith($"{_configFileProvider.UrlBase}/login", StringComparison.InvariantCultureIgnoreCase)) ||
                    context.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    context.Response = new { Error = "Unauthorized" }.AsResponse(HttpStatusCode.Unauthorized);
                }
            }
        }

        private void SlidingAuthenticationForFormsAuth(NancyContext context)
        {
            if (context.CurrentUser == null)
            {
                return;
            }

            var formsAuthCookieName = FormsAuthentication.FormsAuthenticationCookieName;

            if (!context.Request.Path.Equals("/logout") &&
                context.Request.Cookies.ContainsKey(formsAuthCookieName))
            {
                var formsAuthCookieValue = context.Request.Cookies[formsAuthCookieName];

                if (FormsAuthentication.DecryptAndValidateAuthenticationCookie(formsAuthCookieValue, FormsAuthConfig).IsNotNullOrWhiteSpace())
                {
                    var formsAuthCookie = new NancyCookie(formsAuthCookieName, formsAuthCookieValue, true, false, DateTime.UtcNow.AddDays(7))
                    {
                        Path = GetCookiePath()
                    };

                    context.Response.WithCookie(formsAuthCookie);
                }
            }
        }

        private string GetCookiePath()
        {
            var urlBase = _configFileProvider.UrlBase;

            if (urlBase.IsNullOrWhiteSpace())
            {
                return "/";
            }

            return urlBase;
        }
    }
}
