using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class UpdateCheckFixture : CoreTest<UpdateCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_error_when_app_folder_is_write_protected_and_update_automatically_is_enabled()
        {
            var startupFolder = @"C:\NzbDrone".AsOsAgnostic();

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateAutomatically)
                  .Returns(true);

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(startupFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderWritable(startupFolder))
                  .Returns(false);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_when_ui_folder_is_write_protected_and_update_automatically_is_enabled()
        {
            var startupFolder = @"C:\NzbDrone".AsOsAgnostic();
            var uiFolder = @"C:\NzbDrone\UI".AsOsAgnostic();

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateAutomatically)
                  .Returns(true);

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(startupFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderWritable(startupFolder))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderWritable(uiFolder))
                  .Returns(false);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_not_return_error_when_app_folder_is_write_protected_and_external_script_enabled()
        {
            var startupFolder = @"C:\NzbDrone".AsOsAgnostic();

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateAutomatically)
                  .Returns(true);

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateMechanism)
                  .Returns(UpdateMechanism.Script);

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(startupFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(c => c.FolderWritable(It.IsAny<string>()), Times.Never());

            Subject.Check().ShouldBeOk();
        }
    }
}
