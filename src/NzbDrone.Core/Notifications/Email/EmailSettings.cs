using System;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Email
{
    public class EmailSettings : IProviderConfig
    {
        public EmailSettings()
        {
            Port = 25;
        }

        [FieldDefinition(0, Label = "Server", HelpText = "Hostname or IP of Email server")]
        public String Server { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "SSL", Type = FieldType.Checkbox)]
        public Boolean Ssl { get; set; }

        [FieldDefinition(3, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password)]
        public String Password { get; set; }

        [FieldDefinition(5, Label = "From Address")]
        public String From { get; set; }

        [FieldDefinition(6, Label = "Recipient Address")]
        public String To { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Server) && Port > 0 && !string.IsNullOrWhiteSpace(From) && !string.IsNullOrWhiteSpace(To);
            }
        }

        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
