using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Missing
{
    public class MissingModule : NzbDroneRestModule<EpisodeResource>
    {
        private readonly IEpisodeService _episodeService;
        private readonly SeriesService _seriesService;

        public MissingModule(IEpisodeService episodeService, SeriesService seriesService)
            :base("missing")
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            GetResourcePaged = GetMissingEpisodes;
        }

        private PagingResource<EpisodeResource> GetMissingEpisodes(PagingResource<EpisodeResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Episode>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            var episodeResources = ApplyToPage(_episodeService.EpisodesWithoutFiles, pagingSpec);

            var series = _seriesService.GetSeriesInList(episodeResources.Records.SelectDistinct(e => e.SeriesId));
            episodeResources.Records.Join(series, episode => episode.SeriesId, s => s.Id, episode => episode.Series);

            return episodeResources;
        }
    }
}