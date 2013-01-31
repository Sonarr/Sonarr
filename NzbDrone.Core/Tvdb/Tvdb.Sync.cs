using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using RestSharp.Deserializers;

namespace NzbDrone.Core.Tvdb
{
    public partial class Tvdb
    {
        private T ProcessRequest <T>(RestRequest request)
                where T: new()
        {
            return ProcessRequest<T>(BASE_URL, request);
        }

        private T ProcessRequest <T>(string url, RestRequest request)
                where T: new()
        {
            var client = new RestClient(url);
            client.AddHandler("text/xml", new DotNetXmlDeserializer());

            if(Timeout.HasValue)
                client.Timeout = Timeout.Value;

#if !WINDOWS_PHONE
            if(Proxy != null)
                client.Proxy = Proxy;
#endif

            Error = null;

            //var resp = client.Execute(request);
            IRestResponse<T> resp = client.Execute<T>(request);

            ResponseContent = resp.Content;
            ResponseHeaders = resp.Headers.ToDictionary(k => k.Name, v => v.Value);

            if(resp.ResponseStatus == ResponseStatus.Completed)
            {
                return resp.Data;

                // Manual deserialization
                //TextReader r = new StringReader(resp.Content);
                //XmlSerializer s = new XmlSerializer(typeof(T));
                //return (T)s.Deserialize(r);
            }
            else
            {
                if(resp.ErrorException != null)
                    throw resp.ErrorException;
                else
                    Error = resp.ErrorMessage;
            }

            return default(T);
        }

        public TvdbMirrors GetMirrors()
        {
            return ProcessRequest<TvdbMirrors>(BuildGetMirrorsRequest());
        }

        /// <summary>
        ///     http://www.thetvdb.com/api/Updates.php?type=none
        /// </summary>
        /// <returns></returns>
        public TvdbServerTime GetServerTime()
        {
            return ProcessRequest<TvdbServerTime>(BuildGetServerTimeRequest());
        }

        /// <summary>
        ///     http://www.thetvdb.com/api/{apikey}/languages.xml
        /// </summary>
        /// <returns></returns>
        public List<TvdbLanguage> GetLanguages()
        {
            var root = ProcessRequest<TvdbLanguagesRoot>(BuildGetLanguagesRequest());
            if(root != null)
                return root.Languages;

            return null;
        }


        /// <summary>
        ///     http://www.thetvdb.com/api/GetSeries.php?seriesname={series}
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public List<TvdbSeriesSearchItem> SearchSeries(string search)
        {
            var root = ProcessRequest<TvdbSeriesSearchRoot>(BuildGetSearchSeriesRequest(search));
            if(root != null)
                return root.Series;

            return null;
        }

        /// <summary>
        ///     http://thetvdb.com/api/{apikey}/series/79349/en.xml
        /// </summary>
        /// <param name="XMLMirror"></param>
        /// <param name="SeriesId"></param>
        /// <param name="Language"></param>
        /// <returns></returns>
        public TvdbSeriesBase GetSeriesBaseRecord(string XMLMirror, int SeriesId, string Language)
        {
            if(string.IsNullOrEmpty(Language))
                Language = "en";

            var root = ProcessRequest<TvdbSeriesRecordRoot>(XMLMirror,
                                                            BuildGetSeriesBaseRecordRequest(SeriesId, Language));
            if(root != null)
                return root.Series;

            return null;
        }

        public TvdbSeriesBase GetSeriesBaseRecord(string XMLMirror, int SeriesId)
        {
            return GetSeriesBaseRecord(XMLMirror, SeriesId, null);
        }

        /// <summary>
        ///     http://thetvdb.com/api/{apikey}/series/79349/all/en.xml
        /// </summary>
        /// <param name="XMLMirror"></param>
        /// <param name="SeriesId"></param>
        /// <param name="Language"></param>
        /// <returns></returns>
        public TvdbSeriesFull GetSeriesFullRecord(string XMLMirror, int SeriesId, string Language)
        {
            if(string.IsNullOrEmpty(Language))
                Language = "en";

            return ProcessRequest<TvdbSeriesFull>(XMLMirror, BuildGetSeriesFullRecordRequest(SeriesId, Language));
        }

        public TvdbSeriesFull GetSeriesFullRecord(string MirrorPath, int SeriesId)
        {
            return GetSeriesFullRecord(MirrorPath, SeriesId, null);
        }

        /// <summary>
        ///     http://thetvdb.com/api/{apikey}/series/79349/banners.xml
        /// </summary>
        /// <param name="XMLMirror"></param>
        /// <param name="SeriesId"></param>
        /// <returns></returns>
        public List<TvdbBanner> GetSeriesBanners(string XMLMirror, int SeriesId)
        {
            var root = ProcessRequest<TvdbBannerRoot>(XMLMirror, BuildGetSeriesBannersRequest(SeriesId));
            if(root != null)
                return root.Banners;

            return null;
        }

        /// <summary>
        ///     http://thetvdb.com/api/{apikey}/series/79349/actors.xml
        /// </summary>
        /// <param name="XMLMirror"></param>
        /// <param name="SeriesId"></param>
        /// <returns></returns>
        public List<TvdbActor> GetSeriesActors(string XMLMirror, int SeriesId)
        {
            var root = ProcessRequest<TvdbActorRoot>(XMLMirror, BuildGetSeriesActorsRequest(SeriesId));
            if(root != null)
                return root.Actors;

            return null;
        }

        public TvdbEpisode GetEpisode(string XMLMirror, int EpisodeId, string Language)
        {
            if(string.IsNullOrEmpty(Language))
                Language = "en";

            var root = ProcessRequest<TvdbEpisodeRoot>(XMLMirror, BuildGetEpisodeRequest(EpisodeId, Language));
            if(root != null)
                return root.Episode;

            return null;
        }

        public TvdbEpisode GetEpisode(string XMLMirror, int EpisodeId)
        {
            return GetEpisode(XMLMirror, EpisodeId, null);
        }

        public TvdbEpisode GetSeriesEpisode(string XMLMirror, int SeriesId, int SeasonNum, int EpisodeNum,
                                            string Language)
        {
            if(string.IsNullOrEmpty(Language))
                Language = "en";

            var root = ProcessRequest<TvdbEpisodeRoot>(XMLMirror,
                                                       BuildGetSeriesEpisodeRequest(SeriesId, SeasonNum, EpisodeNum,
                                                                                    Language));
            if(root != null)
                return root.Episode;

            return null;
        }

        public TvdbEpisode GetSeriesEpisode(string XMLMirror, int SeriesId, int SeasonNum, int EpisodeNum)
        {
            return GetSeriesEpisode(XMLMirror, SeriesId, SeasonNum, EpisodeNum, null);
        }

        public TvdbUpdates GetUpdates(string XMLMirror, TvdbUpdatePeriod Period)
        {
            return ProcessRequest<TvdbUpdates>(XMLMirror, BuildGetUpdatesRequest(Period));
        }

        public TvdbUpdateItems GetUpdatesSince(string XMLMirror, Int64 LastTime)
        {
            return ProcessRequest<TvdbUpdateItems>(XMLMirror, BuildGetUpdatesSinceRequest(LastTime));
        }
    }
}
