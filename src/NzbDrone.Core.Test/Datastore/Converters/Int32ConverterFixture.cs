using System;
using FluentAssertions;
using Marr.Data.Converters;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class Int32ConverterFixture : CoreTest<Int32Converter>
    {
        [Test]
        public void should_return_int_when_saving_int_to_db()
        {
            var i = 5;

            Subject.ToDB(i).Should().Be(5);
        }

        [Test]
        public void should_return_int_when_getting_int_from_db()
        {
            var i = 5;

            var context = new ConverterContext
                          {
                              DbValue = i
                          };

            Subject.FromDB(context).Should().Be(i);
        }

        [Test]
        public void should_return_int_when_getting_string_from_db()
        {
            var i = 5;

            var context = new ConverterContext
                          {
                              DbValue = i.ToString()
                          };

            Subject.FromDB(context).Should().Be(i);
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
