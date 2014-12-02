using FluentValidation.Validators;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using Omu.ValueInjecter;

namespace NzbDrone.Core.Validation.Paths
{
    public class SeriesPathValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;

        public SeriesPathValidator(ISeriesService seriesService)
            : base("Path is already configured for another series")
        {
            _seriesService = seriesService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            var series = new Series();
            series.InjectFrom(context.ParentContext.InstanceToValidate);

            if (series.Id == 0) return true;

            return (!_seriesService.GetAllSeries().Exists(s => s.Path.PathEquals(context.PropertyValue.ToString()) && s.Id != series.Id));
        }
    }
}