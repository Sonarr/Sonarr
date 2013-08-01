using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Composition;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Email;
using NzbDrone.Core.Notifications.Growl;
using NzbDrone.Core.Notifications.Plex;
using NzbDrone.Core.Notifications.Prowl;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests
{
    public class NotificationServiceFixture : DbTest<NotificationService, NotificationDefinition>
    {
        private List<INotification> _notifications;

        [SetUp]
        public void Setup()
        {
            _notifications = new List<INotification>();

            _notifications.Add(new Notifications.Xbmc.Xbmc(null, null));
            _notifications.Add(new PlexClient(null));
            _notifications.Add(new PlexServer(null));
            _notifications.Add(new Email(null));
            _notifications.Add(new Growl(null));
            _notifications.Add(new Prowl(null));

            Mocker.SetConstant<IEnumerable<INotification>>(_notifications);
        }

        [Test]
        public void getting_list_of_indexers_should_be_empty_by_default()
        {
            Mocker.SetConstant<INotificationRepository>(Mocker.Resolve<NotificationRepository>());

            var notifications = Subject.All().ToList();
            notifications.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_schema_for_all_notifications()
        {
            Mocker.SetConstant<INotificationRepository>(Mocker.Resolve<NotificationRepository>());

            Mocker.GetMock<IContainer>().Setup(s => s.Resolve(typeof(Notifications.Xbmc.Xbmc)))
                  .Returns(new Notifications.Xbmc.Xbmc(null, null));

            Mocker.GetMock<IContainer>().Setup(s => s.Resolve(typeof(PlexClient)))
                  .Returns(new PlexClient(null));

            Mocker.GetMock<IContainer>().Setup(s => s.Resolve(typeof(PlexServer)))
                  .Returns(new PlexServer(null));

            Mocker.GetMock<IContainer>().Setup(s => s.Resolve(typeof(Email)))
                  .Returns(new Email(null));

            Mocker.GetMock<IContainer>().Setup(s => s.Resolve(typeof(Growl)))
                  .Returns(new Growl(null));

            Mocker.GetMock<IContainer>().Setup(s => s.Resolve(typeof(Prowl)))
                  .Returns(new Prowl(null));

            var notifications = Subject.Schema().ToList();
            notifications.Should().NotBeEmpty();
            notifications.Should().NotContain(c => c.Settings == null);
            notifications.Should().NotContain(c => c.Instance == null);
            notifications.Should().NotContain(c => c.ImplementationName == null);
            notifications.Select(c => c.ImplementationName).Should().OnlyHaveUniqueItems();
            notifications.Select(c => c.Instance).Should().OnlyHaveUniqueItems();
            notifications.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        }

        [Test]
        [Explicit]
        public void should_try_other_notifiers_when_one_fails()
        {
            var notifications = Builder<NotificationDefinition>.CreateListOfSize(2)
                .All()
                .With(n => n.OnGrab = true)
                .With(n => n.OnDownload = true)
                .TheFirst(1)
                .With(n => n.Implementation = "Xbmc")
                .TheLast(1)
                .With(n => n.Implementation = "Email")
                .Build()
                .ToList();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesType = SeriesTypes.Standard)
                .Build();

            var parsedEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew()
                .With(p => p.EpisodeNumbers = new int[] {1})
                .Build();

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(e => e.Series = series)
                .With(e => e.ParsedEpisodeInfo = parsedEpisodeInfo)
                .With(e => e.Episodes = Builder<Episode>.CreateListOfSize(1)
                    .Build().ToList())
                .Build();

            Mocker.GetMock<INotificationRepository>()
                .Setup(s => s.All())
                .Returns(notifications);

            //Todo: How can we test this, right now without an empty constructor it won't work
            Mocker.GetMock<Notifications.Xbmc.Xbmc>()
                .Setup(s => s.OnDownload(It.IsAny<string>(), series))
                .Throws(new SocketException());

            Subject.Handle(new EpisodeDownloadedEvent(localEpisode));

            Mocker.GetMock<Notifications.Xbmc.Xbmc>()
                .Verify(v => v.OnDownload(It.IsAny<string>(), series), Times.Once());

            Mocker.GetMock<Email>()
                .Verify(v => v.OnDownload(It.IsAny<string>(), series), Times.Once());
        }
    }
}