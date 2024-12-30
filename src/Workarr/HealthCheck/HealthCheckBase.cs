using Workarr.Localization;

namespace Workarr.HealthCheck
{
    public abstract class HealthCheckBase : IProvideHealthCheck
    {
        public readonly ILocalizationService _localizationService;

        public HealthCheckBase(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public abstract HealthCheck Check();

        public virtual bool CheckOnStartup => true;

        public virtual bool CheckOnSchedule => true;
    }
}
