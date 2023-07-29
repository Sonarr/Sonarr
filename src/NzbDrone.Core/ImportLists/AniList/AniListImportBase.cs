using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.AniList
{
    public abstract class AniListImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : AniListSettingsBase<TSettings>, new()
    {
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        private const string _cacheKey = "animemappings";
        private readonly TimeSpan _cacheInterval = TimeSpan.FromHours(20);
        private readonly ICached<Dictionary<int, MediaMapping>> _cache;

        public const string OAuthUrl = "https://anilist.co/api/v2/oauth/authorize";
        public const string RedirectUri = "http://localhost:5000/anilist/auth";
        public const string RenewUri = "http://localhost:5000/anilist/renew";
        public const string ClientId = "13737";

        private IImportListRepository _importListRepository;

        protected AniListImportBase(IImportListRepository netImportRepository,
                            IHttpClient httpClient,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            Logger logger,
                            ICacheManager cacheManager)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _importListRepository = netImportRepository;
            _cache = cacheManager.GetCache<Dictionary<int, MediaMapping>>(GetType());
        }

        public Dictionary<int, MediaMapping> Mappings => _cache.Get(_cacheKey, GetMappingData, _cacheInterval);

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = new HttpRequestBuilder(OAuthUrl)
                    .AddQueryParam("client_id", ClientId)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("redirect_uri", RedirectUri)
                    .AddQueryParam("state", query["callbackUrl"])
                    .Build();

                return new
                {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                return new
                {
                    accessToken = query["access_token"],
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(query["expires_in"])),
                    refreshToken = query["refresh_token"],
                };
            }

            return new { };
        }

        public void RefreshMappings()
        {
            var mappings = GetMappingData();
            _cache.Set(_cacheKey, mappings, _cacheInterval);
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            CheckToken();
            return base.Fetch();
        }

        protected virtual Dictionary<int, MediaMapping> GetMappingData()
        {
            var result = new Dictionary<int, MediaMapping>();
            try
            {
                var request = new HttpRequest(Settings.MapSourceUrl, HttpAccept.Json);
                var response = _httpClient.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var mappingList = STJson.Deserialize<List<MediaMapping>>(response.Content);
                    foreach (var item in mappingList)
                    {
                        if (item.Anilist.HasValue && item.Anilist > 0 && item.TVDB.HasValue && item.TVDB > 0)
                        {
                            result.Add((int)item.Anilist, item);
                        }
                    }
                }
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.NameResolutionFailure ||
                    webException.Status == WebExceptionStatus.ConnectFailure)
                {
                    _importListStatusService.RecordConnectionFailure(Definition.Id);
                }
                else
                {
                    _importListStatusService.RecordFailure(Definition.Id);
                }

                if (webException.Message.Contains("502") || webException.Message.Contains("503") ||
                    webException.Message.Contains("timed out"))
                {
                    _logger.Warn("{0} server is currently unavailable. {1} {2}", this, Settings.MapSourceUrl, webException.Message);
                }
                else
                {
                    _logger.Warn("{0} {1} {2}", this, Settings.MapSourceUrl, webException.Message);
                }
            }
            catch (HttpException ex)
            {
                _importListStatusService.RecordFailure(Definition.Id);
                _logger.Warn("{0} {1}", this, ex.Message);
            }
            catch (JsonSerializationException ex)
            {
                _importListStatusService.RecordFailure(Definition.Id);
                ex.WithData("MappingUrl", Settings.MapSourceUrl);
                _logger.Error(ex, "Mapping source data is invalid. {0}", Settings.MapSourceUrl);
            }
            catch (Exception ex)
            {
                _importListStatusService.RecordFailure(Definition.Id);
                ex.WithData("MappingUrl", Settings.MapSourceUrl);
                _logger.Error(ex, "An error occurred while downloading mapping file. {0}", Settings.MapSourceUrl);
            }

            return result;
        }

        protected override ValidationFailure TestConnection()
        {
            if (Mappings.Empty())
            {
                return new NzbDroneValidationFailure(string.Empty,
                            "Mapping source is not available or is invalid.")
                { IsWarning = true };
            }

            return base.TestConnection();
        }

        protected void CheckToken()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                _logger.Trace("Refreshing Token");

                Settings.Validate().Filter("RefreshToken").ThrowOnError();

                var request = new HttpRequestBuilder(RenewUri)
                    .AddQueryParam("refresh_token", Settings.RefreshToken)
                    .Build();

                try
                {
                    var response = _httpClient.Get<RefreshRequestResponse>(request);

                    if (response != null && response.Resource != null)
                    {
                        var token = response.Resource;
                        Settings.AccessToken = token.AccessToken;
                        Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                        Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                        if (Definition.Id > 0)
                        {
                            _importListRepository.UpdateSettings((ImportListDefinition)Definition);
                        }
                    }
                }
                catch (HttpException)
                {
                    _logger.Warn($"Error refreshing access token");
                }
            }
        }
    }
}
