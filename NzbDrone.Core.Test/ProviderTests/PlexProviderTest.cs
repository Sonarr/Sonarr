// ReSharper disable RedundantUsingDirective

using System;
using System.IO;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Xbmc;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class PlexProviderTest : CoreTest
    {
        [Test]
        public void GetSectionKeys_should_return_single_section_key_when_only_one_show_section()
        {
            //Setup
            
            var response = "<MediaContainer size=\"1\" mediaTagPrefix=\"/system/bundle/media/flags/\" mediaTagVersion=\"1329809559\" title1=\"Plex Library\" identifier=\"com.plexapp.plugins.library\"><Directory refreshing=\"0\" key=\"5\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory></MediaContainer>";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadStream("http://localhost:32400/library/sections", null))
                    .Returns(stream);

            //Act
            var result = Mocker.Resolve<PlexProvider>().GetSectionKeys("localhost:32400");

            //Assert
            result.Should().HaveCount(1);
            result.First().Should().Be(5);
        }

        [Test]
        public void GetSectionKeys_should_return_single_section_key_when_only_one_show_section_with_other_sections()
        {
            //Setup

            var response = "<MediaContainer size=\"1\" mediaTagPrefix=\"/system/bundle/media/flags/\" mediaTagVersion=\"1329809559\" title1=\"Plex Library\" identifier=\"com.plexapp.plugins.library\"><Directory refreshing=\"0\" key=\"5\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory><Directory refreshing=\"0\" key=\"7\" type=\"movie\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory></MediaContainer>";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadStream("http://localhost:32400/library/sections", null))
                    .Returns(stream);

            //Act
            var result = Mocker.Resolve<PlexProvider>().GetSectionKeys("localhost:32400");

            //Assert
            result.Should().HaveCount(1);
            result.First().Should().Be(5);
        }

        [Test]
        public void GetSectionKeys_should_return_multiple_section_keys_when_there_are_multiple_show_sections()
        {
            //Setup

            var response = "<MediaContainer size=\"1\" mediaTagPrefix=\"/system/bundle/media/flags/\" mediaTagVersion=\"1329809559\" title1=\"Plex Library\" identifier=\"com.plexapp.plugins.library\"><Directory refreshing=\"0\" key=\"5\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory><Directory refreshing=\"0\" key=\"6\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory></MediaContainer>";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadStream("http://localhost:32400/library/sections", null))
                    .Returns(stream);

            //Act
            var result = Mocker.Resolve<PlexProvider>().GetSectionKeys("localhost:32400");

            //Assert
            result.Should().HaveCount(2);
            result.First().Should().Be(5);
            result.Last().Should().Be(6);
        }

        [Test]
        public void UpdateSection_should_update_section()
        {
            //Setup

            var response = "";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://localhost:32400/library/sections/5/refresh"))
                    .Returns(response);

            //Act
            Mocker.Resolve<PlexProvider>().UpdateSection("localhost:32400", 5);

            //Assert
            
        }
    }
}