using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class NotificationFixture : IntegrationTest
    {
        [Test]
        public void should_not_have_any_default_notifications()
        {
            var notifications = Notifications.All();

            notifications.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_schema()
        {
            var schema = Notifications.Schema();

            schema.Should().NotBeEmpty();
            schema.Should().Contain(c => string.IsNullOrWhiteSpace(c.Name));
        }

        [Test]
        public void should_be_able_to_add_a_new_notification()
        {
            var schema = Notifications.Schema();

            var xbmc = schema.Single(s => s.Implementation.Equals("Xbmc", StringComparison.InvariantCultureIgnoreCase));

            xbmc.Name = "Test XBMC";
            xbmc.Fields.Single(f => f.Name.Equals("host")).Value = "localhost";

            var result = Notifications.Post(xbmc);
            Notifications.Delete(result.Id);
        }
    }
}
