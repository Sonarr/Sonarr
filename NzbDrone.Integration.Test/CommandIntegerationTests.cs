using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Commands;
using NzbDrone.Common.Messaging.Tracking;
using NzbDrone.Common.Serializer;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class CommandIntegrationTest : IntegrationTest
    {
        [Test]
        public void should_be_able_to_run_rss_sync()
        {
            var request = new RestRequest("command")
            {
                RequestFormat = DataFormat.Json,
                Method = Method.POST
            };

            request.AddBody(new CommandResource {Command = "rsssync"});

            var restClient = new RestClient("http://localhost:8989/api");
            var response = restClient.Execute(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            response.ErrorMessage.Should().BeBlank();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var trackedCommand = Json.Deserialize<TrackedCommand>(response.Content);
            trackedCommand.Id.Should().NotBeNullOrEmpty();
        }
    }
}