using System;
using FluentValidation.Validators;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Validation.Paths
{
    public class SeriesExistsValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;

        public SeriesExistsValidator(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "This series has already been added";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var tvdbId = Convert.ToInt32(context.PropertyValue.ToString());

            return !_seriesService.GetAllSeries().Exists(s => s.TvdbId == tvdbId);
        }
    }
}
