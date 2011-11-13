// ReSharper disable InconsistentNaming
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class WebClientTests : TestBase
    {
        [Test]
        public void DownloadString_should_be_able_to_download_jquery()
        {
            var jquery = new WebClientProvider().DownloadString("http://ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js");

            jquery.Should().NotBeBlank();
            jquery.Should().Contain("function(a,b)");
        }

        [TestCase("")]
        [TestCase("http://")]
        [TestCase(null)]
        [ExpectedException]
        public void DownloadString_should_throw_on_error(string url)
        {
            var jquery = new WebClientProvider().DownloadString(url);
        }
    }
}
