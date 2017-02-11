using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Tv
{
    public interface IAddSeriesService
    {
        Series AddSeries(Series newSeries);
        List<Series> AddSeries(List<Series> newSeries);
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

            _logger.Info("Adding Series {0} Path: [{1}]", newSeries, newSeries.Path);
            _seriesService.AddSeries(newSeries);

            return newSeries;
        }

        public List<Series> AddSeries(List<Series> newSeries)
        {
            var added = DateTime.UtcNow;
            var seriesToAdd = new List<Series>();

            foreach (var s in newSeries)
            {
                // TODO: Verify if adding skyhook data will be slow
                var series = AddSkyhookData(s);
                series = SetPropertiesAndValidate(series);
                series.Added = added;
                seriesToAdd.Add(series);
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
                _logger.Error("tvdbid {1} was not found, it may have been removed from TheTVDB.", newSeries.TvdbId);
                
                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new ValidationFailure("TvdbId", "A series with this ID was not found", newSeries.TvdbId)
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

            var validationResult = _addSeriesValidator.Validate(newSeries);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newSeries;
        }
    }
}
