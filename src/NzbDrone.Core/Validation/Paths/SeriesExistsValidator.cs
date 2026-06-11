using System;
using System.Linq;
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
            dynamic instance = context.ParentContext.InstanceToValidate;
            string seriesEdition = SeriesEditions.Standard;

            try
            {
                seriesEdition = instance.SeriesEdition;
            }
            catch
            {
                seriesEdition = SeriesEditions.Standard;
            }

            seriesEdition = SeriesEditions.Normalize(seriesEdition);

            return !_seriesService.AllSeriesTvdbIdEditions()
                                  .Any(s => s.TvdbId == tvdbId && s.SeriesEdition == seriesEdition);
        }
    }
}
