using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.RootFolders;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class RootFolderIntegrationTest : IntegrationTest
    {
        [Test]
        public void should_have_no_root_folder_initially()
        {
            RootFolders.All().Should().BeEmpty();
        }

        [Test]
        public void should_add_and_delete_root_folders()
        {
            ConnectSignalR();

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
            postResponse.Should().NotBeEmpty();
        }
    }
}