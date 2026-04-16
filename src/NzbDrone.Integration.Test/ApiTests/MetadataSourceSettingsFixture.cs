using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Integration.Test.Client;
using RestSharp;
using Sonarr.Http.REST;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class MetadataSourceSettingsFixture : IntegrationTest
    {
        private ClientBase<TestMetadataSourceSettingsResource> _metadataSourceSettings;

        [SetUp]
        public void SetUpTest()
        {
            var v5Client = new RestClient(RootUrl + "api/v5/");
            _metadataSourceSettings = new ClientBase<TestMetadataSourceSettingsResource>(v5Client, ApiKey, "settings/metadatasource");
        }

        [Test]
        public void should_be_able_to_get_metadata_source_settings()
        {
            var settings = _metadataSourceSettings.GetSingle();

            settings.Should().NotBeNull();
            settings.MetadataSource.Should().Be(MetadataSourceType.Tvdb);
        }

        [Test]
        public void should_be_able_to_update_metadata_source_settings()
        {
            var settings = _metadataSourceSettings.GetSingle();
            settings.MetadataSource = MetadataSourceType.Tmdb;
            settings.TmdbApiKey = "tmdb-test-key";

            var result = _metadataSourceSettings.Put(settings);

            result.MetadataSource.Should().Be(MetadataSourceType.Tmdb);
            result.TmdbApiKey.Should().Be("tmdb-test-key");
        }

        [Test]
        public void should_require_tmdb_api_key_when_tmdb_is_selected()
        {
            var settings = _metadataSourceSettings.GetSingle();
            settings.MetadataSource = MetadataSourceType.Tmdb;
            settings.TmdbApiKey = string.Empty;

            var errors = _metadataSourceSettings.InvalidPut(settings);

            errors.Should().NotBeNull();
        }

        public class TestMetadataSourceSettingsResource : RestResource
        {
            public MetadataSourceType MetadataSource { get; set; }
            public string TmdbApiKey { get; set; }
        }
    }
}
