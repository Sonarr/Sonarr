using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class OsPathConverterFixture : CoreTest<OsPathConverter>
    {
        [Test]
        public void should_return_string_when_saving_os_path_to_db()
        {
            var path = @"C:\Test\TV".AsOsAgnostic();
            var osPath = new OsPath(path);

            Subject.ToDB(osPath).Should().Be(path);
        }

        [Test]
        public void should_return_os_path_when_getting_string_from_db()
        {
            var path = @"C:\Test\TV".AsOsAgnostic();
            var osPath = new OsPath(path);

            var context = new ConverterContext
                          {
                              DbValue = path
                          };

            Subject.FromDB(context).Should().Be(osPath);
        }

        [Test]
        public void should_return_db_null_for_db_null_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = DBNull.Value
                          };

            Subject.FromDB(context).Should().Be(DBNull.Value);
        }
    }
}
