using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Notifications.Smtp
{
    public class SmtpSettings : INotifcationSettings
    {
        [FieldDefinition(0, Label = "Server", HelpText = "Hostname or IP of SMTP server")]
        public String Server { get; set; }

        [FieldDefinition(1, Label = "Port", HelpText = "SMTP Server Port")]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", HelpText = "Does your SMTP server use SSL?")]
        public Boolean UseSsl { get; set; }

        [FieldDefinition(3, Label = "Username", HelpText = "SMTP Server Username")]
        public String Username { get; set; }

        [FieldDefinition(4, Label = "Password", HelpText = "SMTP Server Password")]
        public String Password { get; set; }

        [FieldDefinition(5, Label = "From Address", HelpText = "Sender's address")]
        public String From { get; set; }

        [FieldDefinition(6, Label = "To Address", HelpText = "Recipient address")]
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
