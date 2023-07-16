using System.Collections.Generic;
using System.Threading.Tasks;
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

        Task<IList<ReleaseInfo>> FetchRecent();
        Task<IList<ReleaseInfo>> Fetch(SeasonSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(SingleEpisodeSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(DailyEpisodeSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(DailySeasonSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(AnimeEpisodeSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(AnimeSeasonSearchCriteria searchCriteria);
        Task<IList<ReleaseInfo>> Fetch(SpecialEpisodeSearchCriteria searchCriteria);
        HttpRequest GetDownloadRequest(string link);
    }
}
