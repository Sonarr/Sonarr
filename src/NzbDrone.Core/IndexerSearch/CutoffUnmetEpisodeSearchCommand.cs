using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class CutoffUnmetEpisodeSearchCommand : Command
    {
        public int? SeriesId { get; set; }
        public bool Monitored { get; set; }
        public List<int> SeriesIds { get; set; } = [];
        public List<int> QualityProfileIds { get; set; } = [];
        public List<SeriesTypes> SeriesType { get; set; } = [];
        public HashSet<int> SeriesTags { get; set; } = [];
        public List<int> Quality { get; set; } = [];

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public CutoffUnmetEpisodeSearchCommand()
        {
            Monitored = true;
        }

        public CutoffUnmetEpisodeSearchCommand(int seriesId)
        {
            SeriesId = seriesId;
            Monitored = true;
        }
    }
}
