using System;
using System.Data.SQLite;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class UtcConverterFixture : CoreTest<DapperUtcConverter>
    {
        private SQLiteParameter _param;

        [SetUp]
        public void Setup()
        {
            _param = new SQLiteParameter();
        }

        [Test]
        public void should_return_date_time_when_saving_date_time_to_db()
        {
            var dateTime = DateTime.Now;

            Subject.SetValue(_param, dateTime);
            _param.Value.Should().Be(dateTime.ToUniversalTime());
        }

        [Test]
        public void should_return_time_span_when_getting_time_span_from_db()
        {
            var dateTime = DateTime.Now.ToUniversalTime();

            Subject.Parse(dateTime).Should().Be(dateTime);
        }
    }
}
