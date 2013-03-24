using System.Data;
using System.Linq;
using ServiceStack.OrmLite;

namespace NzbDrone.Core.Datastore
{
    public static class Migration
    {
        public static void CreateTables(IDbConnection dbConnection)
        {
            var types = typeof(ModelBase).Assembly.GetTypes();

            var models = types.Where(c => c.BaseType == typeof(ModelBase));

            foreach (var model in models)
            {
                dbConnection.CreateTable(true, model);
            }
        }

    }
}
