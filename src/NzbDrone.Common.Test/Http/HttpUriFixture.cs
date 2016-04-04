using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http
{
    public class HttpUriFixture : TestBase
    {
        private HttpUri GivenHttpUri(string basePath)
        {
            return new HttpUri("http", "localhost", 8989, basePath, null, null);
        }

        [TestCase("", "", "")]
        [TestCase("base", "", "/base")]
        [TestCase("/base", "", "/base")]
        [TestCase("", "relative", "/relative")]
        [TestCase("", "/relative", "/relative")]
        [TestCase("base", "relative", "/base/relative")]
        [TestCase("base", "/relative", "/base/relative")]
        [TestCase("/base", "relative", "/base/relative")]
        [TestCase("/base", "/relative", "/base/relative")]
        public void should_combine_base_path_and_relative_path(string basePath, string relativePath, string expected)
        {
            GivenHttpUri(basePath).CombinePath(relativePath).Path.Should().Be(expected);
        }
    }
}
