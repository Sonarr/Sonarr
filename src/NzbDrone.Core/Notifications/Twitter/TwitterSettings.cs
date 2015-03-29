using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Twitter
{
    public class TwitterSettingsValidator : AbstractValidator<TwitterSettings>
    {
        public TwitterSettingsValidator()
        {
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.AccessTokenSecret).NotEmpty();
            RuleFor(c => c.ConsumerKey).NotEmpty();
            RuleFor(c => c.ConsumerSecret).NotEmpty();
        }
    }

    public class TwitterSettings : IProviderConfig
    {
        private static readonly TwitterSettingsValidator Validator = new TwitterSettingsValidator();

        public TwitterSettings()
        {
            ConsumerKey = "vHHtcB6WzpWDG6KYlBMr8g"; /* FIXME - Key from Sickbeard */
            ConsumerSecret = "zMqq5CB3f8cWKiRO2KzWPTlBanYmV0VYxSXZ0Pxds0E"; /* FIXME - Key from Sickbeard */
            AuthorizeNotification = "/twitter/step1";

        }

        [FieldDefinition(0, Label = "Access Token", Advanced = true)]
        public String AccessToken { get; set; }

        [FieldDefinition(1, Label = "Access Token Secret", Advanced = true)]
        public String AccessTokenSecret { get; set; }

        [FieldDefinition(2, Label = "Consumer Key", Advanced = true)]
        public String ConsumerKey { get; set; }

        [FieldDefinition(3, Label = "Consumer Key Secret", Advanced = true)]
        public String ConsumerSecret { get; set; }

        [FieldDefinition(4, Label = "Connect to twitter", Type = FieldType.Action)]
        public String AuthorizeNotification { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(AccessTokenSecret) &&
                    !string.IsNullOrWhiteSpace(ConsumerKey) && !string.IsNullOrWhiteSpace(ConsumerSecret);
            }
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
