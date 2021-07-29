using Sonarr.Api.V3.Commands;
using RestSharp;
using NzbDrone.Core.Messaging.Commands;
using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using System.Linq;

namespace NzbDrone.Integration.Test.Client
{
    public class CommandClient : ClientBase<CommandResource>
    {
        public CommandClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public CommandResource PostAndWait(CommandResource command)
        {
            var result = Post(command);
            result.Id.Should().NotBe(0);

            for (var i = 0; i < 50; i++)
            {
                if (result.Status == CommandStatus.Completed)
                {
                    return result;
                }

                Thread.Sleep(500);
                result = Get(result.Id);
            }

            Assert.Fail("Command failed");
            return result;
        }

        public void WaitAll()
        {
            var resources = All();
            for (var i = 0; i < 50; i++)
            {
                if (!resources.Any(v => v.Status == CommandStatus.Queued || v.Status == CommandStatus.Started))
                {
                    return;
                }

                Thread.Sleep(500);
                resources = All();
            }
            
            Assert.Fail("Commands still processing");
        }
    }
}
