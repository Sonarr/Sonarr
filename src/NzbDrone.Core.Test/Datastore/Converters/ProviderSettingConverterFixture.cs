using System.Data.SQLite;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [Ignore("To reinstate once dapper changes worked out")]
    [TestFixture]
    public class ProviderSettingConverterFixture : CoreTest<ProviderSettingConverter>
    {
        private SQLiteParameter _param;

        [SetUp]
        public void Setup()
        {
            _param = new SQLiteParameter();
        }

        [Test]
        public void should_return_null_config_if_config_is_null()
        {
            Subject.Parse(null).Should().Be(NullConfig.Instance);
        }

        [TestCase(null)]
        [TestCase("")]
        public void should_return_null_config_if_config_is_empty(object dbValue)
        {
            Subject.Parse(dbValue).Should().Be(NullConfig.Instance);
        }
    }
}
