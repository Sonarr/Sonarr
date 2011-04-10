using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AutoMoq;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SabControllerTest
    {
        [Test]
        public void AddByUrlSuccess()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";
            string priority = "0";
            string category = "tv";


            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false))
                .Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false))
                .Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false))
                .Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false))
                .Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false))
                .Returns(password);
            fakeConfig.Setup(c => c.GetValue("SabTvPriority", String.Empty, false))
                .Returns(priority);
            fakeConfig.Setup(c => c.GetValue("SabTvCategory", String.Empty, true))
                .Returns(category);

            mocker.GetMock<HttpProvider>()
            .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
            .Returns("ok");

            //Act
            bool result = mocker.Resolve<SabProvider>().AddByUrl("http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an Nzb");

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void AddByUrlError()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";
            string priority = "0";
            string category = "tv";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);
            fakeConfig.Setup(c => c.GetValue("SabTvPriority", String.Empty, false)).Returns(priority);
            fakeConfig.Setup(c => c.GetValue("SabTvCategory", String.Empty, true)).Returns(category);

            mocker.GetMock<HttpProvider>()
           .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
           .Returns("error");

            //Act
            bool result = mocker.Resolve<SabProvider>().AddByUrl("http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an Nzb");

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void IsInQueue_True()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(new StreamReader(@".\Files\Queue.xml").ReadToEnd());

            //Act
            bool result = mocker.Resolve<SabProvider>().IsInQueue("Ubuntu Test");

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void IsInQueue_False_Empty()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            mocker.GetMock<HttpProvider>()
           .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
               .Returns(new StreamReader(@".\Files\QueueEmpty.xml").ReadToEnd());

            //Act
            bool result = mocker.Resolve<SabProvider>().IsInQueue(String.Empty);

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        public void IsInQueue_False_Error()
        {
            //Setup
            string sabHost = "192.168.5.55";
            string sabPort = "2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";

            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            fakeConfig.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            fakeConfig.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            fakeConfig.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            fakeConfig.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            mocker.GetMock<HttpProvider>()
          .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
              .Returns(new StreamReader(@".\Files\QueueError.xml").ReadToEnd());


            //Act
            bool result = mocker.Resolve<SabProvider>().IsInQueue(String.Empty);

            //Assert
            Assert.AreEqual(false, result);
        }
    }
}
