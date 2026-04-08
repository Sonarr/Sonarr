using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Tv
{
    public interface IAddSeriesService
    {
        Series AddSeries(Series newSeries);
        List<Series> AddSeries(List<Series> newSeries, bool ignoreErrors = false);
    }

    public class AddSeriesService : IAddSeriesService
    {
        private readonly ISeriesService _seriesService;
        private readonly IProvideSeriesInfo _seriesInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddSeriesValidator _addSeriesValidator;
        private readonly Logger _logger;

        public AddSeriesService(ISeriesService seriesService,
                                IProvideSeriesInfo seriesInfo,
                                IBuildFileNames fileNameBuilder,
                                IAddSeriesValidator addSeriesValidator,
                                Logger logger)
        {
            _seriesService = seriesService;
            _seriesInfo = seriesInfo;
            _fileNameBuilder = fileNameBuilder;
            _addSeriesValidator = addSeriesValidator;
            _logger = logger;
        }

        public Series AddSeries(Series newSeries)
        {
            Ensure.That(newSeries, () => newSeries).IsNotNull();

            newSeries = AddSkyhookData(newSeries);
            newSeries = SetPropertiesAndValidate(newSeries);

            _logger.Info("Adding Series {SeriesTitle} Path: [{Path}]", newSeries, newSeries.Path);
            _seriesService.AddSeries(newSeries);

            return newSeries;
        }

        public List<Series> AddSeries(List<Series> newSeries, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var seriesToAdd = new List<Series>();
            var existingSeriesTvdbIds = _seriesService.AllSeriesTvdbIds();

            foreach (var s in newSeries)
            {
                if (s.Path.IsNullOrWhiteSpace())
                {
                    _logger.Info("Adding Series {SeriesTitle} Root Folder Path: [{RootFolderPath}]", s, s.RootFolderPath);
                }
                else
                {
                    _logger.Info("Adding Series {SeriesTitle} Path: [{Path}]", s, s.Path);
                }

                try
                {
                    var series = AddSkyhookData(s);
                    series = SetPropertiesAndValidate(series);
                    series.Added = added;
                    if (existingSeriesTvdbIds.Any(f => f == series.TvdbId))
                    {
                        _logger.Debug("TVDB ID {TvdbId} was not added due to validation failure: Series {SeriesTitle} already exists in database", s.TvdbId, s);
                        continue;
                    }

                    if (seriesToAdd.Any(f => f.TvdbId == series.TvdbId))
                    {
                        _logger.Trace("TVDB ID {TvdbId} was already added from another import list, not adding series {SeriesTitle} again", s.TvdbId, s);
                        continue;
                    }

                    var duplicateSlug = seriesToAdd.FirstOrDefault(f => f.TitleSlug == series.TitleSlug);
                    if (duplicateSlug != null)
                    {
                        _logger.Debug("TVDB ID {TvdbId} was not added due to validation failure: Duplicate Slug {TitleSlug} used by series {DuplicateTvdbId}", s.TvdbId, s.TitleSlug, duplicateSlug.TvdbId);
                        continue;
                    }

                    seriesToAdd.Add(series);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("Series {SeriesTitle} with TVDB ID {TvdbId} was not added due to validation failures. {Message}", s, s.TvdbId, ex.Message);
                }
            }

            return _seriesService.AddSeries(seriesToAdd);
        }

        private Series AddSkyhookData(Series newSeries)
        {
            Tuple<Series, List<Episode>> tuple;

            try
            {
                tuple = _seriesInfo.GetSeriesInfo(newSeries.TvdbId);
            }
            catch (SeriesNotFoundException)
            {
                _logger.Error("Series {SeriesTitle} with TVDB ID {TvdbId} was not found, it may have been removed from TheTVDB. Path: {Path}", newSeries, newSeries.TvdbId, newSeries.Path);

                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TvdbId", $"A series with this ID was not found. Path: {newSeries.Path}", newSeries.TvdbId)
                                              });
            }

            var series = tuple.Item1;

            // If seasons were passed in on the new series use them, otherwise use the seasons from Skyhook
            newSeries.Seasons = newSeries.Seasons != null && newSeries.Seasons.Any() ? newSeries.Seasons : series.Seasons;

            series.ApplyChanges(newSeries);

            return series;
        }

        private Series SetPropertiesAndValidate(Series newSeries)
        {
            if (string.IsNullOrWhiteSpace(newSeries.Path))
            {
                var folderName = _fileNameBuilder.GetSeriesFolder(newSeries);
                newSeries.Path = Path.Combine(newSeries.RootFolderPath, folderName);
            }

            newSeries.CleanTitle = newSeries.Title.CleanSeriesTitle();
            newSeries.SortTitle = SeriesTitleNormalizer.Normalize(newSeries.Title, newSeries.TvdbId);
            newSeries.Added = DateTime.UtcNow;

            if (newSeries.AddOptions != null && newSeries.AddOptions.Monitor == MonitorTypes.None)
            {
                newSeries.Monitored = false;
            }

            var validationResult = _addSeriesValidator.Validate(newSeries);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newSeries;
        }
    }
}
