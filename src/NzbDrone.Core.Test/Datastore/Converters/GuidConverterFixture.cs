using System;
using System.Data.SQLite;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class GuidConverterFixture : CoreTest<GuidConverter>
    {
        private SQLiteParameter _param;

        [SetUp]
        public void Setup()
        {
            _param = new SQLiteParameter();
        }

        [Test]
        public void should_return_string_when_saving_guid_to_db()
        {
            var guid = Guid.NewGuid();

            Subject.SetValue(_param, guid);
            _param.Value.Should().Be(guid.ToString());
        }

        [Test]
        public void should_return_guid_when_getting_string_from_db()
        {
            var guid = Guid.NewGuid();

            Subject.Parse(guid.ToString()).Should().Be(guid);
        }

        [Test]
        public void should_return_empty_guid_for_db_null_value_when_getting_from_db()
        {
            Subject.Parse(null).Should().Be(Guid.Empty);
        }
    }
}
