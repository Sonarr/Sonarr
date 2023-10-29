using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Download.Clients.Flood.Models;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Flood
{
    public class FloodSettingsValidator : AbstractValidator<FloodSettings>
    {
        public FloodSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
        }
    }

    public class FloodSettings : IProviderConfig
    {
        private static readonly FloodSettingsValidator Validator = new FloodSettingsValidator();

        public FloodSettings()
        {
            UseSsl = false;
            Host = "localhost";
            Port = 3000;
            Tags = new string[]
            {
                "sonarr"
            };
            AdditionalTags = Enumerable.Empty<int>();
            StartOnAdd = true;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "clientName", "Flood")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "UrlBase", Type = FieldType.Textbox, HelpText = "DownloadClientFloodSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "[protocol]://[host]:[port]/[urlBase]/api")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Destination", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsDestinationHelpText")]
        public string Destination { get; set; }

        [FieldDefinition(7, Label = "Tags", Type = FieldType.Tag, HelpText = "DownloadClientFloodSettingsTagsHelpText")]
        public IEnumerable<string> Tags { get; set; }

        [FieldDefinition(8, Label = "DownloadClientFloodSettingsPostImportTags", Type = FieldType.Tag, HelpText = "DownloadClientFloodSettingsPostImportTagsHelpText", Advanced = true)]
        public IEnumerable<string> PostImportTags { get; set; }

        [FieldDefinition(9, Label = "DownloadClientFloodSettingsAdditionalTags", Type = FieldType.Select, SelectOptions = typeof(AdditionalTags), HelpText = "DownloadClientFloodSettingsAdditionalTagsHelpText", Advanced = true)]
        public IEnumerable<int> AdditionalTags { get; set; }

        [FieldDefinition(10, Label = "DownloadClientFloodSettingsStartOnAdd", Type = FieldType.Checkbox)]
        public bool StartOnAdd { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
