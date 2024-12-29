using FluentValidation.Validators;
using Workarr.Disk;
using Workarr.Extensions;
using Workarr.Tv;

namespace Workarr.Validation.Paths
{
    public class SeriesPathValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;

        public SeriesPathValidator(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is already configured for another series";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            // Skip the path for this series and any invalid paths
            return !Enumerable.Any<KeyValuePair<int, string>>(_seriesService.GetAllSeriesPaths(), s => s.Key != instanceId &&
                                                                                                                               PathExtensions.IsPathValid(s.Value, PathValidationType.CurrentOs) &&
                                                                                                                               PathExtensions.PathEquals(s.Value, context.PropertyValue.ToString()));
        }
    }
}
