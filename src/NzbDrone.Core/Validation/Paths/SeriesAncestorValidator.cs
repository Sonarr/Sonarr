using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Validation.Paths
{
    public class SeriesAncestorValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;

        public SeriesAncestorValidator(ISeriesService seriesService)
            : base("Path is an ancestor of an existing series")
        {
            _seriesService = seriesService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return !_seriesService.GetAllSeriesPaths().Any(s => context.PropertyValue.ToString().IsParentPath(s.Value));
        }
    }
}
