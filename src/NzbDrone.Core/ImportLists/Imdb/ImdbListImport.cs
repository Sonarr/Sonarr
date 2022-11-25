using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.Imdb
{
    public class ImdbListImport : HttpImportListBase<ImdbListSettings>
    {
        public override string Name => "IMDb Lists";

        public override ImportListType ListType => ImportListType.Other;

        public ImdbListImport(IHttpClient httpClient,
                              IImportListStatusService importListStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              Logger logger)
        : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                foreach (var def in base.DefaultDefinitions)
                {
                    yield return def;
                }
            }
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new ImdbListRequestGenerator()
            {
                Settings = Settings
            };
        }

        public override IParseImportListResponse GetParser()
        {
            return new ImdbListParser();
        }
    }
}
