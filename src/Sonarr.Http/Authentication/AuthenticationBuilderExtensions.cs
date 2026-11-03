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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddSingleton<ISonarrServiceTokenService, SonarrServiceTokenService>();
            services.AddSingleton<IOidcTokenValidationService, OidcTokenValidationService>();

            var builder = services.AddAuthentication(options =>
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

            builder.Services.AddOptions<JwtBearerOptions>("Bearer")
                .Configure<ISonarrServiceTokenService>((options, tokenService) =>
                {
                    if (tokenService == null || tokenService.SigningKey == null)
                    {
                        return;
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "sonarr",
                        ValidateAudience = true,
                        ValidAudience = "sonarr",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2),
                        IssuerSigningKey = tokenService.SigningKey,
                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var principal = context.Principal;
                            if (principal == null)
                            {
                                return Task.CompletedTask;
                            }

                            var identity = principal.Identity as ClaimsIdentity ?? new ClaimsIdentity(principal.Claims, "Bearer");
                            if (principal.Identity != principal.Identities.FirstOrDefault() || principal.Identity is not ClaimsIdentity)
                            {
                                // ensure claims are on a ClaimsIdentity we can modify
                            }

                            var subClaim = principal.FindFirst("sub") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
                            if (subClaim != null && principal.FindFirst("oidc_sub") == null)
                            {
                                identity.AddClaim(new Claim("oidc_sub", subClaim.Value));
                            }

                            var roleClaim = principal.FindFirst("role") ?? principal.FindFirst(ClaimTypes.Role);
                            if (roleClaim != null && principal.FindFirst("sonarr_role") == null)
                            {
                                identity.AddClaim(new Claim("sonarr_role", roleClaim.Value));
                            }

                            if (identity != context.Principal?.Identity && context.Principal != null)
                            {
                                context.Principal.AddIdentity(identity);
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return builder.AddJwtBearer("Bearer", options => { });
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
                .Configure<IConfigFileProvider, ICertificateValidationService>((options, configFileProvider, certificateValidationService) =>
                {
                    var instanceName = GetCleanInstanceName(configFileProvider.InstanceName);

                    options.Cookie.Name = $"{instanceName}OIDC";
                    options.AccessDeniedPath = "/login?loginFailed=true";
                    options.LoginPath = "/login";
                    options.ReturnUrlParameter = "returnUrl";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.IsEssential = true;
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        if (configFileProvider.EffectiveAuthenticationMethod() == AuthenticationType.Oidc &&
                            context.Principal?.FindFirst("oidc_sub") != null)
                        {
                            // authorized; fall through to refresh handling
                        }
                        else
                        {
                            var configuredUser = configFileProvider.OidcUserIdentifier;

                            if (!(configFileProvider.EffectiveAuthenticationMethod() == AuthenticationType.Oidc &&
                                  configuredUser.IsNotNullOrWhiteSpace() &&
                                  string.Equals(context.Principal?.FindFirst("user")?.Value, configuredUser, StringComparison.OrdinalIgnoreCase)))
                            {
                                context.RejectPrincipal();

                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                                return;
                            }
                        }

                        try
                        {
                            await RefreshOidcTokensAsync(context, configFileProvider, certificateValidationService);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, "OIDC token refresh failed");
                        }
                    };

                    options.Events.OnRedirectToLogin = context => EventOnRedirectCookiesLogin(context, 401);
                    options.Events.OnRedirectToAccessDenied = context => EventOnRedirectCookiesLogin(context, 403);
                });

            authenticationBuilder.Services.AddOptions<OpenIdConnectOptions>(name)
                .Configure<IConfigFileProvider, ICertificateValidationService>((options, configFileProvider, certificateValidationService) =>
                {
                    options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    options.NonceCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    options.CorrelationCookie.HttpOnly = true;
                    options.NonceCookie.HttpOnly = true;
                    options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                    options.NonceCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                    options.CorrelationCookie.Expiration = TimeSpan.FromMinutes(15);
                    options.NonceCookie.Expiration = TimeSpan.FromMinutes(15);

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
                    options.UsePkce = true;
                    options.BackchannelHttpHandler = GetBackchannelHttpHandler(certificateValidationService);
                    options.RefreshInterval = TimeSpan.FromHours(24);
                    options.Scope.Clear();

                    foreach (var scope in AuthenticationConfigurationExtensions.GetOidcScopes(configFileProvider.OidcScopes).Distinct(StringComparer.Ordinal))
                    {
                        options.Scope.Add(scope);
                    }

                    if (!options.Scope.Contains("offline_access"))
                    {
                        options.Scope.Add("offline_access");
                    }

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.Events.OnTicketReceived = context =>
                    {
                        var idToken = context.Properties?.GetTokenValue("id_token");
                        var existingRefreshToken = context.Properties?.GetTokenValue("refresh_token");
                        var existingExpiresAt = context.Properties?.GetTokenValue("expires_at");

                        var tokens = new List<AuthenticationToken>();
                        if (!idToken.IsNullOrWhiteSpace())
                        {
                            tokens.Add(new AuthenticationToken { Name = "id_token", Value = idToken });
                        }

                        if (!existingRefreshToken.IsNullOrWhiteSpace())
                        {
                            tokens.Add(new AuthenticationToken { Name = "refresh_token", Value = existingRefreshToken });
                        }

                        if (!existingExpiresAt.IsNullOrWhiteSpace())
                        {
                            tokens.Add(new AuthenticationToken { Name = "expires_at", Value = existingExpiresAt });
                        }

                        context.Properties?.StoreTokens(tokens.Count == 0
                            ? Array.Empty<AuthenticationToken>()
                            : tokens.ToArray());

                        if (context.Properties?.Items.ContainsKey(AuthorizedProperty) == true)
                        {
                            return Task.CompletedTask;
                        }

                        context.HandleResponse();
                        context.Response.Redirect(configFileProvider.UrlBase + "/login?ssoFailed=true");

                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        // Servarr sits behind TLS-terminating tailscale serve via a Docker
                        // bridge peer, so HttpRequest.Scheme is http even though the user
                        // arrived over https. Authentik would then send the browser back
                        // to an http:// callback on a TLS-only port (connection reset).
                        // Force https for non-loopback redirect URIs; loopback stays http
                        // for native/PKCE flows. Both schemes are registered in Authentik.
                        var redirectUri = context.ProtocolMessage?.RedirectUri;
                        if (!string.IsNullOrWhiteSpace(redirectUri) &&
                            redirectUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var uri = new Uri(redirectUri);
                                #pragma warning disable CA1845 // Span-based concatenation not available for this legacy pattern in net10.0 build context
                                if (!uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                                    !System.Net.IPAddress.TryParse(uri.Host, out _))
                                {
                                    context.ProtocolMessage.RedirectUri = "https://" + redirectUri.Substring(7);
                                }
                                #pragma warning restore CA1845
                            }
                            catch (UriFormatException ex)
                            {
                                Logger.Warn(ex, "Unable to parse OIDC redirect URI '{0}'", redirectUri);
                            }
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

                        // Force JWKS refresh on signature failure
                        try
                        {
                            var oidcOptions = context.Options as OpenIdConnectOptions;
                            oidcOptions?.ConfigurationManager?.RequestRefresh();
                        }
                        catch
                        {
                        }

                        context.HandleResponse();
                        context.Response.Redirect(configFileProvider.UrlBase + "/login?ssoFailed=true");

                        return Task.CompletedTask;
                    };

                    options.Events.OnTokenValidated = context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetService(typeof(IUserService)) as IUserService;

                        var groupsClaimName = configFileProvider.OidcGroupsClaim;
                        if (groupsClaimName.IsNullOrWhiteSpace())
                        {
                            groupsClaimName = "groups";
                        }

                        var groups = GetGroups(context.Principal, groupsClaimName).ToList();

                        var resolvedRole = OidcGroupMapping.Resolve(groups, configFileProvider.OidcRoleMappings);

                        if (resolvedRole == null)
                        {
                            // Fallback to OidcUserIdentifier for backwards compat when groups don't match (single-user installations)
                            var configuredUser = configFileProvider.OidcUserIdentifier;
                            var fallbackSubject = context.Principal?.FindFirst("sub")?.Value;
                            var fallbackUsername = context.Principal?.FindFirst("preferred_username")?.Value
                                ?? context.Principal?.FindFirst("email")?.Value
                                ?? fallbackSubject;
                            var fallbackEmail = context.Principal?.FindFirst("email")?.Value;

                            if (!string.IsNullOrWhiteSpace(configuredUser) &&
                                (string.Equals(fallbackUsername, configuredUser, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(fallbackSubject, configuredUser, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(fallbackEmail, configuredUser, StringComparison.OrdinalIgnoreCase)))
                            {
                                Logger.Info("OIDC user {0} authorized via OidcUserIdentifier fallback (groups [{1}] not in mapping)",
                                    fallbackUsername ?? fallbackSubject ?? "unknown",
                                    string.Join(",", groups));
                                resolvedRole = SonarrRole.Admin;
                            }
                            else
                            {
                                var subjectForLog = fallbackSubject
                                    ?? fallbackUsername
                                    ?? fallbackEmail;

                                Logger.Warn("OIDC user {0} rejected: groups [{1}] not in allow list",
                                    subjectForLog ?? "unknown",
                                    string.Join(",", groups));

                                context.Fail("User is not authorized: insufficient group membership");
                                context.HandleResponse();
                                context.Response.Redirect(configFileProvider.UrlBase + "/login?ssoFailed=insufficient_group");

                                return Task.CompletedTask;
                            }
                        }

                        var principal = context.Principal;
                        var subject = principal?.FindFirst("sub")?.Value;
                        var username = principal?.FindFirst("preferred_username")?.Value
                            ?? principal?.FindFirst("email")?.Value
                            ?? subject;

                        if (userService == null)
                        {
                            Logger.Error("IUserService is not available; cannot provision OIDC user");
                        }
                        else
                        {
                            try
                            {
                                userService.UpsertOidcUser(subject, username, string.Join(",", groups), resolvedRole.Value);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Failed to upsert OIDC user");
                            }
                        }

                        if (!TryAuthorize(configFileProvider, principal, null, resolvedRole.Value, subject, username, groups))
                        {
                            context.Fail("User is not authorized to access this application");
                            context.HandleResponse();
                            context.Response.Redirect(configFileProvider.UrlBase + "/login?ssoFailed=true");

                            return Task.CompletedTask;
                        }

                        context.Properties.Items[AuthorizedProperty] = "true";

                        return Task.CompletedTask;
                    };

                    // no-op: authorization moved to OnTokenValidated
                    options.Events.OnUserInformationReceived = context => Task.CompletedTask;
                });

            return authenticationBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => { })
                .AddOpenIdConnect(name, options => { });
        }

        private static async Task RefreshOidcTokensAsync(CookieValidatePrincipalContext context, IConfigFileProvider configFileProvider, ICertificateValidationService certificateValidationService)
        {
            if (configFileProvider.EffectiveAuthenticationMethod() != AuthenticationType.Oidc || !configFileProvider.IsOidcConfigured())
            {
                return;
            }

            var refreshToken = context.Properties.GetTokenValue("refresh_token");
            var expiresAtRaw = context.Properties.GetTokenValue("expires_at");

            DateTimeOffset? expiresAt = null;
            if (!string.IsNullOrWhiteSpace(expiresAtRaw) && DateTimeOffset.TryParse(expiresAtRaw, out var parsedExpires))
            {
                expiresAt = parsedExpires;
            }

            var nearExpiry = expiresAt == null || (expiresAt.Value - DateTimeOffset.UtcNow) < TimeSpan.FromMinutes(5);
            if (!nearExpiry || string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            string tokenEndpoint = null;
            var oidcOptionsMonitor = context.HttpContext.RequestServices.GetService(typeof(IOptionsMonitor<OpenIdConnectOptions>)) as IOptionsMonitor<OpenIdConnectOptions>;
            var oidcOptions = oidcOptionsMonitor?.Get(nameof(AuthenticationType.Oidc));
            if (oidcOptions?.Configuration != null)
            {
                tokenEndpoint = oidcOptions.Configuration.TokenEndpoint;
            }

            if (string.IsNullOrWhiteSpace(tokenEndpoint) && oidcOptions?.ConfigurationManager != null)
            {
                try
                {
                    var cfg = await oidcOptions.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
                    tokenEndpoint = cfg?.TokenEndpoint;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to discover OIDC configuration");
                }
            }

            if (string.IsNullOrWhiteSpace(tokenEndpoint))
            {
                Logger.Warn("OIDC token endpoint not available; skipping refresh");
                return;
            }

            var handler = GetBackchannelHttpHandler(certificateValidationService);
            using var client = new HttpClient(handler, disposeHandler: false);
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = configFileProvider.OidcClientId ?? string.Empty,
                ["client_secret"] = configFileProvider.OidcClientSecret ?? string.Empty,
                ["refresh_token"] = refreshToken
            };

            try
            {
                var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Logger.Warn("OIDC token refresh failed: {0} {1}", response.StatusCode, body);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var newAccess = root.TryGetProperty("access_token", out var aEl) ? aEl.GetString() : null;
                if (string.IsNullOrWhiteSpace(newAccess))
                {
                    Logger.Warn("OIDC token refresh response missing access_token");
                    return;
                }

                var newRefresh = root.TryGetProperty("refresh_token", out var rEl) ? rEl.GetString() : null;
                var expiresIn = 3600;
                if (root.TryGetProperty("expires_in", out var eEl) && eEl.TryGetInt32(out var ei))
                {
                    expiresIn = ei;
                }

                var refreshToStore = string.IsNullOrWhiteSpace(newRefresh) ? refreshToken : newRefresh;

                var estimatedSize = (newAccess?.Length ?? 0) + (refreshToStore?.Length ?? 0) + 200;
                if (estimatedSize > 3800)
                {
                    Logger.Warn("OIDC refresh token too large for cookie; skipping renewal");
                    return;
                }

                var tokens = new List<AuthenticationToken>
                {
                    new AuthenticationToken { Name = "access_token", Value = newAccess },
                    new AuthenticationToken { Name = "refresh_token", Value = refreshToStore },
                    new AuthenticationToken { Name = "expires_at", Value = DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("o") }
                };

                var existingIdToken = context.Properties.GetTokenValue("id_token");
                if (!string.IsNullOrWhiteSpace(existingIdToken))
                {
                    tokens.Add(new AuthenticationToken { Name = "id_token", Value = existingIdToken });
                }

                context.Properties.StoreTokens(tokens);
                context.ShouldRenew = true;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "OIDC token refresh request failed");
            }
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

        public static IEnumerable<string> GetGroups(ClaimsPrincipal principal, string claimName)
        {
            var groups = new List<string>();

            if (principal == null || string.IsNullOrWhiteSpace(claimName))
            {
                return groups;
            }

            foreach (var claim in principal.FindAll(claimName))
            {
                SplitAndAdd(groups, claim.Value);
            }

            if (groups.Count == 0 && !string.Equals(claimName, "groups", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var claim in principal.FindAll("groups"))
                {
                    SplitAndAdd(groups, claim.Value);
                }
            }

            return groups.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void SplitAndAdd(List<string> target, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            foreach (var part in value.Split(new[] { ',', ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                target.Add(part);
            }
        }

        private static bool TryAuthorize(IConfigFileProvider configFileProvider, ClaimsPrincipal principal, JsonElement? user, SonarrRole role, string subject = null, string username = null, IEnumerable<string> groups = null)
        {
            if (subject == null)
            {
                subject = principal?.FindFirst("sub")?.Value
                    ?? principal?.FindFirst("oidc_sub")?.Value;
            }

            if (username == null)
            {
                username = principal?.FindFirst("preferred_username")?.Value
                    ?? principal?.FindFirst("email")?.Value
                    ?? subject;
            }

            if (groups == null)
            {
                groups = GetGroups(principal, configFileProvider.OidcGroupsClaim);
            }

            var groupString = string.Join(",", groups);

            var claims = new List<Claim>
            {
                new("user", username ?? string.Empty),
                new("sonarr_role", role.ToString()),
                new("groups", groupString),
                new("oidc_sub", subject ?? string.Empty),
                new("AuthenticationType", nameof(AuthenticationType.Oidc)),
                new("Provider", "OIDC")
            };

            if (principal != null)
            {
                foreach (var identity in principal.Identities)
                {
                    foreach (var claim in identity.FindAll("sonarr_role").ToList())
                    {
                        identity.RemoveClaim(claim);
                    }
                }
            }

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
