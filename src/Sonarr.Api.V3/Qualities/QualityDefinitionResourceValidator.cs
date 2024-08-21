using FluentValidation;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V3.Qualities;

public class QualityDefinitionResourceValidator : AbstractValidator<QualityDefinitionResource>
{
    public QualityDefinitionResourceValidator(QualityDefinitionLimits limits)
    {
        When(c => c.MinSize is not null, () =>
        {
            RuleFor(c => c.MinSize)
                .GreaterThanOrEqualTo(limits.MinLimit)
                .WithErrorCode("GreaterThanOrEqualTo")
                .LessThanOrEqualTo(c => c.PreferredSize ?? limits.MaxLimit)
                .WithErrorCode("LessThanOrEqualTo");
        });

        When(c => c.MaxSize is not null, () =>
        {
            RuleFor(c => c.MaxSize)
                .GreaterThanOrEqualTo(c => c.PreferredSize ?? limits.MinLimit)
                .WithErrorCode("GreaterThanOrEqualTo")
                .LessThanOrEqualTo(limits.MaxLimit)
                .WithErrorCode("LessThanOrEqualTo");
        });
    }
}
