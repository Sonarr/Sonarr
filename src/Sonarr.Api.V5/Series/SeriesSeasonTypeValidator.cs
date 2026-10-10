using FluentValidation.Validators;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace Sonarr.Api.V5.Series;

public class SeriesSeasonTypeValidator : PropertyValidator
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IMediaFileService _mediaFileService;

    public SeriesSeasonTypeValidator(ISeriesRepository seriesRepository, IMediaFileService mediaFileService)
    {
        _seriesRepository = seriesRepository;
        _mediaFileService = mediaFileService;
    }

    protected override string GetDefaultMessageTemplate() => "Unable to change Season Type for a series with episode files";

    protected override bool IsValid(PropertyValidatorContext context)
    {
        if (context.PropertyValue is not string seasonType)
        {
            return true;
        }

        if (context.InstanceToValidate is not SeriesResource seriesResource || seriesResource.Id == 0)
        {
            return true;
        }

        var series = _seriesRepository.Find(seriesResource.Id);

        if (series == null || series.SeasonType == seasonType)
        {
            return true;
        }

        return _mediaFileService.GetFilesBySeries(series.Id).Count == 0;
    }
}
