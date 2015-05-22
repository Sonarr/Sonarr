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
            return base.WithTestDb(m =>
            {
                if (m.GetType() == typeof(TMigration))
                {
                    beforeMigration(m);
                }
            });
        }

        [SetUp]
        public override void SetupDb()
        {
            SetupContainer();
        }
    }
}