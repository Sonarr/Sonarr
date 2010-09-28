using System.Collections.Generic;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public interface ITvDbProvider
    {
        IList<TvdbSearchResult> SearchSeries(string name);
        TvdbSeries GetSeries(int id, TvdbLanguage language);
    }
}