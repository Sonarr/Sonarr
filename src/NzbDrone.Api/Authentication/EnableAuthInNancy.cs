using System;
using System.Text;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Cryptography;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Extensions.Pipelines;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Authentication
{
    public class EnableAuthInNancy : IRegisterNancyPipeline
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfigService _configService;
        private readonly IConfigFileProvider _configFileProvider;

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
            }

            else if (_configFileProvider.AuthenticationMethod == AuthenticationType.Basic)
            {
                pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(_authenticationService, "Sonarr"));                
            }

            pipelines.BeforeRequest.AddItemToEndOfPipeline((Func<NancyContext, Response>) RequiresAuthentication);
            pipelines.AfterRequest.AddItemToEndOfPipeline((Action<NancyContext>) RemoveLoginHooksForApiCalls);
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
            var cryptographyConfiguration = new CryptographyConfiguration(
                    new RijndaelEncryptionProvider(new PassphraseKeyGenerator(_configService.RijndaelPassphrase, Encoding.ASCII.GetBytes(_configService.RijndaelSalt))),
                    new DefaultHmacProvider(new PassphraseKeyGenerator(_configService.HmacPassphrase, Encoding.ASCII.GetBytes(_configService.HmacSalt)))
                );

            FormsAuthentication.Enable(pipelines, new FormsAuthenticationConfiguration
            {
                RedirectUrl = _configFileProvider.UrlBase + "/login",
                UserMapper = _authenticationService,
                CryptographyConfiguration = cryptographyConfiguration
            });
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
    }
}
