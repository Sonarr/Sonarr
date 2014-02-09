using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Wanted
{
    public class MissingModule : NzbDroneRestModule<EpisodeResource>
    {
        private readonly IEpisodeService _episodeService;
        private readonly SeriesRepository _seriesRepository;

        public MissingModule(IEpisodeService episodeService, SeriesRepository seriesRepository)
            :base("wanted/missing")
        {
            _episodeService = episodeService;
            _seriesRepository = seriesRepository;
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
            
            if (pagingResource.FilterKey == "monitored" && pagingResource.FilterValue == "false")
                pagingSpec.FilterExpression = v => v.Monitored == false || v.Series.Monitored == false;
            else
                pagingSpec.FilterExpression = v => v.Monitored == true && v.Series.Monitored == true;

            PagingResource<EpisodeResource> resource = ApplyToPage(v => _episodeService.GetMissingEpisodes(v), pagingSpec);

            resource.Records = resource.Records.LoadSubtype(e => e.SeriesId, _seriesRepository).ToList();

            return resource;
        }
    }
}