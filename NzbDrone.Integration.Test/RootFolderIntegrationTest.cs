using System.IO;
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

            var rootFolder = new RootFolderResource
                {
                    Path = Directory.GetCurrentDirectory()
                };

            var postResponse = RootFolders.Post(rootFolder);

            postResponse.Id.Should().NotBe(0);
            postResponse.FreeSpace.Should().NotBe(0);

            RootFolders.All().Should().OnlyContain(c => c.Id == postResponse.Id);


            RootFolders.Delete(postResponse.Id);

            RootFolders.All().Should().BeEmpty();


        }

    }
}