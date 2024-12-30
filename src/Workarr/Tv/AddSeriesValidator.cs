using FluentValidation;
using FluentValidation.Results;
using Workarr.Validation.Paths;

namespace Workarr.Tv
{
    public interface IAddSeriesValidator
    {
        ValidationResult Validate(Series instance);
    }

    public class AddSeriesValidator : AbstractValidator<Series>, IAddSeriesValidator
    {
        public AddSeriesValidator(RootFolderValidator rootFolderValidator,
                                  SeriesPathValidator seriesPathValidator,
                                  SeriesAncestorValidator seriesAncestorValidator,
                                  SeriesTitleSlugValidator seriesTitleSlugValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(seriesPathValidator)
                                .SetValidator(seriesAncestorValidator);

            RuleFor(c => c.TitleSlug).SetValidator(seriesTitleSlugValidator);
        }
    }
}
