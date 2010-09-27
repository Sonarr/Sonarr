using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using log4net;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Controllers;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class SabControllerTest
    {
        [Test]
        public void AddByUrl()
        {
            //Setup
            string sabnzbdInfo = "192.168.5.55:2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";
            string priority = "0";

            var config = new Mock<IConfigController>();
            config.Setup(c => c.GetValue("SabnzbdInfo", String.Empty, false)).Returns(sabnzbdInfo);
            config.Setup(c => c.GetValue("ApiKey", String.Empty, false)).Returns(apikey);
            config.Setup(c => c.GetValue("Username", String.Empty, false)).Returns(username);
            config.Setup(c => c.GetValue("Password", String.Empty, false)).Returns(password);
            config.Setup(c => c.GetValue("Priority", String.Empty, false)).Returns(priority);

            var target = new SabController(config.Object, new Mock<ILog>().Object);

            ItemInfo nzb = new ItemInfo();
            nzb.Link = new Uri("http://www.nzbclub.com/nzb_download.aspx?mid=1950232");
            nzb.Title = "This is an Nzb";

            //Act
            bool result = target.AddByUrl(nzb);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void IsInQueue()
        {
            //Setup
            string sabnzbdInfo = "192.168.5.55:2222";
            string apikey = "5c770e3197e4fe763423ee7c392c25d1";
            string username = "admin";
            string password = "pass";
            string priority = "0";

            var config = new Mock<IConfigController>();
            config.Setup(c => c.GetValue("SabnzbdInfo", String.Empty, false)).Returns(sabnzbdInfo);
            config.Setup(c => c.GetValue("ApiKey", String.Empty, false)).Returns(apikey);
            config.Setup(c => c.GetValue("Username", String.Empty, false)).Returns(username);
            config.Setup(c => c.GetValue("Password", String.Empty, false)).Returns(password);
            config.Setup(c => c.GetValue("Priority", String.Empty, false)).Returns(priority);

            var target = new SabController(config.Object, new Mock<ILog>().Object);

            Episode episode = new Episode();
            FeedItem item = new FeedItem();
            item.TitleFix = "This is my fixed title";
            episode.FeedItem = item;

            //Act
            bool result = target.IsInQueue(episode);

            //Assert
            Assert.AreEqual(false, result);
        }
    }
}
