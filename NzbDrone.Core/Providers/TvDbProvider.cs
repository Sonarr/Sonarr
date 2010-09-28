using System.Collections.Generic;
using System.IO;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class TvDbProvider : ITvDbProvider
    {
        private const string TVDB_APIKEY = "5D2D188E86E07F4F";
        private readonly TvdbHandler _handler;

        public TvDbProvider()
        {
            _handler = new TvdbHandler(new XmlCacheProvider(Path.Combine(Main.AppPath, @"cache\tvdbcache.xml")), TVDB_APIKEY);
        }

        #region ITvDbProvider Members

        public IList<TvdbSearchResult> SearchSeries(string name)
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