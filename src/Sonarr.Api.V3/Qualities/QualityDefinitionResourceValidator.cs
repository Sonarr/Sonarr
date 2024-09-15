using FluentValidation;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V3.Qualities;

public class QualityDefinitionResourceValidator : AbstractValidator<QualityDefinitionResource>
{
    public QualityDefinitionResourceValidator()
    {
        RuleFor(c => c.MinSize)
            .GreaterThanOrEqualTo(QualityDefinitionLimits.Min)
            .WithErrorCode("GreaterThanOrEqualTo")
            .LessThanOrEqualTo(c => c.PreferredSize ?? QualityDefinitionLimits.Max)
            .WithErrorCode("LessThanOrEqualTo")
            .When(c => c.MinSize is not null);

        RuleFor(c => c.PreferredSize)
            .GreaterThanOrEqualTo(c => c.MinSize ?? QualityDefinitionLimits.Min)
            .WithErrorCode("GreaterThanOrEqualTo")
            .LessThanOrEqualTo(c => c.MaxSize ?? QualityDefinitionLimits.Max)
            .WithErrorCode("LessThanOrEqualTo")
            .When(c => c.PreferredSize is not null);

        RuleFor(c => c.MaxSize)
            .GreaterThanOrEqualTo(c => c.PreferredSize ?? QualityDefinitionLimits.Min)
            .WithErrorCode("GreaterThanOrEqualTo")
            .LessThanOrEqualTo(QualityDefinitionLimits.Max)
            .WithErrorCode("LessThanOrEqualTo")
            .When(c => c.MaxSize is not null);
    }
}
