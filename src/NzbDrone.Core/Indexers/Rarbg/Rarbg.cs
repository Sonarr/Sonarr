using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class Rarbg : HttpIndexerBase<RarbgSettings>
    {
        private readonly IRarbgTokenProvider _tokenProvider;

        public override string Name => "Rarbg";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override TimeSpan RateLimit => TimeSpan.FromSeconds(2);

        public Rarbg(IRarbgTokenProvider tokenProvider, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _tokenProvider = tokenProvider;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new RarbgRequestGenerator(_tokenProvider) { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new RarbgParser();
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "checkCaptcha")
            {
                Settings.Validate().Filter("BaseUrl").ThrowOnError();

                try
                {
                    var request = new HttpRequestBuilder(Settings.BaseUrl.Trim('/'))
                           .Resource("/pubapi_v2.php?get_token=get_token")
                           .Accept(HttpAccept.Json)
                           .Build();

                    _httpClient.Get(request);
                }
                catch (CloudFlareCaptchaException ex)
                {
                    return new
                    {
                        captchaRequest = new
                        {
                            host = ex.CaptchaRequest.Host,
                            ray = ex.CaptchaRequest.Ray,
                            siteKey = ex.CaptchaRequest.SiteKey,
                            secretToken = ex.CaptchaRequest.SecretToken,
                            responseUrl = ex.CaptchaRequest.ResponseUrl.FullUri,
                        }
                    };
                }

                return new
                {
                    captchaToken = ""
                };
            }
            else if (action == "getCaptchaCookie")
            {
                if (query["responseUrl"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam responseUrl invalid.");
                }

                if (query["ray"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam ray invalid.");
                }

                if (query["captchaResponse"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam captchaResponse invalid.");
                }

                var request = new HttpRequestBuilder(query["responseUrl"])
                    .AddQueryParam("id", query["ray"])
                    .AddQueryParam("g-recaptcha-response", query["captchaResponse"])
                    .Build();

                request.UseSimplifiedUserAgent = true;
                request.AllowAutoRedirect = false;

                var response = _httpClient.Get(request);

                var cfClearanceCookie = response.GetCookies()["cf_clearance"];

                return new
                {
                    captchaToken = cfClearanceCookie
                };
            }

            return new { };
        }
    }
}
