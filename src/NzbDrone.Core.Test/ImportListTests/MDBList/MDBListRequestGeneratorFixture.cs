using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.MDBList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.MDBList
{
    public class MDBListRequestGeneratorFixture : CoreTest<MDBListRequestGenerator>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Settings = new MDBListSettings
            {
                ApiKey = "api-key",
                ListUrl = "https://mdblist.com/lists/fuzi0n/tv-shows-indian-top-rated-ol-msl"
            };
        }

        [Test]
        public void should_build_list_items_api_request_from_list_url()
        {
            var request = Subject.GetListItems().GetAllTiers().Single().First();

            request.Url.FullUri.Should().Be("https://api.mdblist.com/lists/fuzi0n/tv-shows-indian-top-rated-ol-msl/items?apikey=api-key&limit=1000&offset=0&mediatype=show");
        }

        [Test]
        public void should_build_paged_requests()
        {
            var requests = Subject.GetListItems().GetAllTiers().Single().ToList();

            requests.Should().HaveCount(10);
            requests[1].Url.FullUri.Should().Contain("offset=1000");
            requests[9].Url.FullUri.Should().Contain("offset=9000");
        }

        [Test]
        public void should_parse_list_url()
        {
            var result = MDBListSettings.ParseListUrl("https://www.mdblist.com/lists/fuzi0n/tv-shows-indian-top-rated-ol-msl?foo=bar");

            result.Username.Should().Be("fuzi0n");
            result.ListName.Should().Be("tv-shows-indian-top-rated-ol-msl");
        }

        [Test]
        public void should_validate_required_fields()
        {
            var settings = new MDBListSettings
            {
                ApiKey = "api-key",
                ListUrl = "https://mdblist.com/lists/fuzi0n/tv-shows-indian-top-rated-ol-msl"
            };

            settings.Validate().IsValid.Should().BeTrue();

            settings.ListUrl = "https://example.com/list";

            settings.Validate().IsValid.Should().BeFalse();
        }
    }
}
