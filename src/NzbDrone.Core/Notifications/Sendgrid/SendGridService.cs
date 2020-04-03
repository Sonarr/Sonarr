using System;
using System.Linq;
using NLog;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Http.Headers;
using System.Net.Mail;
using FluentValidation.Results;
using NzbDrone.Core.Notifications.Sendgrid.Models;

namespace NzbDrone.Core.Notifications.Sendgrid
{

    public interface ISendGridService
    {
        SendGridResponse Send(SendgridSettings settings, string subject, string body);
        ValidationFailure Test(SendgridSettings settings);
    }


    public class SendGridService : ISendGridService
    {
        private readonly Logger _logger;
        private readonly SendGridClient _client;
        private string apiKey = SendgridSettings.ApiKey;
        private static readonly string MessageId = "X-Message-Id";

        public SendGridService(Logger logger)
        {
            _logger = logger;
            _client = new SendGridClient(apiKey);
        }


        public SendGridResponse Send(SendgridSettings settings, string subject, string body)
        {
            var emailMessage = new SendGridMessage()
            {
                From = new EmailAddress(settings.From),
                Subject = subject, 
                HtmlContent = body
            };
            
            
            emailMessage.AddTo(new EmailAddress(settings.To));
            
            return ProcessResponse(_client.SendEmailAsync(emailMessage).Result);
        }

        public ValidationFailure Test(SendgridSettings settings)
        {
            const string body = "Success! You have properly configured your SendGrid notification settings";

                try
                {
                    Send(settings, "Sonarr - Test Notification", body);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to send test email though SendGrid");
                    return new ValidationFailure("apiKey", "Unable to send test email though SendGrid");
                }

                return null;
        }

        private SendGridResponse ProcessResponse(Response response)
        {
            if (response.StatusCode.Equals(System.Net.HttpStatusCode.Accepted)
                || response.StatusCode.Equals(System.Net.HttpStatusCode.OK))
            {
                return ToMailResponse(response);
            }

            //TODO check for null
            var errorResponse = response.Body.ReadAsStringAsync().Result;

            throw new SendGridServiceException(response.StatusCode.ToString(), errorResponse);
        }
        
        private static SendGridResponse ToMailResponse(Response response)
        {
            if (response == null)
                return null;

            var headers = (HttpHeaders)response.Headers;
            var messageId = headers.GetValues(MessageId).FirstOrDefault();
            return new SendGridResponse()
            {
                UniqueMessageId = messageId,
                DateSent = DateTime.UtcNow,
            };
        }
    }
}