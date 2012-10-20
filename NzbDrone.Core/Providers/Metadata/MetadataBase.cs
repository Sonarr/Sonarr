using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers.Metadata
{
    public abstract class MetadataBase
    {
        protected readonly Logger _logger;
        protected readonly ConfigProvider _configProvider;
        protected readonly DiskProvider _diskProvider;
        protected readonly BannerProvider _bannerProvider;
        protected readonly EpisodeProvider _episodeProvider;

        protected MetadataBase(ConfigProvider configProvider, DiskProvider diskProvider,
                                BannerProvider bannerProvider, EpisodeProvider episodeProvider)
        {
            _configProvider = configProvider;
            _diskProvider = diskProvider;
            _bannerProvider = bannerProvider;
            _episodeProvider = episodeProvider;
            _logger = LogManager.GetLogger(GetType().ToString());
        }

        /// <summary>
        ///   Gets the name for the metabase provider
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   Creates metadata for a series
        /// </summary>
        /// <param name = "series">The series to create the metadata for</param>
        /// <param name = "tvDbSeries">Series information from TheTvDb</param>
        public abstract void CreateForSeries(Series series, TvdbSeries tvDbSeries);

        /// <summary>
        ///   Creates metadata for the episode file
        /// </summary>
        /// <param name = "episodeFile">The episode file to create the metadata</param>
        /// <param name = "tvDbSeries">Series information from TheTvDb</param>
        public abstract void CreateForEpisodeFile(EpisodeFile episodeFile, TvdbSeries tvDbSeries);

        /// <summary>
        ///   Removes metadata for a series
        /// </summary>
        /// <param name = "series">The series to create the metadata for</param>
        public abstract void RemoveForSeries(Series series);

        /// <summary>
        ///   Removes metadata for the episode file
        /// </summary>
        /// <param name = "episodeFile">The episode file to create the metadata</param>
        public abstract void RemoveForEpisodeFile(EpisodeFile episodeFile);

        public virtual string GetEpisodeGuideUrl(int seriesId)
        {
            return String.Format("http://www.thetvdb.com/api/{0}/series/{1}/all/en.zip", TvDbProvider.TVDB_APIKEY, seriesId);
        }
    }
}
