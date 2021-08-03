using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public class AirDCPPSettingsValidator : AbstractValidator<AirDCPPSettings>
    {
        public AirDCPPSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.BaseUrl).ValidUrlBase().When(c => c.BaseUrl.IsNotNullOrWhiteSpace());

            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.Delay).GreaterThanOrEqualTo(0);
        }
    }

    public class AirDCPPSettings : IIndexerSettings
    {
        private static readonly AirDCPPSettingsValidator Validator = new AirDCPPSettingsValidator();

        public AirDCPPSettings()
        {
            Host = "localhost";
            Port = 5600;
            Delay = 1000;
            MultiLanguages = new List<int>();
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Use a secure connection.")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the AirDC++ url, e.g. http://[host]:[port]/[urlBase]/api/v1")]
        public string BaseUrl { get; set; }

        [FieldDefinition(4, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(5, Type = FieldType.Password, Label = "Password")]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Delay", HelpText = "Time in milliseconds to wait between retrieving hub search results", Advanced = true)]
        public int Delay { get; set; }

        public IEnumerable<int> MultiLanguages { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
