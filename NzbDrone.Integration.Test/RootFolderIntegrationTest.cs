using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using NUnit.Framework;
using NzbDrone.Api.RootFolders;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class RootFolderIntegrationTest : IntegrationTest
    {
        private Connection _connection;
        private List<object> _signalRReceived;

        [SetUp]
        public void Setup()
        {
            _signalRReceived = new List<object>();
            _connection = new Connection("http://localhost:8989/signalr/rootfolder");
            _connection.Start(new LongPollingTransport()).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Assert.Fail("SignalrConnection failed. {0}", task.Exception.GetBaseException());
                }
            });

            _connection.Received += _connection_Received;
        }

        private void _connection_Received(string obj)
        {
            _signalRReceived.Add(obj);
        }

        [Test]
        public void should_have_no_root_folder_initially()
        {
            RootFolders.All().Should().BeEmpty();

            var rootFolder = new RootFolderResource
                {
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                };

            var postResponse = RootFolders.Post(rootFolder);

            postResponse.Id.Should().NotBe(0);
            postResponse.FreeSpace.Should().NotBe(0);

            RootFolders.All().Should().OnlyContain(c => c.Id == postResponse.Id);


            RootFolders.Delete(postResponse.Id);

            RootFolders.All().Should().BeEmpty();
        }

        [Test]
        public void invalid_path_should_return_bad_request()
        {
            var rootFolder = new RootFolderResource
            {
                Path = "invalid_path"
            };

            var postResponse = RootFolders.InvalidPost(rootFolder);
            postResponse.Should().NotBeEmpty();
        }
    }
}