using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigRepository : IBasicRepository<Config>
    {
        Config Get(string key);

    }

    public class ConfigRepository : BasicRepository<Config>, IConfigRepository
    {
        public ConfigRepository(IDbConnection database)
            : base(database)
        {
        }


        public Config Get(string key)
        {
            return SingleOrDefault(c => c.Key == key);
        }


    }
}