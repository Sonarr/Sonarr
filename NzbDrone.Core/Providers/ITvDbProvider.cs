using System.Collections.Generic;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public interface ITvDbProvider
    {
        IList<TvdbSearchResult> SearchSeries(string name);
        int GetBestMatch(List<TvdbSearchResult> searchResults, string searchString);
        TvdbSearchResult GetSeries(string title);
        TvdbSeries GetSeries(int id,  bool loadEpisodes);
    }
}