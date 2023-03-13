using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Rss
{
    public class RssImportBase<TSettings> : HttpImportListBase<TSettings>
        where TSettings : RssImportBaseSettings, new()
    {
        public RssImportBase(IHttpClient httpClient,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override ImportListType ListType => ImportListType.Advanced;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);
        public override string Name => "RSS List Base";

        public override IList<ImportListItemInfo> Fetch()
        {
            return FetchItems(g => g.GetListItems());
        }

        public override IParseImportListResponse GetParser()
        {
            return new RssImportBaseParser(_logger);
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new RssImportRequestGenerator()
            {
                Settings = Settings
            };
        }
    }
}
