

using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model.Xbmc;
using NzbDrone.Core.Providers.Xbmc;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    
    public class EventClientProviderTest : CoreTest
    {
        [Test]
        public void SendNotification_true()
        {
            
            

            var header = "NzbDrone Test";
            var message = "Test Message!";
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Notification, It.IsAny<byte[]>())).Returns(true);

            
            var result = Mocker.Resolve<EventClientProvider>().SendNotification(header, message, IconType.Jpeg, "NzbDrone.jpg", address);

            
            Assert.AreEqual(true, result);
        }

        [Test]
        public void SendNotification_false()
        {
            
            

            var header = "NzbDrone Test";
            var message = "Test Message!";
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Notification, It.IsAny<byte[]>())).Returns(false);

            
            var result = Mocker.Resolve<EventClientProvider>().SendNotification(header, message, IconType.Jpeg, "NzbDrone.jpg", address);

            
            Assert.AreEqual(false, result);
        }

        [Test]
        public void SendAction_Update_true()
        {
            
            

            var path = @"C:\Test\TV\30 Rock";
            var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", path);
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Action, It.IsAny<byte[]>())).Returns(true);

            
            var result = Mocker.Resolve<EventClientProvider>().SendAction(address, ActionType.ExecBuiltin, command);

            
            Assert.AreEqual(true, result);
        }

        [Test]
        public void SendAction_Update_false()
        {
            
            

            var path = @"C:\Test\TV\30 Rock";
            var command = String.Format("ExecBuiltIn(UpdateLibrary(video,{0}))", path);
            var address = "localhost";

            var fakeUdp = Mocker.GetMock<UdpProvider>();
            fakeUdp.Setup(s => s.Send(address, UdpProvider.PacketType.Action, It.IsAny<byte[]>())).Returns(false);

            
            var result = Mocker.Resolve<EventClientProvider>().SendAction(address, ActionType.ExecBuiltin, command);

            
            Assert.AreEqual(false, result);
        }
    }
}