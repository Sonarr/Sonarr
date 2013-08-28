using NUnit.Framework;
using NzbDrone.Api.Commands;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class CommandIntegrationTest : IntegrationTest
    {
        [Test]
        public void should_be_able_to_run_rss_sync()
        {
            Commands.Post(new CommandResource {Command = "rsssync"});
        }
    }
}