using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class DbConfigControllerTest
    {
        [Test]
        public void Overwrite_existing_value()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            //Arrange
            var repo = new Mock<IRepository>();
            var config = new Config { Key = key, Value = value };
            repo.Setup(r => r.Single<Config>(key)).Returns(config);
            var target = new ConfigProvider(repo.Object);

            //Act
            target.SetValue(key, value);

            //Assert
            repo.Verify(c => c.Update(config));
            repo.Verify(c => c.Add(It.IsAny<Config>()), Times.Never());
        }

        [Test]
        public void Add_new_value()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            //Arrange
            var repo = new Mock<IRepository>();
            repo.Setup(r => r.Single<Config>(It.IsAny<string>())).Returns<Config>(null).Verifiable();
            var target = new ConfigProvider(repo.Object);

            //Act
            target.SetValue(key, value);

            //Assert
            repo.Verify();
            repo.Verify(r => r.Update(It.IsAny<Config>()), Times.Never());
            repo.Verify(r => r.Add(It.Is<Config>(c => c.Key == key && c.Value == value)), Times.Once());
        }
    }
}
