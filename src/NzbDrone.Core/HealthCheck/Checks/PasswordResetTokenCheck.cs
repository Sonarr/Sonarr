using System.Collections.Generic;
using Microsoft.Extensions.Options;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Options;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ApplicationStartedEvent))]
    public class PasswordResetTokenCheck : HealthCheckBase
    {
        private const int MinimumLength = 20;

        private readonly AuthOptions _authOptions;

        public PasswordResetTokenCheck(IOptions<AuthOptions> authOptions, ILocalizationService localizationService)
            : base(localizationService)
        {
            _authOptions = authOptions.Value;
        }

        public override HealthCheck Check()
        {
            if (_authOptions.ResetToken.IsNullOrWhiteSpace())
            {
                return new HealthCheck(GetType());
            }

            if (_authOptions.ResetToken.Length < MinimumLength)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.PasswordResetTokenConfigured,
                    _localizationService.GetLocalizedString("PasswordResetTokenTooShortHealthCheckMessage",
                        new Dictionary<string, object> { { "length", MinimumLength } }),
                    "#password-reset-token");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Notice,
                HealthCheckReason.PasswordResetTokenConfigured,
                _localizationService.GetLocalizedString("PasswordResetTokenConfiguredHealthCheckMessage"),
                "#password-reset-token");
        }
    }
}
