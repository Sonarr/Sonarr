using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Email;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.ThingiProvider
{

    public class ProviderRepositoryFixture : DbTest<NotificationProviderRepository, NotificationProviderModel>
    {
        [Test]
        public void should_read_write_download_provider()
        {
            var model = Builder<NotificationProviderModel>.CreateNew().BuildNew();
            var emailSettings = Builder<EmailSettings>.CreateNew().Build();
            model.Settings = emailSettings;
            Subject.Insert(model);

            var storedProvider = Subject.Single();
            
            storedProvider.Settings.Should().BeOfType<EmailSettings>();

            var storedSetting = (EmailSettings) storedProvider.Settings;
        
            storedSetting.ShouldHave().AllProperties().EqualTo(emailSettings);
        }
    }
}