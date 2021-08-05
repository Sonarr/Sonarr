using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using NzbDrone.Common.Extensions;
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

            var usedTags = new[] { "Series", "Notifications", "DelayProfiles", "ReleaseProfiles", "ImportLists", "Indexers" }
                .SelectMany(v => GetUsedTags(v, mapper))
                .Distinct()
                .ToList();

            var usedTagsList = usedTags.Select(d => d.ToString()).Join(",");

            mapper.ExecuteNonQuery($"DELETE FROM Tags WHERE NOT Id IN ({usedTagsList})");
        }

        private int[] GetUsedTags(string table, IDataMapper mapper)
        {
            return mapper.ExecuteReader($"SELECT DISTINCT Tags FROM {table} WHERE NOT Tags = '[]' AND NOT Tags IS NULL", reader => reader.GetString(0))
                         .SelectMany(Json.Deserialize<List<int>>)
                         .Distinct()
                         .ToArray();
        }
    }
}
