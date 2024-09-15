namespace Sonarr.Api.V3.Qualities;

// SA1313 still applies to records until https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3181 is available in a release build.
#pragma warning disable SA1313
public record QualityDefinitionLimitsResource(int Min, int Max);
#pragma warning restore SA1313
