using System.Collections.Generic;
using TvdbLib.Data;

namespace NzbDrone.Core.Controllers
{
    public interface ITvDbController
    {
        List<TvdbSearchResult> SearchSeries(string name);
        TvdbSeries GetSeries(int id, TvdbLanguage language);
    }
}