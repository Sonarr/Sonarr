using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public class Aria2SettingsValidator : AbstractValidator<Aria2Settings>
    {
        public Aria2SettingsValidator()
        {
            RuleFor(c => c.RPCUrl).ValidRootUrl();
        }
    }

    public class Aria2Settings : IProviderConfig
    {
        private static readonly Aria2SettingsValidator Validator = new Aria2SettingsValidator();

        public Aria2Settings()
        {
            RPCUrl = "http://localhost:6800/rpc";
            SecretToken = "MySecretToken";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Url)]
        public string RPCUrl { get; set; }

        [FieldDefinition(1, Label = "Secret token", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string SecretToken { get; set; }

        internal string RPCToken => $"token:{SecretToken}";

        public NzbDroneValidationResult Validate()
        {
                return new NzbDroneValidationResult(Validator.Validate(this));
            }
    }
}