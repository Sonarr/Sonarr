using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.Discover;

public sealed class TMDbDiscoverImport : TMDbImportBase<TMDbDiscoverSettings>
{
    public TMDbDiscoverImport(ISonarrCloudRequestBuilder requestBuilder,
                              IHttpClient httpClient,
                              IImportListStatusService importListStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              ILocalizationService localizationService,
                              Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, localizationService, logger)
    {
    }

    public override int PageSize => 20;
    public override string Name => "TMDb Discover";

    public override IParseImportListResponse GetParser()
    {
        return new TMDbDiscoverParser();
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        return new TMDbDiscoverRequestGenerator(Settings);
    }

    protected override IEnumerable<KeyValuePair<string, TMDbDiscoverSettings>> GetPresetDefinitionPairs()
    {
        yield return new KeyValuePair<string, TMDbDiscoverSettings>("Top Rated",
            new TMDbDiscoverSettings
            {
                MinimumVoteCount = "200",
                Sort = (int)TMDbDiscoverSort.Vote_Average,
                SortOrder = (int)TMDbDiscoverSortOrder.Descending
            });
    }
}
