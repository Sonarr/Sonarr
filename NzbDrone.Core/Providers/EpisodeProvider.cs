using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Entities.Episode;
using NzbDrone.Core.Entities.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider : IEpisodeProvider
    {
        //TODO: Remove parsing of the series name, it should be done in series provider
        private static readonly Regex ParseRegex = new Regex(@"(?<showName>.*)
(?:
  s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)-?e(?<episodeNumber2>\d+)
| s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)
|  (?<seasonNumber>\d+)x(?<episodeNumber>\d+)
|  (?<airDate>\d{4}.\d{2}.\d{2})
)
(?:
  (?<episodeName>.*?)
  (?<release>
     (?:hdtv|pdtv|xvid|ws|720p|x264|bdrip|dvdrip|dsr|proper)
     .*)
| (?<episodeName>.*)
)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);


        private readonly IRepository _sonicRepo;
        private readonly ISeriesProvider _series;
        private readonly ISeasonProvider _seasons;
        private readonly ITvDbProvider _tvDb;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public EpisodeProvider(IRepository sonicRepo, ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, ITvDbProvider tvDbProvider)
        {
            _sonicRepo = sonicRepo;
            _series = seriesProvider;
            _tvDb = tvDbProvider;
            _seasons = seasonProvider;

        }

        public BasicEpisode GetEpisode(long id)
        {
            throw new NotImplementedException();
        }

        public BasicEpisode UpdateEpisode(BasicEpisode episode)
        {
            throw new NotImplementedException();
        }

        public IList<BasicEpisode> GetEpisodesBySeason(long seasonId)
        {
            throw new NotImplementedException();
        }

        public IList<BasicEpisode> GetEpisodeBySeries(long seriesId)
        {
            throw new NotImplementedException();
        }

        public String GetSabTitle(BasicEpisode episode)
        {
            var series = _series.GetSeries(episode.SeriesId);
            if (series == null) throw new ArgumentException("Unknown series. ID: " + episode.SeriesId);

            //TODO: This method should return a standard title for the sab episode.
            throw new NotImplementedException();

        }

        /// <summary>
        /// Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name="episode">Episode that needs to be checked</param>
        /// <returns></returns>
        public bool IsNeeded(BasicEpisode episode)
        {
            throw new NotImplementedException();
        }

        public void RefreshSeries(int seriesId)
        {
            Logger.Info("Starting episode info refresh for series:{0}", seriesId);
            int successCount = 0;
            int failCount = 0;
            var targetSeries = _tvDb.GetSeries(seriesId, true);
            foreach (var episode in targetSeries.Episodes)
            {
                try
                {
                    _seasons.EnsureSeason(seriesId, episode.SeasonId, episode.SeasonNumber);
                    var newEpisode = new EpisodeInfo()
                    {
                        AirDate = episode.FirstAired,
                        EpisodeId = episode.Id,
                        EpisodeNumber = episode.EpisodeNumber,
                        Language = episode.Language.Abbriviation,
                        Overview = episode.Overview,
                        SeasonId = episode.SeasonId,
                        SeasonNumber = episode.SeasonNumber,
                        SeriesId = episode.SeriesId,
                        Title = episode.EpisodeName
                    };

                    _sonicRepo.Add<EpisodeInfo>(newEpisode);
                    successCount++;

                }
                catch (Exception e)
                {
                    Logger.FatalException(String.Format("An error has occured while updating episode info for series {0}", seriesId), e);
                    failCount++;
                }
            }

            Logger.Info("Finished episode refresh for series:{0}. Success:{1} - Fail:{2} ", seriesId, successCount, failCount);
        }



        /// <summary>
        /// Parses a post title into list of episode objects
        /// </summary>
        /// <param name="title">Title of the report</param>
        /// <returns>List of episodes relating to the post</returns>
        public static List<RemoteEpisode> Parse(string title)
        {
            var match = ParseRegex.Match(title);

            if (!match.Success)
                throw new ArgumentException(String.Format("Title doesn't match any know patterns. [{0}]", title));

            var result = new List<RemoteEpisode>();

            result.Add(new RemoteEpisode { EpisodeNumber = Convert.ToInt32(match.Groups["episodeNumber"].Value) });

            if (match.Groups["episodeNumber2"].Success)
            {
                result.Add(new RemoteEpisode { EpisodeNumber = Convert.ToInt32(match.Groups["episodeNumber2"].Value) });
            }

            foreach (var ep in result)
            {
                ep.SeasonNumber = Convert.ToInt32(match.Groups["seasonNumber"].Value);
                ep.Proper = title.Contains("PROPER");
                ep.Quality = QualityTypes.Unknown;
            }

            return result;
        }

    }
}