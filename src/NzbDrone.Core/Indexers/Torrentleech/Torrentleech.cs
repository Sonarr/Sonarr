using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NLog;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class Torrentleech : HttpIndexerBase<TorrentleechSettings>
    {
        public override string Name => "TorrentLeech";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsSearch => true;
        public override int PageSize => 0;

        public Torrentleech(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {

        }

        protected override ValidationFailure TestConnection()
        {
            var loginRequest = new HttpRequest(Settings.LoginUrl, HttpAccept.Html)
            {
                Method = HttpMethod.POST,
                StoreResponseCookie = true
            };
            loginRequest.Headers.ContentType = "application/x-www-form-urlencoded";
            loginRequest.SetContent($"username={Settings.Username}&password={Settings.Password}");

            var loginResponse = _httpClient.Execute(loginRequest);

            if (loginResponse.Content.Contains("Invalid Username/password") || loginResponse.Content.Contains("<title>Login :: TorrentLeech.org</title>"))
            {
                return new ValidationFailure(nameof(Settings.Username), "Invalid username or password");
            }
            return null;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new TorrentleechRequestGenerator() { Settings = Settings, HttpClient = _httpClient };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorrentleechParser(Settings);
        }
    }
}
