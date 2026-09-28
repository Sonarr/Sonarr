using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        private const string AuthorizedProperty = "sonarr.authorized";

        private static readonly Logger Logger = LogManager.GetLogger(nameof(AuthenticationBuilderExtensions));

        private static readonly Regex CookieNameRegex =
            new Regex(@"[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Lock BackchannelHttpHandlerLock = new();

        private static SocketsHttpHandler _backchannelHttpHandler;

        public static AuthenticationBuilder AddAppAuthentication(this IServiceCollection services)
        {
            return services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddNone(nameof(AuthenticationType.None))
                .AddExternal(nameof(AuthenticationType.External))
                .AddOidc(nameof(AuthenticationType.Oidc))
                .AddForms(nameof(AuthenticationType.Forms))
                .AddApiKey("API", options =>
                {
                    options.HeaderName = "X-Api-Key";
                    options.QueryName = "apikey";
                })
                .AddApiKey("SignalR", options =>
                {
                    options.HeaderName = "X-Api-Key";
                    options.QueryName = "access_token";
                });
        }

        private static AuthenticationBuilder AddApiKey(this AuthenticationBuilder authenticationBuilder, string name, Action<ApiKeyAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(name,
                options);
        }

        private static AuthenticationBuilder AddNone(this AuthenticationBuilder authenticationBuilder, string name)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>(name,
                options => { });
        }

        private static AuthenticationBuilder AddExternal(this AuthenticationBuilder authenticationBuilder, string name)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>(name,
                options => { });
        }

        private static AuthenticationBuilder AddOidc(this AuthenticationBuilder authenticationBuilder, string name)
        {
            authenticationBuilder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                .Configure<IConfigFileProvider>((options, configFileProvider) =>
                {
                    var instanceName = GetCleanInstanceName(configFileProvider.InstanceName);

                    options.Cookie.Name = $"{instanceName}OIDC";
                    options.AccessDeniedPath = "/login?loginFailed=true";
                    options.LoginPath = "/login";
                    options.ReturnUrlParameter = "returnUrl";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.IsEssential = true;
                    options.Events.OnValidatePrincipal = context =>
                    {
                        var configuredUser = configFileProvider.OidcUserIdentifier;

                        if (configFileProvider.EffectiveAuthenticationMethod() == AuthenticationType.Oidc &&
                            configuredUser.IsNotNullOrWhiteSpace() &&
                            string.Equals(context.Principal?.FindFirst("user")?.Value, configuredUser, StringComparison.OrdinalIgnoreCase))
                        {
                            return Task.CompletedTask;
                        }

                        context.RejectPrincipal();

                        return context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    };

                    options.Events.OnRedirectToLogin = context => EventOnRedirectCookiesLogin(context, 401);
                    options.Events.OnRedirectToAccessDenied = context => EventOnRedirectCookiesLogin(context, 403);
                });

            authenticationBuilder.Services.AddOptions<OpenIdConnectOptions>(name)
                .Configure<IConfigFileProvider, ICertificateValidationService>((options, configFileProvider, certificateValidationService) =>
                {
                    options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    options.NonceCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

                    if (configFileProvider.AuthenticationMethod != AuthenticationType.Oidc ||
                        !configFileProvider.IsOidcConfigured())
                    {
                        options.Authority = "https://localhost";
                        options.ClientId = "Sonarr";

                        return;
                    }

                    options.Authority = configFileProvider.OidcAuthority;
                    options.ClientId = configFileProvider.OidcClientId;
                    options.ClientSecret = configFileProvider.OidcClientSecret;
                    options.ResponseType = "code";
                    options.ResponseMode = "query";
                    options.BackchannelHttpHandler = GetBackchannelHttpHandler(certificateValidationService);
                    options.Scope.Clear();

                    foreach (var scope in AuthenticationConfigurationExtensions.GetOidcScopes(configFileProvider.OidcScopes).Distinct(StringComparer.Ordinal))
                    {
                        options.Scope.Add(scope);
                    }

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.Events.OnTicketReceived = context =>
                    {
                        var idToken = context.Properties?.GetTokenValue("id_token");

                        context.Properties?.StoreTokens(idToken.IsNullOrWhiteSpace()
                            ? Array.Empty<AuthenticationToken>()
                            : [new AuthenticationToken { Name = "id_token", Value = idToken }]);

                        if (context.Properties?.Items.ContainsKey(AuthorizedProperty) == true)
                        {
                            return Task.CompletedTask;
                        }

                        if (!TryAuthorize(configFileProvider, context.Principal, null))
                        {
                            context.HandleResponse();
                            context.Response.Redirect(configFileProvider.UrlBase + "/login?ssoFailed=true");
                        }

                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        if (context.ProtocolMessage.IssuerAddress.IsNullOrWhiteSpace())
                        {
                            Logger.Debug("The OIDC provider doesn't advertise an end session endpoint, signing out locally only");

                            context.HandleResponse();
                            context.Response.Redirect(configFileProvider.UrlBase + "/loggedout");
                        }

                        return Task.CompletedTask;
                    };

                    options.Events.OnRemoteFailure = context =>
                    {
                        Logger.Error(context.Failure, "OIDC authentication failed");

                        context.HandleResponse();
                        context.Response.Redirect(configFileProvider.UrlBase + "/login?ssoFailed=true");

                        return Task.CompletedTask;
                    };

                    // Custom event to validate only authorized users
                    options.Events.OnUserInformationReceived = context =>
                    {
                        if (!TryAuthorize(configFileProvider, context.Principal, context.User.RootElement))
                        {
                            context.Fail("User is not authorized to access this application");

                            return Task.CompletedTask;
                        }

                        context.Properties.Items[AuthorizedProperty] = "true";

                        return Task.CompletedTask;
                    };
                });

            return authenticationBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => { })
                .AddOpenIdConnect(name, options => { });
        }

        private static SocketsHttpHandler GetBackchannelHttpHandler(ICertificateValidationService certificateValidationService)
        {
            lock (BackchannelHttpHandlerLock)
            {
                return _backchannelHttpHandler ??= new SocketsHttpHandler
                {
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = certificateValidationService.ShouldByPassValidationError
                    }
                };
            }
        }

        private static AuthenticationBuilder AddForms(this AuthenticationBuilder authenticationBuilder, string name)
        {
            authenticationBuilder.Services.AddOptions<CookieAuthenticationOptions>(nameof(AuthenticationType.Forms))
                .Configure<IConfigFileProvider>((options, configFileProvider) =>
                {
                    var instanceName = GetCleanInstanceName(configFileProvider.InstanceName);

                    options.Cookie.Name = $"{instanceName}Auth";
                    options.AccessDeniedPath = "/login?loginFailed=true";
                    options.LoginPath = "/login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.ReturnUrlParameter = "returnUrl";
                    options.Events.OnRedirectToLogin = context => EventOnRedirectCookiesLogin(context, 401);
                    options.Events.OnRedirectToAccessDenied = context => EventOnRedirectCookiesLogin(context, 403);
                });

            return authenticationBuilder.AddCookie(nameof(AuthenticationType.Forms));
        }

        private static bool TryAuthorize(IConfigFileProvider configFileProvider, ClaimsPrincipal principal, JsonElement? user)
        {
            var configuredUser = configFileProvider.OidcUserIdentifier;

            if (configuredUser.IsNullOrWhiteSpace())
            {
                Logger.Error("No authorized users configured");

                return false;
            }

            var identifiers = GetIdentifiers(principal, user);

            if (!identifiers.Any(i => string.Equals(i, configuredUser, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Error("User '{0}' is not authorized to access this application", identifiers.FirstOrDefault() ?? "unknown");

                return false;
            }

            var claims = new List<Claim>
            {
                new("user", configuredUser),
                new("AuthenticationType", nameof(AuthenticationType.Oidc)),
                new("Provider", "OIDC")
            };

            principal?.AddIdentity(new ClaimsIdentity(claims));

            return true;
        }

        private static List<string> GetIdentifiers(ClaimsPrincipal principal, JsonElement? user)
        {
            var identifiers = new List<string>();

            AddVerifiedEmail(identifiers, principal, user);

            var subject = GetClaimValue(principal, user, "sub");

            if (subject.IsNotNullOrWhiteSpace())
            {
                identifiers.Add(subject);
            }

            return identifiers;
        }

        private static void AddVerifiedEmail(List<string> identifiers, ClaimsPrincipal principal, JsonElement? user)
        {
            if (user.HasValue && TryGetString(user.Value, "email", out var userInfoEmail))
            {
                if (IsEmailVerified(user.Value))
                {
                    identifiers.Add(userInfoEmail);
                }

                return;
            }

            var email = principal?.FindFirst("email")?.Value;

            if (email.IsNotNullOrWhiteSpace() &&
                bool.TryParse(principal?.FindFirst("email_verified")?.Value, out var verified) &&
                verified)
            {
                identifiers.Add(email);
            }
        }

        private static string GetClaimValue(ClaimsPrincipal principal, JsonElement? user, string claim)
        {
            if (user.HasValue && TryGetString(user.Value, claim, out var value))
            {
                return value;
            }

            return principal?.FindFirst(claim)?.Value;
        }

        private static bool TryGetString(JsonElement element, string property, out string value)
        {
            if (element.TryGetProperty(property, out var jsonValue) && jsonValue.ValueKind == JsonValueKind.String)
            {
                value = jsonValue.GetString();

                return true;
            }

            value = null;

            return false;
        }

        private static bool IsEmailVerified(JsonElement user)
        {
            if (!user.TryGetProperty("email_verified", out var value))
            {
                return false;
            }

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) && parsed,
                _ => false
            };
        }

        private static string GetCleanInstanceName(string instanceName)
        {
            // Replace diacritics and replace non-word characters to ensure cookie name doesn't contain any valid URL characters not allowed in cookie names
            instanceName = instanceName.RemoveDiacritics();
            instanceName = CookieNameRegex.Replace(instanceName, string.Empty);

            return instanceName;
        }

        private static Task EventOnRedirectCookiesLogin(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
        {
            if (string.Equals(context.Request.Query[HeaderNames.XRequestedWith], "XMLHttpRequest", StringComparison.Ordinal) ||
                string.Equals(context.Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.Ordinal))
            {
                context.Response.Headers.Location = context.RedirectUri;
                context.Response.StatusCode = statusCode;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }

            return Task.CompletedTask;
        }
    }
}
