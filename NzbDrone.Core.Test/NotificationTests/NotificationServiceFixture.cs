using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Email;
using NzbDrone.Core.Notifications.Growl;
using NzbDrone.Core.Notifications.Plex;
using NzbDrone.Core.Notifications.Prowl;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests
{
    public class NotificationServiceFixture : DbTest<NotificationService, NotificationDefinition>
    {
        private List<INotification> _notifications;

        [SetUp]
        public void Setup()
        {
            _notifications = new List<INotification>();

            _notifications.Add(new Xbmc(null, null));
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

            Mocker.GetMock<IContainer>().Setup(s => s.Resolve(typeof (Xbmc)))
                  .Returns(new Xbmc(null, null));

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
            notifications.Should().NotContain(c => c.Name == null);
            notifications.Select(c => c.Name).Should().OnlyHaveUniqueItems();
            notifications.Select(c => c.Instance).Should().OnlyHaveUniqueItems();
            notifications.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        }
    }
}