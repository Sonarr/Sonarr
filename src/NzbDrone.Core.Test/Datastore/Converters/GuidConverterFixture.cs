using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class GuidConverterFixture : CoreTest<GuidConverter>
    {
        [Test]
        public void should_return_string_when_saving_guid_to_db()
        {
            var guid = Guid.NewGuid();

            Subject.ToDB(guid).Should().Be(guid.ToString());
        }

        [Test]
        public void should_return_db_null_for_null_value_when_saving_to_db()
        {
            Subject.ToDB(null).Should().Be(DBNull.Value);
        }

        [Test]
        public void should_return_guid_when_getting_string_from_db()
        {
            var guid = Guid.NewGuid();

            var context = new ConverterContext
                          {
                              DbValue = guid.ToString()
                          };

            Subject.FromDB(context).Should().Be(guid);
        }

        [Test]
        public void should_return_empty_guid_for_db_null_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = DBNull.Value
                          };

            Subject.FromDB(context).Should().Be(Guid.Empty);
        }
    }
}
