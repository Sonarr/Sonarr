using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Providers
{
    public class InventoryProvider
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly HistoryProvider _historyProvider;
        private readonly QualityTypeProvider _qualityTypeProvider;
        private readonly QualityProvider _qualityProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public InventoryProvider(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
            HistoryProvider historyProvider, QualityTypeProvider qualityTypeProvider,
            QualityProvider qualityProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _historyProvider = historyProvider;
            _qualityTypeProvider = qualityTypeProvider;
            _qualityProvider = qualityProvider;
        }

        public InventoryProvider()
        {

        }


        public virtual bool IsMonitored(EpisodeParseResult parseResult)
        {
            var series = _seriesProvider.FindSeries(parseResult.CleanTitle);

            if (series == null)
            {
                Logger.Trace("{0} is not mapped to any series in DB. skipping", parseResult.CleanTitle);
                return false;
            }

            parseResult.Series = series;

            if (!series.Monitored)
            {
                Logger.Debug("{0} is present in the DB but not tracked. skipping.", parseResult.CleanTitle);
                return false;
            }

            var episodes = _episodeProvider.GetEpisodesByParseResult(parseResult, true);

            //return monitored if any of the episodes are monitored
            if (episodes.Any(episode => !episode.Ignored))
            {
                return true;
            }

            Logger.Debug("All episodes are ignored. skipping.");
            return false;
        }

        /// <summary>
        ///   Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name = "parsedReport">Episode that needs to be checked</param>
        /// <param name="skipHistory">False unless called by a manual search job</param>
        /// <returns>Whether or not the file quality meets the requirements </returns>
        /// <remarks>for multi episode files, all episodes need to meet the requirement
        /// before the report is downloaded</remarks>
        public virtual bool IsQualityNeeded(EpisodeParseResult parsedReport, bool skipHistory = false)
        {
            Logger.Trace("Checking if report meets quality requirements. {0}", parsedReport.Quality);
            if (!parsedReport.Series.QualityProfile.Allowed.Contains(parsedReport.Quality.QualityType))
            {
                Logger.Trace("Quality {0} rejected by Series' quality profile", parsedReport.Quality);
                return false;
            }

            var cutoff = parsedReport.Series.QualityProfile.Cutoff;

            if (!IsAcceptableSize(parsedReport))
            {
                Logger.Info("Size: {0} is not acceptable for Quality: {1}", FileSizeFormatHelper.Format(parsedReport.Size, 2), parsedReport.Quality);
                return false;
            }

            foreach (var episode in _episodeProvider.GetEpisodesByParseResult(parsedReport, true))
            {
                //Checking File
                var file = episode.EpisodeFile;
                if (file != null)
                {
                    Logger.Trace("Comparing file quality with report. Existing file is {0} proper:{1}", file.Quality, file.Proper);
                    if (!IsUpgrade(new Quality { QualityType = file.Quality, Proper = file.Proper }, parsedReport.Quality, cutoff))
                        return false;
                }

                //Checking History (If not a manual search)
                if (!skipHistory)
                {
                    var bestQualityInHistory = _historyProvider.GetBestQualityInHistory(episode.EpisodeId);
                    if(bestQualityInHistory != null)
                    {
                        Logger.Trace("Comparing history quality with report. History is {0}", bestQualityInHistory);
                        if(!IsUpgrade(bestQualityInHistory, parsedReport.Quality, cutoff))
                            return false;
                    }
                }

                if (parsedReport.Indexer == "Newzbin")
                {
                    //Check for Blacklisting by NewzbinId
                    Logger.Trace("Checking if Newzbin ID has been black listed: ", parsedReport.NewzbinId);
                    if (_historyProvider.IsBlacklisted(parsedReport.NewzbinId))
                    {
                        Logger.Info("Newzbin ID has been blacklisted: [{0}] Skipping", parsedReport.NewzbinId);
                        return false;
                    }
                }

                else
                {
                    Logger.Trace("Checking if Nzb has been black listed: ", parsedReport.OriginalString);
                    if(_historyProvider.IsBlacklisted(parsedReport.OriginalString))
                    {
                        Logger.Info("Nzb has been blacklisted: [{0}] Skipping", parsedReport.OriginalString);
                        return false;
                    }
                }
            }

            Logger.Debug("Episode {0} is needed", parsedReport);
            return true; //If we get to this point and the file has not yet been rejected then accept it
        }

        public static bool IsUpgrade(Quality currentQuality, Quality newQuality, QualityTypes cutOff)
        {
            if (currentQuality.QualityType >= cutOff)
            {
                if (newQuality.QualityType > currentQuality.QualityType ||
                    (newQuality.QualityType == currentQuality.QualityType && newQuality.Proper == currentQuality.Proper))
                {
                    Logger.Trace("Existing item meets cut-off. skipping.");
                    return false;
                }
            }

            if (newQuality > currentQuality)
                return true;

            if (currentQuality > newQuality)
            {
                Logger.Trace("existing item has better quality. skipping");
                return false;
            }

            if (currentQuality == newQuality && !newQuality.Proper)
            {
                Logger.Trace("Same quality, not proper skipping");
                return false;
            }

            Logger.Debug("New item has better quality than existing item");
            return true;
        }

        public virtual bool IsAcceptableSize(EpisodeParseResult parseResult)
        {
            var qualityType = _qualityTypeProvider.Get((int) parseResult.Quality.QualityType);

            //Need to determine if this is a 30 or 60 minute episode
            //Is it a multi-episode release?
            //Is it the first or last series of a season?

            //0 will be treated as unlimited
            if (qualityType.MaxSize == 0)
                return true;

            var maxSize = qualityType.MaxSize.Megabytes();
            var series = parseResult.Series;

            //Multiply maxSize by Series.Runtime
            maxSize = maxSize * series.Runtime;

            //Multiply maxSize by the number of episodes parsed (if EpisodeNumbers is null it will be treated as a single episode)
            if (parseResult.EpisodeNumbers != null)
                maxSize = maxSize * parseResult.EpisodeNumbers.Count;

            //Check if there was only one episode parsed
            //and it is the first or last episode of the season
            if (parseResult.EpisodeNumbers != null && parseResult.EpisodeNumbers.Count == 1 && 
                _episodeProvider.IsFirstOrLastEpisodeOfSeason(series.SeriesId,
                parseResult.SeasonNumber, parseResult.EpisodeNumbers[0]))
            {
                maxSize = maxSize * 2;
            }

            //If the parsed size is greater than maxSize we don't want it
            if (parseResult.Size > maxSize)
                return false;

            return true;
        }

        public virtual bool IsUpgradePossible(Episode episode)
        {
            //Used to check if the existing episode can be upgraded by searching (Before we search)

            if (episode.EpisodeFileId == 0)
                return true;

            var profile = _qualityProvider.Get(episode.Series.QualityProfileId);

            if (episode.EpisodeFile.Quality >= profile.Cutoff)
                return false;

            return true;
        }
    }
}