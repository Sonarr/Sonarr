using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Messaging.Events;
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
        Series FindByImdbId(string imdbId);
        Series FindByTitle(string title);
        Series FindByTitle(string title, int year);
        Series FindByTitleInexact(string title);
        Series FindByPath(string path);
        void DeleteSeries(List<int> seriesIds, bool deleteFiles, bool addImportListExclusion);
        List<Series> GetAllSeries();
        List<int> AllSeriesTvdbIds();
        Dictionary<int, string> GetAllSeriesPaths();
        Dictionary<int, List<int>> GetAllSeriesTags();
        List<Series> AllForTag(int tagId);
        Series UpdateSeries(Series series, bool updateEpisodesToMatchSeason = true, bool publishUpdatedEvent = true);
        List<Series> UpdateSeries(List<Series> series, bool useExistingRelativeFolder);
        bool SeriesPathExists(string folder);
        void RemoveAddOptions(Series series);
        bool UpdateTags(Series series);
    }

    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IEpisodeService _episodeService;
        private readonly IBuildSeriesPaths _seriesPathBuilder;
        private readonly IAutoTaggingService _autoTaggingService;
        private readonly Logger _logger;

        public SeriesService(ISeriesRepository seriesRepository,
                             IEventAggregator eventAggregator,
                             IEpisodeService episodeService,
                             IBuildSeriesPaths seriesPathBuilder,
                             IAutoTaggingService autoTaggingService,
                             Logger logger)
        {
            _seriesRepository = seriesRepository;
            _eventAggregator = eventAggregator;
            _episodeService = episodeService;
            _seriesPathBuilder = seriesPathBuilder;
            _autoTaggingService = autoTaggingService;
            _logger = logger;
        }

        public Series GetSeries(int seriesId)
        {
            return _seriesRepository.GetAsync(seriesId).GetAwaiter().GetResult();
        }

        public List<Series> GetSeries(IEnumerable<int> seriesIds)
        {
            return _seriesRepository.GetAsync(seriesIds).GetAwaiter().GetResult().ToList();
        }

        public Series AddSeries(Series newSeries)
        {
            _seriesRepository.InsertAsync(newSeries).GetAwaiter().GetResult();
            _eventAggregator.PublishEventAsync(new SeriesAddedEvent(GetSeries(newSeries.Id))).GetAwaiter().GetResult();

            return newSeries;
        }

        public List<Series> AddSeries(List<Series> newSeries)
        {
            _seriesRepository.InsertManyAsync(newSeries).GetAwaiter().GetResult();
            _eventAggregator.PublishEventAsync(new SeriesImportedEvent(newSeries.Select(s => s.Id).ToList())).GetAwaiter().GetResult();

            return newSeries;
        }

        public Series FindByTvdbId(int tvRageId)
        {
            return _seriesRepository.FindByTvdbIdAsync(tvRageId).GetAwaiter().GetResult();
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return _seriesRepository.FindByTvRageIdAsync(tvRageId).GetAwaiter().GetResult();
        }

        public Series FindByImdbId(string imdbId)
        {
            return _seriesRepository.FindByImdbIdAsync(imdbId).GetAwaiter().GetResult();
        }

        public Series FindByTitle(string title)
        {
            return _seriesRepository.FindByTitleAsync(title.CleanSeriesTitle()).GetAwaiter().GetResult();
        }

        public Series FindByTitleInexact(string title)
        {
            // find any series clean title within the provided release title
            var cleanTitle = title.CleanSeriesTitle();
            var list = _seriesRepository.FindByTitleInexactAsync(cleanTitle).GetAwaiter().GetResult();
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
            return _seriesRepository.FindByPathAsync(path).GetAwaiter().GetResult();
        }

        public Series FindByTitle(string title, int year)
        {
            return _seriesRepository.FindByTitleAsync(title.CleanSeriesTitle(), year).GetAwaiter().GetResult();
        }

        public void DeleteSeries(List<int> seriesIds, bool deleteFiles, bool addImportListExclusion)
        {
            var series = _seriesRepository.GetAsync(seriesIds).GetAwaiter().GetResult().ToList();
            _seriesRepository.DeleteManyAsync(seriesIds).GetAwaiter().GetResult();
            _eventAggregator.PublishEventAsync(new SeriesDeletedEvent(series, deleteFiles, addImportListExclusion)).GetAwaiter().GetResult();
        }

        public List<Series> GetAllSeries()
        {
            return _seriesRepository.AllAsync().GetAwaiter().GetResult().ToList();
        }

        public List<int> AllSeriesTvdbIds()
        {
            return _seriesRepository.AllSeriesTvdbIdsAsync().GetAwaiter().GetResult().ToList();
        }

        public Dictionary<int, string> GetAllSeriesPaths()
        {
            return _seriesRepository.AllSeriesPathsAsync().GetAwaiter().GetResult();
        }

        public Dictionary<int, List<int>> GetAllSeriesTags()
        {
            return _seriesRepository.AllSeriesTagsAsync().GetAwaiter().GetResult();
        }

        public List<Series> AllForTag(int tagId)
        {
            return GetAllSeries().Where(s => s.Tags.Contains(tagId))
                                 .ToList();
        }

        // updateEpisodesToMatchSeason is an override for EpisodeMonitoredService to use so a change via Season pass doesn't get nuked by the seasons loop.
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
            UpdateTags(series);

            var updatedSeries = _seriesRepository.UpdateAsync(series).GetAwaiter().GetResult();
            if (publishUpdatedEvent)
            {
                _eventAggregator.PublishEventAsync(new SeriesEditedEvent(updatedSeries, storedSeries, episodeMonitoredChanged)).GetAwaiter().GetResult();
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

                UpdateTags(s);
            }

            _seriesRepository.UpdateManyAsync(series).GetAwaiter().GetResult();
            _logger.Debug("{0} series updated", series.Count);
            _eventAggregator.PublishEventAsync(new SeriesBulkEditedEvent(series)).GetAwaiter().GetResult();

            return series;
        }

        public bool SeriesPathExists(string folder)
        {
            return _seriesRepository.SeriesPathExistsAsync(folder).GetAwaiter().GetResult();
        }

        public void RemoveAddOptions(Series series)
        {
            _seriesRepository.SetFieldsAsync(series, default, s => s.AddOptions).GetAwaiter().GetResult();
        }

        public bool UpdateTags(Series series)
        {
            _logger.Trace("Updating tags for {0}", series);

            var tagsAdded = new HashSet<int>();
            var tagsRemoved = new HashSet<int>();
            var changes = _autoTaggingService.GetTagChanges(series);

            foreach (var tag in changes.TagsToRemove)
            {
                if (series.Tags.Contains(tag))
                {
                    series.Tags.Remove(tag);
                    tagsRemoved.Add(tag);
                }
            }

            foreach (var tag in changes.TagsToAdd)
            {
                if (!series.Tags.Contains(tag))
                {
                    series.Tags.Add(tag);
                    tagsAdded.Add(tag);
                }
            }

            if (tagsAdded.Any() || tagsRemoved.Any())
            {
                _logger.Debug("Updated tags for '{0}'. Added: {1}, Removed: {2}", series.Title, tagsAdded.Count, tagsRemoved.Count);

                return true;
            }

            _logger.Debug("Tags not updated for '{0}'", series.Title);

            return false;
        }
    }
}
