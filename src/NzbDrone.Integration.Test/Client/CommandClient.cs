using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Messaging.Commands;
using RestSharp;
using Sonarr.Http.REST;

namespace NzbDrone.Integration.Test.Client
{
    public class SimpleCommandResource : RestResource
    {
        public string Name { get; set; }
        public string CommandName { get; set; }
        public string Message { get; set; }
        public CommandPriority Priority { get; set; }
        public CommandStatus Status { get; set; }
        public DateTime Queued { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Exception { get; set; }
        public CommandTrigger Trigger { get; set; }

        [JsonIgnore]
        public Command Body { get; set; }
        [JsonProperty("body")]
        public Command BodyReadOnly
        {
            get { return Body; }
        }
    }

    public class CommandClient : ClientBase<SimpleCommandResource>
    {
        public CommandClient(IRestClient restClient, string apiKey)
        : base(restClient, apiKey, "command")
        {
        }

        public SimpleCommandResource PostAndWait<T>(T command)
            where T : Command, new()
        {
            var request = BuildRequest();
            request.AddJsonBody(command);
            var result = Post<SimpleCommandResource>(request);
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
