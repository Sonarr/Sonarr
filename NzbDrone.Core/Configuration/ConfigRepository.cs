using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging;


namespace NzbDrone.Core.Configuration
{
    public interface IConfigRepository : IBasicRepository<Config>
    {
        Config Get(string key);

    }

    public class ConfigRepository : BasicRepository<Config>, IConfigRepository
    {
        public ConfigRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
        }


        public Config Get(string key)
        {
            return Query.SingleOrDefault(c => c.Key == key);
        }


    }
}