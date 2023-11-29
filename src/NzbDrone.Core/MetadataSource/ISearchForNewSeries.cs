using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewSeries
    {
        List<Series> SearchForNewSeries(string title);
        List<Series> SearchForNewSeriesByImdbId(string imdbId);
        List<Series> SearchForNewSeriesByAniListId(int aniListId);
        List<Series> SearchForNewSeriesByTmdbId(int tmdbId);
    }
}
