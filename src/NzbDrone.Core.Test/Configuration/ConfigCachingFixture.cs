using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Configuration
{
    [TestFixture]
    public class ConfigCachingFixture : CoreTest<ConfigService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigRepository>().Setup(c => c.All())
                    .Returns(new List<Config> { new Config { Key = "key1", Value = "Value1" } });
        }

        [Test]
        public void getting_value_more_than_once_should_hit_db_once()
        {
            Subject.GetValue("Key1", null).Should().Be("Value1");
            Subject.GetValue("Key1", null).Should().Be("Value1");
            Subject.GetValue("Key1", null).Should().Be("Value1");

            Mocker.GetMock<IConfigRepository>().Verify(c => c.All(), Times.Once());
        }
    }
}
