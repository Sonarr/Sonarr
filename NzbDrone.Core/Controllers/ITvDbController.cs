using System.Collections.Generic;
using TvdbLib.Data;

namespace NzbDrone.Core.Controllers
{
    public interface ITvDbController
    {
        IList<TvdbSearchResult> SearchSeries(string name);
        TvdbSeries GetSeries(int id, TvdbLanguage language);
    }
}