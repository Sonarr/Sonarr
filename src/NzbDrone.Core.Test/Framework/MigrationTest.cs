using System;
using System.Data;
using FluentMigrator;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Test.Common.AutoMoq;

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
                    if (beforeMigration != null && migration is TMigration)
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
            SetupContainer();
        }

        [Obsolete("Don't use Mocker/Repositories in MigrationTests, query the DB.", true)]
        public new AutoMoqer Mocker
        {
            get { return base.Mocker; }
        }
    }
}