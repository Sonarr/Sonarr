using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatRepository : IBasicRepository<CustomFormat>
    {
    }

    public class CustomFormatRepository : BasicRepository<CustomFormat>, ICustomFormatRepository
    {
        public CustomFormatRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
