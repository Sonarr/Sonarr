using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Providers;
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

            var config = new Mock<IConfigProvider>();
            config.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            config.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            config.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            config.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            config.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);
            config.Setup(c => c.GetValue("SabPriority", String.Empty, false)).Returns(priority);
            config.Setup(c => c.GetValue("SabCategory", String.Empty, false)).Returns(category);

            var http = new Mock<IHttpProvider>();
            http.Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass")).Returns("ok");

            var target = new SabProvider(config.Object, http.Object);

            //Act
            bool result = target.AddByUrl("http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an Nzb");

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

            var config = new Mock<IConfigProvider>();
            config.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            config.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            config.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            config.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            config.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);
            config.Setup(c => c.GetValue("SabPriority", String.Empty, false)).Returns(priority);
            config.Setup(c => c.GetValue("SabCategory", String.Empty, false)).Returns(category);

            var http = new Mock<IHttpProvider>();
            http.Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&cat=tv&nzbname=This+is+an+Nzb&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass")).Returns("error");

            var target = new SabProvider(config.Object, http.Object);

            //Act
            bool result = target.AddByUrl("http://www.nzbclub.com/nzb_download.aspx?mid=1950232", "This is an Nzb");

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

            var config = new Mock<IConfigProvider>();
            config.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            config.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            config.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            config.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            config.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            var http = new Mock<IHttpProvider>();
            http.Setup(
                s =>
                s.DownloadString(
                    "http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(new StreamReader(@".\Files\Queue.xml").ReadToEnd());

            var target = new SabProvider(config.Object, http.Object);

            //Act
            bool result = target.IsInQueue("Ubuntu Test");

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

            var config = new Mock<IConfigProvider>();
            config.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            config.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            config.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            config.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            config.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            var http = new Mock<IHttpProvider>();
            http.Setup(
                s =>
                s.DownloadString(
                    "http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(new StreamReader(@".\Files\QueueEmpty.xml").ReadToEnd());

            var target = new SabProvider(config.Object, http.Object);

            //Act
            bool result = target.IsInQueue(String.Empty);

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

            var config = new Mock<IConfigProvider>();
            config.Setup(c => c.GetValue("SabHost", String.Empty, false)).Returns(sabHost);
            config.Setup(c => c.GetValue("SabPort", String.Empty, false)).Returns(sabPort);
            config.Setup(c => c.GetValue("SabApiKey", String.Empty, false)).Returns(apikey);
            config.Setup(c => c.GetValue("SabUsername", String.Empty, false)).Returns(username);
            config.Setup(c => c.GetValue("SabPassword", String.Empty, false)).Returns(password);

            var http = new Mock<IHttpProvider>();
            http.Setup(
                s =>
                s.DownloadString(
                    "http://192.168.5.55:2222/api?mode=queue&output=xml&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                .Returns(new StreamReader(@".\Files\QueueError.xml").ReadToEnd());

            var target = new SabProvider(config.Object, http.Object);

            //Act
            bool result = target.IsInQueue(String.Empty);

            //Assert
            Assert.AreEqual(false, result);
        }
    }
}
