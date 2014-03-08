using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class UpdateCheckFixture : CoreTest<UpdateCheck>
    {
        [Test]
        public void should_return_error_when_app_folder_is_write_protected()
        {
            WindowsOnly();

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(@"C:\NzbDrone");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.WriteAllText(It.IsAny<String>(), It.IsAny<String>()))
                  .Throws<Exception>();

            Subject.Check().ShouldBeError();
        }
    }
}
