using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Notifications.Plex.PlexTv;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Plex
{
    public class PlexImport : HttpImportListBase<PlexListSettings>
    {
        public readonly IPlexTvService _plexTvService;

        public override ImportListType ListType => ImportListType.Plex;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public PlexImport(IPlexTvService plexTvService,
                                  IHttpClient httpClient,
                                  IImportListStatusService importListStatusService,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  ILocalizationService localizationService,
                                  Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
            _plexTvService = plexTvService;
        }

        public override string Name => _localizationService.GetLocalizedString("ImportListsPlexSettingsWatchlistName");
        public override int PageSize => 50;

        public override ImportListFetchResult Fetch()
        {
            Settings.Validate().Filter("AccessToken").ThrowOnError();

            // var generator = GetRequestGenerator();

            return FetchItems(g => g.GetListItems());
        }

        public override IParseImportListResponse GetParser()
        {
            return new PlexParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new PlexListRequestGenerator(_plexTvService, PageSize)
            {
                Settings = Settings
            };
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                return _plexTvService.GetPinUrl();
            }
            else if (action == "continueOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["callbackUrl"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam callbackUrl invalid.");
                }

                if (query["id"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam id invalid.");
                }

                if (query["code"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam code invalid.");
                }

                return _plexTvService.GetSignInUrl(query["callbackUrl"], Convert.ToInt32(query["id"]), query["code"]);
            }
            else if (action == "getOAuthToken")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["pinId"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam pinId invalid.");
                }

                var accessToken = _plexTvService.GetAuthToken(Convert.ToInt32(query["pinId"]));

                return new
                {
                    accessToken
                };
            }

            return new { };
        }

        protected override ValidationFailure TestConnection()
        {
            try
            {
                var parser = GetParser();
                var generator = GetRequestGenerator();
                var pageableRequests = generator.GetListItems();
                var request = pageableRequests.GetAllTiers().First().First();
                var releases = FetchPage(request, parser);

                if (releases.Empty())
                {
                    _logger.Info("No results were returned from Plex Watchlist.");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to Plex Watchlist");
                return new ValidationFailure(string.Empty, $"Unable to connect to Plex Watchlist: {ex.Message}. Check the log for details.");
            }

            return null; // Indicate no fatal errors even if the list is empty.
        }
    }
}
