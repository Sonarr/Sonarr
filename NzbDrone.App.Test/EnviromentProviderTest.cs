using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Providers;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class EnviromentProviderTest
    {

        [Test]
        public void Is_user_interactive_should_be_false()
        {
            var enviromentController = new EnviromentProvider();

            //Act
            enviromentController.IsUserInteractive.Should().BeTrue();
        }

        [Test]
        public void Log_path_should_not_be_empty()
        {
            var enviromentController = new EnviromentProvider();

            //Act
            enviromentController.LogPath.Should().NotBeBlank();
        }
    }
}
