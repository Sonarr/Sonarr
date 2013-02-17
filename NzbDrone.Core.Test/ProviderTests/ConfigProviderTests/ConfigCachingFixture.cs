using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test.ProviderTests.ConfigProviderTests
{
    [TestFixture]
    public class ConfigCachingFixture : CoreTest
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDatabase>().Setup(c => c.Fetch<Config>())
                    .Returns(new List<Config> { new Config { Key = "Key1", Value = "Value1" } });

        }

        [Test]
        public void getting_value_more_than_once_should_hit_db_once()
        {
            Mocker.Resolve<ConfigProvider>().GetValue("Key1", null).Should().Be("Value1");
            Mocker.Resolve<ConfigProvider>().GetValue("Key1", null).Should().Be("Value1");
            Mocker.Resolve<ConfigProvider>().GetValue("Key1", null).Should().Be("Value1");

            Mocker.GetMock<IDatabase>().Verify(c => c.Fetch<Config>(), Times.Once());
        }

    }
}
