using System.Data.SQLite;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class QualityIntConverterFixture : CoreTest<DapperQualityIntConverter>
    {
        private SQLiteParameter _param;

        [SetUp]
        public void Setup()
        {
            _param = new SQLiteParameter();
        }

        [Test]
        public void should_return_int_when_saving_quality_to_db()
        {
            var quality = Quality.Bluray1080p;

            Subject.SetValue(_param, quality);
            _param.Value.Should().Be(quality.Id);
        }

        [Test]
        public void should_return_0_when_saving_db_null_to_db()
        {
            Subject.SetValue(_param, null);
            _param.Value.Should().Be(0);
        }

        [Test]
        public void should_return_quality_when_getting_string_from_db()
        {
            var quality = Quality.Bluray1080p;

            Subject.Parse(quality.Id).Should().Be(quality);
        }

        [Test]
        public void should_return_unknown_for_null_value_when_getting_from_db()
        {
            Subject.Parse(null).Should().Be(Quality.Unknown);
        }
    }
}
