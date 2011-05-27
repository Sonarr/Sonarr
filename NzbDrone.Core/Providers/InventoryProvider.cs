using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class InventoryProvider
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly SeasonProvider _seasonProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly HistoryProvider _historyProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public InventoryProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, HistoryProvider historyProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _historyProvider = historyProvider;
        }

        public InventoryProvider()
        {
            
        }

        public virtual bool IsNeeded(EpisodeParseResult parseResult)
        {
            var series = _seriesProvider.FindSeries(parseResult.CleanTitle);

            if (series == null)
            {
                Logger.Trace("{0} is not mapped to any series in DB. skipping", parseResult.CleanTitle);
                return false;
            }

            parseResult.Series = series;
            parseResult.Episodes = new List<Episode>();

            if (!series.Monitored)
            {
                Logger.Debug("{0} is present in the DB but not tracked. skipping.", parseResult.CleanTitle);
                return false;
            }

            if (!_seriesProvider.QualityWanted(series.SeriesId, parseResult.Quality))
            {
                Logger.Debug("Post doesn't meet the quality requirements [{0}]. skipping.", parseResult.Quality);
                return false;
            }

            if (_seasonProvider.IsIgnored(series.SeriesId, parseResult.SeasonNumber))
            {
                Logger.Debug("Season {0} is currently set to ignore. skipping.", parseResult.SeasonNumber);
                return false;
            }

            foreach (var episodeNumber in parseResult.EpisodeNumbers)
            {
                var episodeInfo = _episodeProvider.GetEpisode(series.SeriesId, parseResult.SeasonNumber, episodeNumber);
                if (episodeInfo == null)
                {
                    episodeInfo = _episodeProvider.GetEpisode(series.SeriesId, parseResult.AirDate);
                }
                //if still null we should add the temp episode
                if (episodeInfo == null)
                {
                    Logger.Debug("Episode {0} doesn't exist in db. adding it now.", parseResult);
                    episodeInfo = new Episode
                    {
                        SeriesId = series.SeriesId,
                        AirDate = DateTime.Now.Date,
                        EpisodeNumber = episodeNumber,
                        SeasonNumber = parseResult.SeasonNumber,
                        Title = parseResult.EpisodeTitle,
                        Overview = String.Empty,
                    };

                    _episodeProvider.AddEpisode(episodeInfo);
                }

                parseResult.Episodes.Add(episodeInfo);
            }

            foreach (var episode in parseResult.Episodes)
            {
                //Todo: How to handle full season files? Currently the episode list is completely empty for these releases
                //Todo: Should we assume that the release contains all the episodes that belong to this season and add them from the DB?
                //Todo: Fix this so it properly handles multi-epsiode releases (Currently as long as the first episode is needed we download it)
                //Todo: for small releases this is less of an issue, but for Full Season Releases this could be an issue if we only need the first episode (or first few)



                if (!_episodeProvider.IsNeeded(parseResult, episode))
                {
                    Logger.Debug("Episode {0} is not needed. skipping.", parseResult);
                    continue;
                }

                if (_historyProvider.Exists(episode.EpisodeId, parseResult.Quality, parseResult.Proper))
                {
                    Logger.Debug("Episode {0} is in history. skipping.", parseResult);
                    continue;
                }

                //Congragulations younge feed item! you have made it this far. you are truly special!!!
                Logger.Debug("Episode {0} is needed", parseResult);

                return true;
            }

            return false;
        }
    }
}