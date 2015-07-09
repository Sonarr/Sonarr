using FluentValidation;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public sealed class HadoukenSettingsValidator : AbstractValidator<HadoukenSettings>
    {
        public HadoukenSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).GreaterThan(0);

            RuleFor(c => c.Token).NotEmpty()
                                 .When(c => c.AuthenticationType == (int) AuthenticationType.Token)
                                 .WithMessage("Token must not be empty when using token auth.");

            RuleFor(c => c.Username).NotEmpty()
                                    .When(c => c.AuthenticationType == (int)AuthenticationType.Basic)
                                    .WithMessage("Username must not be empty when using basic auth.");

            RuleFor(c => c.Password).NotEmpty()
                                    .When(c => c.AuthenticationType == (int)AuthenticationType.Basic)
                                    .WithMessage("Password must not be empty when using basic auth.");
        }
    }
}
