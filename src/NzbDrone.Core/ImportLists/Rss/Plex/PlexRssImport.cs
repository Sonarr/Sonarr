using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Rss.Plex
{
    public class PlexRssImport : RssImportBase<PlexRssImportSettings>
    {
        public PlexRssImport(IHttpClient httpClient,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override ImportListType ListType => ImportListType.Plex;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);
        public override string Name => "Plex Watchlist RSS";

        public override IParseImportListResponse GetParser()
        {
            return new PlexRssImportParser(_logger);
        }
    }
}
