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
        var resource = new QualityDefinitionResource
        {
            MinSize = QualityDefinitionLimits.Min - 1,
            PreferredSize = null,
            MaxSize = null
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MinSize)
            .WithErrorCode("GreaterThanOrEqualTo");
    }

    [Test]
    public void Validate_fails_when_min_size_is_above_preferred_size_and_below_limit()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = 10,
            PreferredSize = 5,
            MaxSize = null
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MinSize)
            .WithErrorCode("LessThanOrEqualTo");

        result.ShouldHaveValidationErrorFor(r => r.PreferredSize)
            .WithErrorCode("GreaterThanOrEqualTo");
    }

    [Test]
    public void Validate_passes_when_min_size_is_within_limits()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = QualityDefinitionLimits.Min,
            PreferredSize = null,
            MaxSize = null
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_fails_when_max_size_is_below_preferred_size_and_above_limit()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = null,
            PreferredSize = 10,
            MaxSize = 5
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MaxSize)
            .WithErrorCode("GreaterThanOrEqualTo");

        result.ShouldHaveValidationErrorFor(r => r.PreferredSize)
            .WithErrorCode("LessThanOrEqualTo");
    }

    [Test]
    public void Validate_fails_when_max_size_exceeds_max_limit()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = null,
            PreferredSize = null,
            MaxSize = QualityDefinitionLimits.Max + 1
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MaxSize)
            .WithErrorCode("LessThanOrEqualTo");
    }

    [Test]
    public void Validate_passes_when_max_size_is_within_limits()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = null,
            PreferredSize = null,
            MaxSize = QualityDefinitionLimits.Max
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_fails_when_preferred_size_is_below_min_size_and_above_max_size()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = 10,
            PreferredSize = 7,
            MaxSize = 5
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.PreferredSize)
            .WithErrorCode("GreaterThanOrEqualTo");

        result.ShouldHaveValidationErrorFor(r => r.MaxSize)
            .WithErrorCode("GreaterThanOrEqualTo");
    }

    [Test]
    public void Validate_passes_when_preferred_size_is_null_and_other_sizes_are_valid()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = 5,
            PreferredSize = null,
            MaxSize = 10
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_passes_when_preferred_size_equals_limits()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = 5,
            PreferredSize = 5,
            MaxSize = 10
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_fails_when_all_sizes_are_provided_and_invalid()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = 15,
            PreferredSize = 10,
            MaxSize = 5
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.MinSize)
            .WithErrorCode("LessThanOrEqualTo");

        result.ShouldHaveValidationErrorFor(r => r.MaxSize)
            .WithErrorCode("GreaterThanOrEqualTo");

        result.ShouldHaveValidationErrorFor(r => r.PreferredSize)
            .WithErrorCode("GreaterThanOrEqualTo");
    }

    [Test]
    public void Validate_passes_when_preferred_size_is_valid_within_limits()
    {
        var resource = new QualityDefinitionResource
        {
            MinSize = 5,
            PreferredSize = 7,
            MaxSize = 10
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
