using System;
using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Wanted
{
    public class MissingModule : EpisodeModuleWithSignalR
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesRepository _seriesRepository;

        public MissingModule(IEpisodeService episodeService, ISeriesRepository seriesRepository, IBroadcastSignalRMessage signalRBroadcaster)
            : base(episodeService, signalRBroadcaster, "wanted/missing")
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
            {
                pagingSpec.FilterExpression = v => v.Monitored == false || v.Series.Monitored == false;
            }
            else
            {
                pagingSpec.FilterExpression = v => v.Monitored == true && v.Series.Monitored == true;
            }

            PagingResource<EpisodeResource> resource = ApplyToPage(v => _episodeService.EpisodesWithoutFiles(v), pagingSpec);

            resource.Records = resource.Records.LoadSubtype(e => e.SeriesId, _seriesRepository).ToList();

            return resource;
        }
    }
}