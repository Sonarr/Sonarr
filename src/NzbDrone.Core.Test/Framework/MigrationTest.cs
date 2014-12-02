using System;
using FluentMigrator;
using NUnit.Framework;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Test.Framework
{
    [Category("DbMigrationTest")]
    [Category("DbTest")]
    public abstract class MigrationTest<TMigration> : DbTest where TMigration : MigrationBase
    {
        protected override TestDatabase WithTestDb(Action<MigrationBase> beforeMigration)
        {
            var factory = Mocker.Resolve<DbFactory>();

            var database = factory.Create(MigrationType, m =>
            {
                if (m.GetType() == typeof(TMigration))
                {
                    beforeMigration(m);
                }
            });

            var testDb = new TestDatabase(database);
            Mocker.SetConstant(database);

            return testDb;
        }

        [SetUp]
        public override void SetupDb()
        {
            SetupContainer();
        }
    }
}