using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        bool SupportsRss { get; }
        bool SupportsSearch { get; }
        DownloadProtocol Protocol { get; }

        IList<ReleaseInfo> FetchRecent();
        IList<ReleaseInfo> Fetch(SeasonSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(SingleEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(DailyEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(DailySeasonSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(AnimeEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(SpecialEpisodeSearchCriteria searchCriteria);
        HttpRequest GetDownloadRequest(string link);
    }
}
