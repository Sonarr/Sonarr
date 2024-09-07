using FluentValidation;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V3.Qualities;

public class QualityDefinitionResourceValidator : AbstractValidator<QualityDefinitionResource>
{
    public QualityDefinitionResourceValidator()
    {
        RuleFor(c => c.MinSize)
            .GreaterThanOrEqualTo(QualityDefinitionLimits.MinLimit)
            .WithErrorCode("GreaterThanOrEqualTo")
            .LessThanOrEqualTo(c => c.PreferredSize ?? QualityDefinitionLimits.MaxLimit)
            .WithErrorCode("LessThanOrEqualTo")
            .When(c => c.MinSize is not null);

        RuleFor(c => c.PreferredSize)
            .GreaterThanOrEqualTo(c => c.MinSize ?? QualityDefinitionLimits.MinLimit)
            .WithErrorCode("GreaterThanOrEqualTo")
            .LessThanOrEqualTo(c => c.MaxSize ?? QualityDefinitionLimits.MaxLimit)
            .WithErrorCode("LessThanOrEqualTo")
            .When(c => c.PreferredSize is not null);

        RuleFor(c => c.MaxSize)
            .GreaterThanOrEqualTo(c => c.PreferredSize ?? QualityDefinitionLimits.MinLimit)
            .WithErrorCode("GreaterThanOrEqualTo")
            .LessThanOrEqualTo(QualityDefinitionLimits.MaxLimit)
            .WithErrorCode("LessThanOrEqualTo")
            .When(c => c.MaxSize is not null);
    }
}
