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
            String key = "SabnzbdInfo";
            String value = "192.168.5.55:2222";

            var repo = new Mock<IRepository>();
            var config = new Mock<IConfigController>();
            config.Setup(c => c.SetValue("SabnzbdInfo", "192.168.5.55:2222"));

            //var config = new Config() { Key = key, Value = value };
            var target = new SabController(config.Object, new Mock<ILog>().Object);

            //Act
            bool result = target.AddByUrl("http://www.nzbclub.com/nzb_download.aspx?mid=1950232");

            //Assert
            Assert.AreEqual(true, result);
        }
    }
}
