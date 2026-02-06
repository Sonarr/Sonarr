using FluentValidation;
using NzbDrone.Core.Configuration;
using Sonarr.Http;

namespace Sonarr.Api.V5.Config;

/// <summary>
/// Supported TVDB/SkyHook metadata language codes (ISO 639-1).
/// </summary>
public static class TvdbMetadataLanguageCodes
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        new[]
        {
            "en", "de", "fr", "es", "it", "tr", "pt", "nl", "pl", "ru", "ja", "zh",
            "sv", "da", "fi", "no", "ko", "cs", "hu", "el", "ro", "th", "uk", "id",
            "ms", "he", "ar", "hi"
        },
        StringComparer.OrdinalIgnoreCase);
}

[V5ApiController("settings/metadatasource")]
public class MetadataSourceSettingsController : ConfigController<MetadataSourceSettingsResource>
{
    public MetadataSourceSettingsController(IConfigService configService)
        : base(configService)
    {
        SharedValidator.RuleFor(c => c.TvdbMetadataLanguage)
            .NotEmpty()
            .WithMessage("TVDB metadata language cannot be empty");

        SharedValidator.RuleFor(c => c.TvdbMetadataLanguage)
            .Must(code => TvdbMetadataLanguageCodes.All.Contains(code))
            .WithMessage("Invalid TVDB metadata language code. Use a supported ISO 639-1 code (e.g. en, de, fr).");
    }

    protected override MetadataSourceSettingsResource ToResource(IConfigService model)
    {
        return MetadataSourceSettingsResourceMapper.ToResource(model);
    }
}
