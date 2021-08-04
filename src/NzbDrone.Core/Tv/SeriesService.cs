using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesService
    {
        Series GetSeries(int seriesId);
        List<Series> GetSeries(IEnumerable<int> seriesIds);
        Series AddSeries(Series newSeries);
        List<Series> AddSeries(List<Series> newSeries);
        Series FindByTvdbId(int tvdbId);
        Series FindByTvRageId(int tvRageId);
        Series FindByTitle(string title);
        Series FindByTitle(string title, int year);
        Series FindByTitleInexact(string title);
        Series FindByPath(string path);
        void DeleteSeries(int seriesId, bool deleteFiles, bool addImportListExclusion);
        List<Series> GetAllSeries();
        Dictionary<int, string> GetAllSeriesPaths();
        List<Series> AllForTag(int tagId);
        Series UpdateSeries(Series series, bool updateEpisodesToMatchSeason = true, bool publishUpdatedEvent = true);
        List<Series> UpdateSeries(List<Series> series, bool useExistingRelativeFolder);
        bool SeriesPathExists(string folder);
        void RemoveAddOptions(Series series);
    }

    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IEpisodeService _episodeService;
        private readonly IBuildSeriesPaths _seriesPathBuilder;
        private readonly Logger _logger;

        public SeriesService(ISeriesRepository seriesRepository,
                             IEventAggregator eventAggregator,
                             IEpisodeService episodeService,
                             IBuildSeriesPaths seriesPathBuilder,
                             Logger logger)
        {
            _seriesRepository = seriesRepository;
            _eventAggregator = eventAggregator;
            _episodeService = episodeService;
            _seriesPathBuilder = seriesPathBuilder;
            _logger = logger;
        }

        public Series GetSeries(int seriesId)
        {
            return _seriesRepository.Get(seriesId);
        }

        public List<Series> GetSeries(IEnumerable<int> seriesIds)
        {
            return _seriesRepository.Get(seriesIds).ToList();
        }

        public Series AddSeries(Series newSeries)
        {
            _seriesRepository.Insert(newSeries);
            _eventAggregator.PublishEvent(new SeriesAddedEvent(GetSeries(newSeries.Id)));

            return newSeries;
        }

        public List<Series> AddSeries(List<Series> newSeries)
        {
            _seriesRepository.InsertMany(newSeries);
            _eventAggregator.PublishEvent(new SeriesImportedEvent(newSeries.Select(s => s.Id).ToList()));

            return newSeries;
        }

        public Series FindByTvdbId(int tvRageId)
        {
            return _seriesRepository.FindByTvdbId(tvRageId);
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return _seriesRepository.FindByTvRageId(tvRageId);
        }

        public Series FindByTitle(string title)
        {
            return _seriesRepository.FindByTitle(title.CleanSeriesTitle());
        }

        public Series FindByTitleInexact(string title)
        {
            // find any series clean title within the provided release title
            string cleanTitle = title.CleanSeriesTitle();
            var list = _seriesRepository.FindByTitleInexact(cleanTitle);
            if (!list.Any())
            {
                // no series matched
                return null;
            }

            if (list.Count == 1)
            {
                // return the first series if there is only one
                return list.Single();
            }

            // build ordered list of series by position in the search string
            var query =
                list.Select(series => new
                {
                    position = cleanTitle.IndexOf(series.CleanTitle),
                    length = series.CleanTitle.Length,
                    series = series
                })
                    .Where(s => (s.position >= 0))
                    .ToList()
                    .OrderBy(s => s.position)
                    .ThenByDescending(s => s.length)
                    .ToList();

            // get the leftmost series that is the longest
            // series are usually the first thing in release title, so we select the leftmost and longest match
            var match = query.First().series;

            _logger.Debug("Multiple series matched {0} from title {1}", match.Title, title);
            foreach (var entry in list)
            {
                _logger.Debug("Multiple series match candidate: {0} cleantitle: {1}", entry.Title, entry.CleanTitle);
            }

            return match;
        }

        public Series FindByPath(string path)
        {
            return _seriesRepository.FindByPath(path);
        }

        public Series FindByTitle(string title, int year)
        {
            return _seriesRepository.FindByTitle(title.CleanSeriesTitle(), year);
        }

        public void DeleteSeries(int seriesId, bool deleteFiles, bool addImportListExclusion)
        {
            var series = _seriesRepository.Get(seriesId);
            _seriesRepository.Delete(seriesId);
            _eventAggregator.PublishEvent(new SeriesDeletedEvent(series, deleteFiles, addImportListExclusion));
        }

        public List<Series> GetAllSeries()
        {
            return _seriesRepository.All().ToList();
        }

        public Dictionary<int, string> GetAllSeriesPaths()
        {
            return _seriesRepository.AllSeriesPaths();
        }

        public List<Series> AllForTag(int tagId)
        {
            return GetAllSeries().Where(s => s.Tags.Contains(tagId))
                                 .ToList();
        }

        // updateEpisodesToMatchSeason is an override for EpisodeMonitoredService to use so a change via Season pass doesn't get nuked by the seasons loop.
        // TODO: Remove when seasons are split from series (or we come up with a better way to address this)
        public Series UpdateSeries(Series series, bool updateEpisodesToMatchSeason = true, bool publishUpdatedEvent = true)
        {
            var storedSeries = GetSeries(series.Id);

            var episodeMonitoredChanged = false;

            if (updateEpisodesToMatchSeason)
            {
                foreach (var season in series.Seasons)
                {
                    var storedSeason = storedSeries.Seasons.SingleOrDefault(s => s.SeasonNumber == season.SeasonNumber);

                    if (storedSeason != null && season.Monitored != storedSeason.Monitored)
                    {
                        _episodeService.SetEpisodeMonitoredBySeason(series.Id, season.SeasonNumber, season.Monitored);
                        episodeMonitoredChanged = true;
                    }
                }
            }

            // Never update AddOptions when updating a series, keep it the same as the existing stored series.
            series.AddOptions = storedSeries.AddOptions;

            var updatedSeries = _seriesRepository.Update(series);
            if (publishUpdatedEvent)
            {
                _eventAggregator.PublishEvent(new SeriesEditedEvent(updatedSeries, storedSeries, episodeMonitoredChanged));
            }

            return updatedSeries;
        }

        public List<Series> UpdateSeries(List<Series> series, bool useExistingRelativeFolder)
        {
            _logger.Debug("Updating {0} series", series.Count);

            foreach (var s in series)
            {
                _logger.Trace("Updating: {0}", s.Title);

                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    s.Path = _seriesPathBuilder.BuildPath(s, useExistingRelativeFolder);

                    _logger.Trace("Changing path for {0} to {1}", s.Title, s.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Title);
                }
            }

            _seriesRepository.UpdateMany(series);
            _logger.Debug("{0} series updated", series.Count);

            return series;
        }

        public bool SeriesPathExists(string folder)
        {
            return _seriesRepository.SeriesPathExists(folder);
        }

        public void RemoveAddOptions(Series series)
        {
            _seriesRepository.SetFields(series, s => s.AddOptions);
        }
    }
}
