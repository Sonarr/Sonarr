using System;
using System.Collections.Generic;
using NzbDrone.Api.History;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.REST;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeActivityModule : NzbDroneRestModule<HistoryResource>
                                 
    {
        private readonly IHistoryService _historyService;

        public EpisodeActivityModule(IHistoryService historyService)
            : base("episodes/activity")
        {
            _historyService = historyService;

            GetResourceAll = GetActivity;
        }

        private List<HistoryResource> GetActivity()
        {
            var episodeId = (int?)Request.Query.EpisodeId;

            if (episodeId == null)
            {
                throw new BadRequestException("episodeId is missing");
            }

            return ToListResource(() => _historyService.ByEpisode(episodeId.Value));
        }
    }
}