using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class BooleanIntConverterFixture : CoreTest<Core.Datastore.Converters.BooleanIntConverter>
    {
        [TestCase(true, 1)]
        [TestCase(false, 0)]
        public void should_return_int_when_saving_boolean_to_db(bool input, int expected)
        {
            Subject.ToDB(input).Should().Be(expected);
        }

        [Test]
        public void should_return_db_null_for_null_value_when_saving_to_db()
        {
            Subject.ToDB(null).Should().Be(DBNull.Value);
        }

        [TestCase(1, true)]
        [TestCase(0, false)]
        public void should_return_bool_when_getting_int_from_db(int input, bool expected)
        {
            var context = new ConverterContext
                          {
                              DbValue = (long)input
                          };

            Subject.FromDB(context).Should().Be(expected);
        }

        [Test]
        public void should_return_db_null_for_null_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = DBNull.Value
                          };

            Subject.FromDB(context).Should().Be(DBNull.Value);
        }

        [Test]
        public void should_throw_for_non_boolean_equivalent_number_value_when_getting_from_db()
        {
            var context = new ConverterContext
                          {
                              DbValue = 2L
                          };

            Assert.Throws<ConversionException>(() => Subject.FromDB(context));
        }
    }
}
