using System;
using FluentMigrator;
using FluentMigrator.Runner;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Test.Framework
{
    [Category("DbMigrationTest")]
    [Category("DbTest")]
    public abstract class MigrationTest<TMigration> : DbTest where TMigration : NzbDroneMigrationBase
    {
        protected long MigrationVersion
        {
            get
            {
                var attrib = (MigrationAttribute)Attribute.GetCustomAttribute(typeof(TMigration), typeof(MigrationAttribute));
                return attrib.Version;
            }
        }

        protected virtual IDirectDataMapper WithMigrationTestDb(Action<TMigration> beforeMigration = null)
        {
            var db = WithTestDb(new MigrationContext(MigrationType, MigrationVersion)
            {
                BeforeMigration = m =>
                {
                    var migration = m as TMigration;
                    if (beforeMigration != null && migration != null)
                    {
                        beforeMigration(migration);
                    }
                }
            });

            return db.GetDirectDataMapper();
        }

        [SetUp]
        public override void SetupDb()
        {
            Mocker.SetConstant<IAnnouncer>(Mocker.Resolve<MigrationLogger>());
            SetupContainer();
        }
    }
}