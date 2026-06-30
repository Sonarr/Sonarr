using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.HttpTests
{
    [TestFixture]
    public class StaticResourceControllerFixture : IntegrationTest
    {
        [TestCase("Content/..%5cindex.html")]
        [TestCase("Content/..%2findex.html")]
        public async Task should_get_not_found_for_invalid_path(string path)
        {
            using var client = new HttpClient();
            var url = $"{RootUrl}/{path}";
            var response = await client.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestCase("Content/styles.css")]
        public async Task should_get_ok_response_for_valid_path(string path)
        {
            using var client = new HttpClient();
            var url = $"{RootUrl}/{path}";
            var response = await client.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
