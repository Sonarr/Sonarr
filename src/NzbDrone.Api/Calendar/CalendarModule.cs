using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Calendar
{
    public class CalendarModule : NzbDroneRestModuleWithSignalR<EpisodeResource, Episode>,
                                  IHandle<EpisodeGrabbedEvent>,                         
                                  IHandle<EpisodeDownloadedEvent>
    {
        private readonly IEpisodeService _episodeService;
        private readonly SeriesRepository _seriesRepository;

        public CalendarModule(ICommandExecutor commandExecutor,
                              IEpisodeService episodeService,
                              SeriesRepository seriesRepository)
            : base(commandExecutor, "calendar")
        {
            _episodeService = episodeService;
            _seriesRepository = seriesRepository;

            GetResourceAll = GetCalendar;
            GetResourceById = GetEpisode;
        }

        private EpisodeResource GetEpisode(int id)
        {
            return _episodeService.GetEpisode(id).InjectTo<EpisodeResource>();
        }

        private List<EpisodeResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);

            var queryStart = Request.Query.Start;
            var queryEnd = Request.Query.End;

            if (queryStart.HasValue) start = DateTime.Parse(queryStart.Value);
            if (queryEnd.HasValue) end = DateTime.Parse(queryEnd.Value);

            var resources = ToListResource(() => _episodeService.EpisodesBetweenDates(start, end))
                .LoadSubtype(e => e.SeriesId, _seriesRepository);

            return resources.OrderBy(e => e.AirDateUtc).ToList();
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var resource = episode.InjectTo<EpisodeResource>();
                resource.Downloading = true;

                BroadcastResourceChange(ModelAction.Updated, resource);
            }
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.Id);
            }
        }
    }
}
