using Workarr.Datastore;

namespace Workarr.ThingiProvider.Status
{
    public abstract class ProviderStatusBase : ModelBase
    {
        public int ProviderId { get; set; }

        public DateTime? InitialFailure { get; set; }
        public DateTime? MostRecentFailure { get; set; }
        public int EscalationLevel { get; set; }
        public DateTime? DisabledTill { get; set; }

        public virtual bool IsDisabled()
        {
            return DisabledTill.HasValue && DisabledTill.Value > DateTime.UtcNow;
        }
    }
}
