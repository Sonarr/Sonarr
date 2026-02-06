using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Configuration;
using Sonarr.Http;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/metadatasource")]
    public class MetadataSourceConfigController : ConfigController<MetadataSourceConfigResource>
    {
        private static readonly HashSet<string> TvdbMetadataLanguageCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "en", "de", "fr", "es", "it", "tr", "pt", "nl", "pl", "ru", "ja", "zh", "sv", "da", "fi", "no", "ko", "cs", "hu", "el", "ro", "th", "uk", "id", "ms", "he", "ar", "hi"
        };

        public MetadataSourceConfigController(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.TvdbMetadataLanguage)
                .NotEmpty()
                .WithMessage("TVDB metadata language cannot be empty");

            SharedValidator.RuleFor(c => c.TvdbMetadataLanguage)
                .Must(code => TvdbMetadataLanguageCodes.Contains(code))
                .WithMessage("Invalid TVDB metadata language code. Use a supported ISO 639-1 code (e.g. en, de, fr).");
        }

        protected override MetadataSourceConfigResource ToResource(IConfigService model)
        {
            return MetadataSourceConfigResourceMapper.ToResource(model);
        }
    }
}
