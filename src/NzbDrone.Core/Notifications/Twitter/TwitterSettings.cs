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
            ConsumerKey = "3POVsO3KW90LKZXyzPOjQ"; /* FIXME - Key from Couchpotato so needs to be replaced */
            ConsumerSecret = "Qprb94hx9ucXvD4Wvg2Ctsk4PDK7CcQAKgCELXoyIjE"; /* FIXME - Key from Couchpotato so needs to be replaced */
            AuthorizeNotification = "step1";
        }

        [FieldDefinition(0, Label = "Access Token", Advanced = true)]
        public String AccessToken { get; set; }

        [FieldDefinition(1, Label = "Access Token Secret", Advanced = true)]
        public String AccessTokenSecret { get; set; }

        public String ConsumerKey { get; set; }
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
