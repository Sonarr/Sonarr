using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Tv
{
    public interface IAddSeriesValidator
    {
        ValidationResult Validate(Series instance);
    }

    public class AddSeriesValidator : AbstractValidator<Series>, IAddSeriesValidator
    {
        public AddSeriesValidator(RootFolderValidator rootFolderValidator,
                                  SeriesPathValidator seriesPathValidator,
                                  DroneFactoryValidator droneFactoryValidator,
                                  SeriesAncestorValidator seriesAncestorValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.StopOnFirstFailure)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(seriesPathValidator)
                                .SetValidator(droneFactoryValidator)
                                .SetValidator(seriesAncestorValidator);
        }
    }
}
