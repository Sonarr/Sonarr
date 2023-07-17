using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    internal class MalImport : HttpImportListBase<MalListSettings>
    {
        public override string Name => "MyAnimeList";
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => throw new NotImplementedException();

        // This constructor the first thing that is called when sonarr creates a button
        public MalImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            return FetchItems(g => g.GetListItems());
        }

        public override IParseImportListResponse GetParser()
        {
            return new MalParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new MalRequestGenerator();
        }
    }
}
