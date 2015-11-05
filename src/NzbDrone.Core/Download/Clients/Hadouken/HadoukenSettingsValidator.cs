using FluentValidation;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public class HadoukenSettingsValidator : AbstractValidator<HadoukenSettings>
    {
        public HadoukenSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).GreaterThan(0);

            RuleFor(c => c.Username).NotEmpty()
                                    .WithMessage("Username must not be empty.");

            RuleFor(c => c.Password).NotEmpty()
                                    .WithMessage("Password must not be empty.");
        }
    }
}
