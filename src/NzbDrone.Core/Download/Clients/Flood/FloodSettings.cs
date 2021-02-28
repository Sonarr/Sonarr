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

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Use secure connection when connecting to Flood")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Url Base", Type = FieldType.Textbox, HelpText = "Optionally adds a prefix to Flood API, such as [protocol]://[host]:[port]/[urlBase]api")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Destination", Type = FieldType.Textbox, HelpText = "Manually specifies download destination")]
        public string Destination { get; set; }

        [FieldDefinition(7, Label = "Tags", Type = FieldType.Tag, HelpText = "Initial tags of a download. To be recognized, a download must have all initial tags. This avoids conflicts with unrelated downloads.")]
        public IEnumerable<string> Tags { get; set; }

        [FieldDefinition(8, Label = "Post-Import Tags", Type = FieldType.Tag, HelpText = "Appends tags after a download is imported.", Advanced = true)]
        public IEnumerable<string> PostImportTags { get; set; }

        [FieldDefinition(9, Label = "Additional Tags", Type = FieldType.Select, SelectOptions = typeof(AdditionalTags), HelpText = "Adds properties of media as tags. Hints are examples.", Advanced = true)]
        public IEnumerable<int> AdditionalTags { get; set; }

        [FieldDefinition(10, Label = "Start on Add", Type = FieldType.Checkbox)]
        public bool StartOnAdd { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
