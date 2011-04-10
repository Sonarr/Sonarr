using AutoMoq;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers.Core;
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
            var config = new Config {Key = key, Value = value};

            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(r => r.Single<Config>(key))
                .Returns(config);

            //Act
            mocker.Resolve<ConfigProvider>().SetValue(key, value);

            //Assert
            mocker.GetMock<IRepository>().Verify(c => c.Update(config));
            mocker.GetMock<IRepository>().Verify(c => c.Add(It.IsAny<Config>()), Times.Never());
        }

        [Test]
        public void Add_new_value()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            //Arrange
            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(r => r.Single<Config>(It.IsAny<string>()))
                .Returns<Config>(null)
                .Verifiable();

            //Act
            mocker.Resolve<ConfigProvider>().SetValue(key, value);

            //Assert
            mocker.GetMock<IRepository>().Verify();
            mocker.GetMock<IRepository>().Verify(r => r.Update(It.IsAny<Config>()), Times.Never());
            mocker.GetMock<IRepository>().Verify(r => r.Add(It.Is<Config>(c => c.Key == key && c.Value == value)),
                                                 Times.Once());
        }
    }
}