using System;
using System.Data;
using FluentMigrator;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Test.Framework
{
    [Category("DbMigrationTest")]
    [Category("DbTest")]
    public abstract class MigrationTest<TMigration> : DbTest
        where TMigration : NzbDroneMigrationBase
    {
        protected long MigrationVersion => ((MigrationAttribute)Attribute.GetCustomAttribute(typeof(TMigration), typeof(MigrationAttribute))).Version;

        [SetUp]
        public override void SetupDb()
        {
            SetupContainer();
        }

        protected virtual IDirectDataMapper WithMigrationTestDb(Action<TMigration> beforeMigration = null)
        {
            return WithMigrationAction(beforeMigration).GetDirectDataMapper();
        }

        protected virtual IDbConnection WithDapperMigrationTestDb(Action<TMigration> beforeMigration = null)
        {
            return WithMigrationAction(beforeMigration).OpenConnection();
        }

        protected override void SetupLogging()
        {
            Mocker.SetConstant<ILoggerProvider>(Mocker.Resolve<NLogLoggerProvider>());
        }

        private ITestDatabase WithMigrationAction(Action<TMigration> beforeMigration = null)
        {
            return WithTestDb(new MigrationContext(MigrationType, MigrationVersion)
            {
                BeforeMigration = m =>
                {
                    if (beforeMigration != null && m is TMigration migration)
                    {
                        beforeMigration(migration);
                    }
                }
            });
        }
    }
}
