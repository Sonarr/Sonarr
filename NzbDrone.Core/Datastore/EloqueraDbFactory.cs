using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Eloquera.Client;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore
{
    public class EloqueraDbFactory
    {
        private readonly EnvironmentProvider _environmentProvider;

        public EloqueraDbFactory()
        {
            _environmentProvider = new EnvironmentProvider();
        }

        public EloqueraDb Create(string dbName = "NzbDrone", string dbFilename = "nzbdrone.eloq")
        {
            DB db = new DB();
            DB.Configuration.ServerSettings.DatabasePath = Path.Combine(_environmentProvider.GetAppDataPath(), dbName);
            db.CreateDatabase(dbName);
            db.OpenDatabase(dbName);
            db.RefreshMode = ObjectRefreshMode.AlwaysReturnUpdatedValues;

            return new EloqueraDb(db);
        }
    }
}
