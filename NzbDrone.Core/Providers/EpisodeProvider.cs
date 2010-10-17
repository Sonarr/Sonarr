using System;
using System.Linq;
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

        public EpisodeInfo GetEpisode(long id)
        {
            return _sonicRepo.Single<EpisodeInfo>(e => e.EpisodeId == id);
        }

        public void UpdateEpisode(EpisodeInfo episode)
        {
            var episodeToUpdate = _sonicRepo.Single<EpisodeInfo>(e => e.EpisodeId == episode.EpisodeId);

            episodeToUpdate.AirDate = episode.AirDate;
            episodeToUpdate.Overview = episode.Overview;
            episodeToUpdate.Title = episode.Title;
            episodeToUpdate.EpisodeNumber = episode.EpisodeNumber;
            episodeToUpdate.SeasonNumber = episode.SeasonNumber;

            _sonicRepo.Update<EpisodeInfo>(episodeToUpdate);
        }

        public IList<EpisodeInfo> GetEpisodesBySeason(long seasonId)
        {
            return _sonicRepo.Find<EpisodeInfo>(e => e.SeasonId == seasonId);
        }

        public IList<EpisodeInfo> GetEpisodeBySeries(long seriesId)
        {
            return _sonicRepo.Find<EpisodeInfo>(e => e.SeriesId == seriesId);
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
        public bool IsNeeded(RemoteEpisode episode)
        {
            throw new NotImplementedException();
        }

        public void RefreshEpisodeInfo(int seriesId)
        {
            Logger.Info("Starting episode info refresh for series:{0}", seriesId);
            int successCount = 0;
            int failCount = 0;
            var targetSeries = _tvDb.GetSeries(seriesId, true);

            var updateList = new List<EpisodeInfo>();
            var newList = new List<EpisodeInfo>();

            Logger.Debug("Updating season info for series:{0}", seriesId);
            targetSeries.Episodes.Select(e => new { e.SeasonId, e.SeasonNumber })
                .Distinct().ToList()
                .ForEach(s => _seasons.EnsureSeason(seriesId, s.SeasonId, s.SeasonNumber));

            foreach (var episode in targetSeries.Episodes)
            {
                try
                {
                    Logger.Debug("Updating info for series:{0} - episode:{1}", seriesId, episode.Id);
                    var newEpisode = new EpisodeInfo()
                      {
                          AirDate = episode.FirstAired,
                          EpisodeId = episode.Id,
                          EpisodeNumber = episode.EpisodeNumber,
                          Language = episode.Language.Abbriviation,
                          Overview = episode.Overview,
                          SeasonId = episode.SeasonId,
                          SeasonNumber = episode.SeasonNumber,
                          SeriesId = seriesId,
                          Title = episode.EpisodeName
                      };

                    if (_sonicRepo.Exists<EpisodeInfo>(e => e.EpisodeId == newEpisode.EpisodeId))
                    {
                        updateList.Add(newEpisode);
                    }
                    else
                    {
                        newList.Add(newEpisode);
                    }

                    successCount++;
                }
                catch (Exception e)
                {
                    Logger.FatalException(String.Format("An error has occurred while updating episode info for series {0}", seriesId), e);
                    failCount++;
                }
            }

            _sonicRepo.AddMany(newList);
            _sonicRepo.UpdateMany(updateList);

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