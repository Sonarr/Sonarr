using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Model
{
    internal struct EpisodeParseResult
    {
        internal string SeriesTitle { get; set; }
        internal int SeasonNumber { get; set; }
        internal int EpisodeNumber { get; set; }
    }
}