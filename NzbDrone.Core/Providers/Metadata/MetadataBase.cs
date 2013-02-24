using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers.Metadata
{
    public abstract class MetadataBase
    {
        protected readonly Logger _logger;
        protected readonly IConfigService _configService;
        protected readonly DiskProvider _diskProvider;
        protected readonly BannerProvider _bannerProvider;
        protected readonly IEpisodeService _episodeService;

        protected MetadataBase(IConfigService configService, DiskProvider diskProvider,
                                BannerProvider bannerProvider, IEpisodeService episodeService)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _bannerProvider = bannerProvider;
            _episodeService = episodeService;
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
