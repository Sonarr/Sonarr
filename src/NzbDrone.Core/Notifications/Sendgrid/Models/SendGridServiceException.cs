using System;
using System.Runtime.Serialization;

namespace NzbDrone.Core.Notifications.Sendgrid.Models
{
    public class SendGridServiceException : Exception
    {
        public string Body { get; private set; }

        public SendGridServiceException(string message, string body) : base (message)
        {
            Body = body;
        }

        public SendGridServiceException(string message, string body, Exception innerException) : base(message,
            innerException)
        {
            Body = body;
        }
    }

    public class SendGridResponse
    {
        public DateTime DateSent { get; internal set; }
        public string UniqueMessageId { get; internal set; }
    }
}