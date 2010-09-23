using System.Collections.Generic;
using System.IO;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;

namespace NzbDrone.Core.Controllers
{
    public class TvDbController : ITvDbController
    {
        private const string TvDbApiKey = "5D2D188E86E07F4F";
        private readonly TvdbHandler _handler;

        public TvDbController()
        {
            _handler = new TvdbHandler(new XmlCacheProvider(Path.Combine(Main.AppPath, @"\tvdbcache.xml")), TvDbApiKey);
        }

        #region ITvDbController Members

        public List<TvdbSearchResult> SearchSeries(string name)
        {
            return _handler.SearchSeries(name);
        }

        public TvdbSeries GetSeries(int id, TvdbLanguage language)
        {
            return _handler.GetSeries(id, language, true, false, false);
        }

        #endregion
    }
}