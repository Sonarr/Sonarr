using Workarr.Datastore;
using Workarr.Extras.Files;
using Workarr.Messaging.Events;

namespace Workarr.Extras.Others
{
    public interface IOtherExtraFileRepository : IExtraFileRepository<OtherExtraFile>
    {
    }

    public class OtherExtraFileRepository : ExtraFileRepository<OtherExtraFile>, IOtherExtraFileRepository
    {
        public OtherExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
