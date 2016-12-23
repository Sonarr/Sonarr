using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupUnusedTags : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupUnusedTags(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.GetDataMapper();

            var usedTags = new[] { "Series", "Notifications", "DelayProfiles", "Restrictions" }
                .SelectMany(v => GetUsedTags(v, mapper))
                .Distinct()
                .ToArray();

            var usedTagsList = string.Join(",", usedTags.Select(d => d.ToString()).ToArray());

            mapper.ExecuteNonQuery($"DELETE FROM Tags WHERE NOT Id IN ({usedTagsList})");
        }

        private int[] GetUsedTags(string table, IDataMapper mapper)
        {
            return mapper.ExecuteReader($"SELECT DISTINCT Tags FROM {table} WHERE NOT Tags = '[]'", reader => reader.GetString(0))
                         .SelectMany(Json.Deserialize<List<int>>)
                         .Distinct()
                         .ToArray();
        }
    }
}
