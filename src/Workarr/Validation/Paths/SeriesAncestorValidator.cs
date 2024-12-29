using FluentValidation.Validators;
using Workarr.Extensions;
using Workarr.Tv;

namespace Workarr.Validation.Paths
{
    public class SeriesAncestorValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;

        public SeriesAncestorValidator(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is an ancestor of an existing series";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            return !Enumerable.Any<KeyValuePair<int, string>>(_seriesService.GetAllSeriesPaths(), s => context.PropertyValue.ToString().IsParentPath(s.Value));
        }
    }
}
