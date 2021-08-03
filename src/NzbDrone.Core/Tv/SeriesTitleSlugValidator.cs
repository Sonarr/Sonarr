using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Tv
{
    public class SeriesTitleSlugValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;

        public SeriesTitleSlugValidator(ISeriesService seriesService)
            : base("Title slug '{slug}' is in use by series '{seriesTitle}'")
        {
            _seriesService = seriesService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;
            var slug = context.PropertyValue.ToString();

            var conflictingSeries = _seriesService.GetAllSeries()
                                                  .FirstOrDefault(s => s.TitleSlug.IsNotNullOrWhiteSpace() &&
                                                              s.TitleSlug.Equals(context.PropertyValue.ToString()) &&
                                                              s.Id != instanceId);

            if (conflictingSeries == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("slug", slug);
            context.MessageFormatter.AppendArgument("seriesTitle", conflictingSeries.Title);

            return false;
        }
    }
}
