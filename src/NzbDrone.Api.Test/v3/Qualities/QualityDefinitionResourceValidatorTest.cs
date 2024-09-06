using FluentValidation.TestHelper;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using Sonarr.Api.V3.Qualities;

namespace NzbDrone.Api.Test.v3.Qualities;

[Parallelizable(ParallelScope.All)]
public class QualityDefinitionResourceValidatorTests
{
    private readonly QualityDefinitionResourceValidator _validator = new ();

    [Test]
    public void Validate_fails_when_min_size_is_below_min_limit()
    {
        var resource = new QualityDefinitionResource { MinSize = QualityDefinitionLimits.MinLimit - 1 };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MinSize)
            .WithErrorCode("GreaterThanOrEqualTo");
    }

    [Test]
    public void Validate_fails_when_min_size_is_above_preferred_size()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = 10,
            PreferredSize = 5
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MinSize)
            .WithErrorCode("LessThanOrEqualTo");
    }

    [Test]
    public void Validate_passes_when_min_size_is_within_limits()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = QualityDefinitionLimits.MinLimit,
            PreferredSize = QualityDefinitionLimits.MaxLimit
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_fails_when_max_size_is_below_preferred_size()
    {
        var resource = new QualityDefinitionResource
        {
            MaxSize = 5,
            PreferredSize = 10
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MaxSize)
            .WithErrorCode("GreaterThanOrEqualTo");
    }

    [Test]
    public void Validate_fails_when_max_size_exceeds_max_limit()
    {
        var resource = new QualityDefinitionResource { MaxSize = QualityDefinitionLimits.MaxLimit + 1 };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MaxSize)
            .WithErrorCode("LessThanOrEqualTo");
    }

    [Test]
    public void Validate_passes_when_max_size_is_within_limits()
    {
        var resource = new QualityDefinitionResource
        {
            MaxSize = QualityDefinitionLimits.MaxLimit,
            PreferredSize = QualityDefinitionLimits.MinLimit
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
