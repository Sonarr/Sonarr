using FluentValidation;
using Workarr.Extensions;
using Workarr.Validation;
using Workarr.Validation.Paths;

namespace Sonarr.Api.V3.Series
{
    public class SeriesEditorValidator : AbstractValidator<Workarr.Tv.Series>
    {
        public SeriesEditorValidator(RootFolderExistsValidator rootFolderExistsValidator, QualityProfileExistsValidator qualityProfileExistsValidator)
        {
            RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator)
                .When(s => s.RootFolderPath.IsNotNullOrWhiteSpace());

            RuleFor(c => c.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);
        }
    }
}
