using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearch : SearchBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EpisodeSearch(IEpisodeService episodeService, DownloadProvider downloadProvider, IIndexerService indexerService,
                             ISceneMappingService sceneMappingService, IDownloadDirector downloadDirector,
                              ISeriesRepository seriesRepository)
            : base(seriesRepository, episodeService, downloadProvider, indexerService, sceneMappingService,
                   downloadDirector)
        {
        }

        public EpisodeSearch()
        {
        }

        public override List<EpisodeParseResult> PerformSearch(Series series, List<Episode> episodes, ProgressNotification notification)
        {
            //Todo: Daily and Anime or separate them out?
            //Todo: Epsiodes that use scene numbering

            var episode = episodes.Single();


            var reports = new List<EpisodeParseResult>();
            var title = GetSearchTitle(series);

            var seasonNumber = episode.SeasonNumber;
            var episodeNumber = episode.EpisodeNumber;

            if (series.UseSceneNumbering)
            {
                if (episode.SceneSeasonNumber > 0 && episode.SceneEpisodeNumber > 0)
                {
                    logger.Trace("Using Scene Numbering for: {0}", episode);
                    seasonNumber = episode.SceneSeasonNumber;
                    episodeNumber = episode.SceneEpisodeNumber;
                }
            }

            Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchEpisode(title, seasonNumber, episodeNumber));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-S{1:00}E{2:00} from: {3}",
                                                         series.Title, episode.SeasonNumber, episode.EpisodeNumber, indexer.Name), e);
                }
            });

            return reports;
        }

        public override bool IsEpisodeMatch(Series series, dynamic options, EpisodeParseResult episodeParseResult)
        {
            if (series.UseSceneNumbering && options.Episode.SeasonNumber > 0 && options.Episode.EpisodeNumber > 0)
            {
                if (options.Episode.SceneSeasonNumber != episodeParseResult.SeasonNumber)
                {
                    logger.Trace("Season number does not match searched season number, skipping.");
                    return false;
                }

                if (!episodeParseResult.EpisodeNumbers.Contains(options.Episode.SceneEpisodeNumber))
                {
                    logger.Trace("Episode number does not match searched episode number, skipping.");
                    return false;
                }

                return true;
            }

            if (options.Episode.SeasonNumber != episodeParseResult.SeasonNumber)
            {
                logger.Trace("Season number does not match searched season number, skipping.");
                return false;
            }

            if (!episodeParseResult.EpisodeNumbers.Contains(options.Episode.EpisodeNumber))
            {
                logger.Trace("Episode number does not match searched episode number, skipping.");
                return false;
            }

            return true;
        }

    }
}
