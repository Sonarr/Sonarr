using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
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
            using (var mapper = _database.OpenConnection())
            {
                var usedTags = new[] { "Series", "Notifications", "DelayProfiles", "ReleaseProfiles", "ImportLists", "Indexers" }
                    .SelectMany(v => GetUsedTags(v, mapper))
                    .Distinct()
                    .ToArray();

                var usedTagsList = string.Join(",", usedTags.Select(d => d.ToString()).ToArray());

                mapper.Execute($"DELETE FROM Tags WHERE NOT Id IN ({usedTagsList})");
            }
        }

        private int[] GetUsedTags(string table, IDbConnection mapper)
        {
            return mapper.Query<List<int>>($"SELECT DISTINCT Tags FROM {table} WHERE NOT Tags = '[]' AND NOT Tags IS NULL")
                .SelectMany(x => x)
                .Distinct()
                .ToArray();
        }
    }
}
