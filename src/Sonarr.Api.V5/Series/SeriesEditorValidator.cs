using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace Sonarr.Api.V5.Series;

public class SeriesEditorValidator : AbstractValidator<NzbDrone.Core.Tv.Series>
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

        RuleFor(s => s.Language).Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(l => IsoLanguages.Get(l) != null)
            .WithMessage("Invalid Language value");
    }
}
