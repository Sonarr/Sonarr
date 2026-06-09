using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListImport : HttpImportListBase<MDBListSettings>
    {
        public override string Name => "MDBList";
        public override ImportListType ListType => ImportListType.MDBList;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);
        public override int PageSize => 1000;
        protected override int MaxNumResultsPerQuery => 10000;

        public MDBListImport(IHttpClient httpClient,
                             IImportListStatusService importListStatusService,
                             IConfigService configService,
                             IParsingService parsingService,
                             ILocalizationService localizationService,
                             Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override IParseImportListResponse GetParser()
        {
            return new MDBListParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new MDBListRequestGenerator
            {
                Settings = Settings
            };
        }
    }
}
