using System;
using System.Globalization;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class TimeSpanConverterFixture : CoreTest<TimeSpanConverter>
    {
        [Test]
        public void should_return_string_when_saving_timespan_to_db()
        {
            var timeSpan = TimeSpan.FromMinutes(5);

            Subject.ToDB(timeSpan).Should().Be(timeSpan.ToString("c", CultureInfo.InvariantCulture));
        }

        [Test]
        public void should_return_null_when_saving_empty_string_to_db()
        {
            Subject.ToDB("").Should().Be(null);
        }

        [Test]
        public void should_return_time_span_when_getting_time_span_from_db()
        {
            var timeSpan = TimeSpan.FromMinutes(5);

            var context = new ConverterContext
                          {
                              DbValue = timeSpan
                          };

            Subject.FromDB(context).Should().Be(timeSpan);
        }

        [Test]
        public void should_return_time_span_when_getting_string_from_db()
        {
            var timeSpan = TimeSpan.FromMinutes(5);

            var context = new ConverterContext
                          {
                              DbValue = timeSpan.ToString("c", CultureInfo.InvariantCulture)
                          };

            Subject.FromDB(context).Should().Be(timeSpan);
        }

        [Test]
        public void should_return_time_span_zero_for_db_null_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = DBNull.Value
                          };

            Subject.FromDB(context).Should().Be(TimeSpan.Zero);
        }
    }
}
