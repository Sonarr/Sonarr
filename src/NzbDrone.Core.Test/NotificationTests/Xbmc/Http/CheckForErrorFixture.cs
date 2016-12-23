using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Http
{
    [TestFixture]
    public class CheckForErrorFixture : CoreTest<HttpApiProvider>
    {
        [Test]
        public void should_be_true_when_the_response_contains_an_error()
        {
            const string response = "html><li>Error:Unknown command</html>";

            Subject.CheckForError(response).Should().BeTrue();
        }

        [Test]
        public void JsonError_true_empty_response()
        {
            var response = string.Empty;

            Subject.CheckForError(response).Should().BeTrue();
        }

        [Test]
        public void JsonError_false()
        {
            const string response = "html><li>Filename:[Nothing Playing]</html>";

            Subject.CheckForError(response).Should().BeFalse();
        }
    }
}
