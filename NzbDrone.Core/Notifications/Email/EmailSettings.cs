using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Email
{
    public class EmailSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Server", HelpText = "Hostname or IP of Email server")]
        public String Server { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", HelpText = "Does your Email server use SSL?")]
        public Boolean UseSsl { get; set; }

        [FieldDefinition(3, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(4, Label = "Password")]
        public String Password { get; set; }

        [FieldDefinition(5, Label = "Sender Address")]
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
    }
}
