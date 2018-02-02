using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class DoubleConverterFixture : CoreTest<DoubleConverter>
    {
        [Test]
        public void should_return_double_when_saving_double_to_db()
        {
            var input = 10.5D;

            Subject.ToDB(input).Should().Be(input);
        }

        [Test]
        public void should_return_null_for_null_value_when_saving_to_db()
        {
            Subject.ToDB(null).Should().Be(null);
        }

        [Test]
        public void should_return_db_null_for_db_null_value_when_saving_to_db()
        {
            Subject.ToDB(DBNull.Value).Should().Be(DBNull.Value);
        }

        [Test]
        public void should_return_double_when_getting_double_from_db()
        {
            var expected = 10.5D;

            var context = new ConverterContext
                          {
                              DbValue = expected
                          };

            Subject.FromDB(context).Should().Be(expected);
        }

        [Test]
        public void should_return_double_when_getting_string_from_db()
        {
            var expected = 10.5D;

            var context = new ConverterContext
                          {
                              DbValue = $"{expected}"
                          };

            Subject.FromDB(context).Should().Be(expected);
        }

        [Test]
        public void should_return_null_for_null_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = DBNull.Value
                          };

            Subject.FromDB(context).Should().Be(DBNull.Value);
        }
    }
}
