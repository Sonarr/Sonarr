using System;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Growl
{
    public class GrowlSettings : IProviderConfig
    {
        public GrowlSettings()
        {
            Port = 23053;
        }

        [FieldDefinition(0, Label = "Host")]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Password")]
        public String Password { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Password) && Port > 0;
            }
        }

        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
