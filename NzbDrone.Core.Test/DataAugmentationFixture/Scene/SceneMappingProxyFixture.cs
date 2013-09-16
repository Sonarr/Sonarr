using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
using NzbDrone.Common;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DataAugmentationFixture.Scene
{
    [TestFixture]

    public class SceneMappingProxyFixture : CoreTest<SceneMappingProxy>
    {
        private const string SCENE_MAPPING_URL = "http://services.nzbdrone.com/v1/SceneMapping";

        [Test]
        public void fetch_should_return_list_of_mappings()
        {
            Mocker.GetMock<IHttpProvider>()
             .Setup(s => s.DownloadString(SCENE_MAPPING_URL))
             .Returns(ReadAllText("Files", "SceneMappings.json"));

            var mappings = Subject.Fetch();

            mappings.Should().NotBeEmpty();

            mappings.Should().NotContain(c => string.IsNullOrWhiteSpace(c.SearchTerm));
            mappings.Should().NotContain(c => string.IsNullOrWhiteSpace(c.ParseTerm));
            mappings.Should().NotContain(c => c.TvdbId == 0);
        }

        [Test]
        public void should_throw_on_server_error()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(SCENE_MAPPING_URL))
                  .Throws(new WebException());
            Assert.Throws<WebException>(() => Subject.Fetch());
        }

        [Test]
        public void should_throw_on_bad_json()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(SCENE_MAPPING_URL))
                  .Returns("bad json");
            Assert.Throws<JsonReaderException>(() => Subject.Fetch());
        }
    }
}
