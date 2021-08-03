using FluentValidation.Validators;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public class ImportListExclusionExistsValidator : PropertyValidator
    {
        private readonly IImportListExclusionService _importListExclusionService;

        public ImportListExclusionExistsValidator(IImportListExclusionService importListExclusionService)
            : base("This exclusion has already been added.")
        {
            _importListExclusionService = importListExclusionService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return !_importListExclusionService.All().Exists(s => s.TvdbId == (int)context.PropertyValue);
        }
    }
}
