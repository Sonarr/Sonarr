using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class NamingConfigTests : IntegrationTest
    {

        [Test]
        public void should_be_able_to_get()
        {
            NamingConfig.GetSingle().Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_get_by_id()
        {
            var config = NamingConfig.GetSingle();
            NamingConfig.Get(config.Id).Should().NotBeNull();
            NamingConfig.Get(config.Id).Id.Should().Be(config.Id);
        }

        [Test]
        public void should_be_able_to_update()
        {
            var config = NamingConfig.GetSingle();
            config.RenameEpisodes = false;

            NamingConfig.Put(config).RenameEpisodes.Should().BeFalse();
        }
    }
}