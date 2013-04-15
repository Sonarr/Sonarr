using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbgetProviderTests
{
    public class QueueFixture : CoreTest
    {
        [SetUp]
        public void Setup()
        {
            var fakeConfig = Mocker.GetMock<IConfigService>();
            fakeConfig.SetupGet(c => c.NzbgetHost).Returns("192.168.5.55");
            fakeConfig.SetupGet(c => c.NzbgetPort).Returns(6789);
            fakeConfig.SetupGet(c => c.NzbgetUsername).Returns("nzbget");
            fakeConfig.SetupGet(c => c.NzbgetPassword).Returns("pass");
            fakeConfig.SetupGet(c => c.NzbgetTvCategory).Returns("TV");
            fakeConfig.SetupGet(c => c.NzbgetBacklogTvPriority).Returns(PriorityType.Normal);
            fakeConfig.SetupGet(c => c.NzbgetRecentTvPriority).Returns(PriorityType.High);
        }

        private void WithFullQueue()
        {
            Mocker.GetMock<IHttpProvider>()
                    .Setup(s => s.PostCommand("192.168.5.55:6789", "nzbget", "pass", It.IsAny<String>()))
                    .Returns(ReadAllText("Files", "Nzbget", "Queue.txt"));
        }

        private void WithEmptyQueue()
        {
            Mocker.GetMock<IHttpProvider>()
                    .Setup(s => s.PostCommand("192.168.5.55:6789", "nzbget", "pass", It.IsAny<String>()))
                    .Returns(ReadAllText("Files", "Nzbget", "Queue_empty.txt"));
        }

        private void WithFailResponse()
        {
            Mocker.GetMock<IHttpProvider>()
                    .Setup(s => s.PostCommand("192.168.5.55:6789", "nzbget", "pass", It.IsAny<String>()))
                    .Returns(ReadAllText("Files", "Nzbget", "JsonError.txt"));
        }

        [Test]
        public void should_return_no_items_when_queue_is_empty()
        {
            WithEmptyQueue();

            Mocker.Resolve<NzbgetClient>()
                  .GetQueue()
                  .Should()
                  .BeEmpty();
        }

        [Test]
        public void should_return_item_when_queue_has_item()
        {
            WithFullQueue();

            Mocker.Resolve<NzbgetClient>()
                  .GetQueue()
                  .Should()
                  .HaveCount(1);
        }

        [Test]
        public void should_throw_when_error_is_returned()
        {
            WithFailResponse();

            Assert.Throws<ApplicationException>(() => Mocker.Resolve<NzbgetClient>().GetQueue());
        }
    }
}
