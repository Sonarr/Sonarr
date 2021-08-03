using System;
using FluentAssertions;
using NUnit.Framework;
using Sonarr.Api.V3.RootFolders;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class RootFolderFixture : IntegrationTest
    {
        [Test]
        public void should_have_no_root_folder_initially()
        {
            RootFolders.All().Should().BeEmpty();
        }

        [Test]
        [Ignore("SignalR on CI seems unstable")]
        public void should_add_and_delete_root_folders()
        {
            ConnectSignalR().Wait();

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

            SignalRMessages.Should().Contain(c => c.Name == "rootfolder");
        }

        [Test]
        public void invalid_path_should_return_bad_request()
        {
            var rootFolder = new RootFolderResource
            {
                Path = "invalid_path"
            };

            var postResponse = RootFolders.InvalidPost(rootFolder);
            postResponse.Should().NotBeNull();
        }
    }
}
