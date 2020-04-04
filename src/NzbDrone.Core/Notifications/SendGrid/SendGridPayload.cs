using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.SendGrid
{
    public class SendGridPayload
    {
        public SendGridPayload()
        {
            Personalizations = new List<SendGridPersonalization>();
            Content = new List<SendGridContent>();
        }

        public List<SendGridContent> Content { get; set; }
        public SendGridEmail From { get; set; }
        public List<SendGridPersonalization> Personalizations { get; set; }
    }

    public class SendGridContent
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class SendGridEmail
    {
        public string Email { get; set; }
    }

    public class SendGridPersonalization
    {
        public SendGridPersonalization()
        {
            To = new List<SendGridEmail>();
        }

        public List<SendGridEmail> To { get; set; }
        public string Subject { get; set; }
    }

    public class SendGridSenderResponse
    {
        public List<SendGridSender> Result { get; set; }
    }

    public class SendGridSender
    {
        public SendGridEmail From { get; set; }
        public string Nickname { get; set; }
    }
}
