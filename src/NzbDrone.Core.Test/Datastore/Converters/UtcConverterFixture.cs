using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class UtcConverterFixture : CoreTest<UtcConverter>
    {
        [Test]
        public void should_return_date_time_when_saving_date_time_to_db()
        {
            var dateTime = DateTime.Now;

            Subject.ToDB(dateTime).Should().Be(dateTime.ToUniversalTime());
        }

        [Test]
        public void should_return_db_null_when_saving_db_null_to_db()
        {
            Subject.ToDB(DBNull.Value).Should().Be(DBNull.Value);
        }

        [Test]
        public void should_return_time_span_when_getting_time_span_from_db()
        {
            var dateTime = DateTime.Now.ToUniversalTime();

            var context = new ConverterContext
                          {
                              DbValue = dateTime
                          };

            Subject.FromDB(context).Should().Be(dateTime);
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
