using NUnit.Framework;
using NzbDrone.Core.Notifications.Growl;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests
{
    [Explicit]
    [TestFixture]
    public class GrowlProviderTest : CoreTest
    {
        [Test]
        public void Register_should_add_new_application_to_local_growl_instance()
        {
            
            
            
            
            Mocker.Resolve<GrowlProvider>().Register("localhost", 23053, "");

            
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void TestNotification_should_send_a_message_to_local_growl_instance()
        {
            
            

            
            Mocker.Resolve<GrowlProvider>().TestNotification("localhost", 23053, "");

            
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void OnGrab_should_send_a_message_to_local_growl_instance()
        {
            
            

            
            Mocker.Resolve<GrowlProvider>().SendNotification("Episode Grabbed", "Series Title - 1x05 - Episode Title", "GRAB", "localhost", 23053, "");

            
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void OnDownload_should_send_a_message_to_local_growl_instance()
        {
            
            

            
            Mocker.Resolve<GrowlProvider>().SendNotification("Episode Downloaded", "Series Title - 1x05 - Episode Title", "DOWNLOAD", "localhost", 23053, "");

            
            Mocker.VerifyAllMocks();
        }
    }
}