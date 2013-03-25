using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Processors;

namespace NzbDrone.Core.Datastore.Migration.Sqlite
{
    public class MonoSqliteDbFactory : ReflectionBasedDbFactory
    {
        public MonoSqliteDbFactory()
            : base("Mono.Data.Sqlite", "Mono.Data.Sqlite.SqliteFactory")
        {
        }

    
    }
}
