using System;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbgetProviderTests
{
    public class DownloadNzbFixture : CoreTest
    {
        private const string _url = "http://www.nzbdrone.com";
        private const string _title = "30.Rock.S01E01.Pilot.720p.hdtv";
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            var fakeConfig = Mocker.GetMock<IConfigService>();
            fakeConfig.SetupGet(c => c.NzbgetHost).Returns("192.168.5.55");
            fakeConfig.SetupGet(c => c.NzbgetPort).Returns(6789);
            fakeConfig.SetupGet(c => c.NzbgetUsername).Returns("nzbget");
            fakeConfig.SetupGet(c => c.NzbgetPassword).Returns("pass");
            fakeConfig.SetupGet(c => c.NzbgetTvCategory).Returns("TV");
            fakeConfig.SetupGet(c => c.NzbgetRecentTvPriority).Returns(PriorityType.High);

            _remoteEpisode = new RemoteEpisode();
            _remoteEpisode.Release = new ReleaseInfo();
            _remoteEpisode.Release.Title = _title;
            _remoteEpisode.Release.DownloadUrl = _url;

            _remoteEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                      .All()
                                                      .With(e => e.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT))
                                                      .Build()
                                                      .ToList();
        }

        private void WithFailResponse()
        {
            Mocker.GetMock<IHttpProvider>()
                    .Setup(s => s.PostCommand("192.168.5.55:6789", "nzbget", "pass", It.IsAny<String>()))
                    .Returns(ReadAllText("Files", "Nzbget", "JsonError.txt"));
        }

        [Test]
        public void should_add_item_to_queue()
        {

            var command = new JsonRequest
            {
                Method = "appendurl",
                Params = new object[] { "30.Rock.S01E01.Pilot.720p.hdtv.nzb", "TV", 50, false, "http://www.nzbdrone.com" }
            };

            Mocker.GetMock<IHttpProvider>()
                    .Setup(s => s.PostCommand("192.168.5.55:6789", "nzbget", "pass",
                        It.Is<String>(c => c.Equals(command.ToJson()))))
                    .Returns("{\"version\": \"1.1\",\"result\": true}");

            Mocker.Resolve<NzbgetClient>().DownloadNzb(_remoteEpisode);
        }

        [Test]
        public void should_throw_when_error_is_returned()
        {
            WithFailResponse();

            Assert.Throws<ApplicationException>(() => Mocker.Resolve<NzbgetClient>().DownloadNzb(_remoteEpisode));
        }
    }
}
