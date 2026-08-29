using System;
using System.IO;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Options;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Authentication
{
    [TestFixture]
    public class InitialUserServiceFixture : CoreTest<InitialUserService>
    {
        private string _configPath;

        [SetUp]
        public void Setup()
        {
            _configPath = Path.Combine(TempFolder, "config.xml");

            Mocker.GetMock<IAppFolderInfo>()
                .SetupGet(c => c.AppDataFolder)
                .Returns(TempFolder);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(_configPath))
                .Returns(false);

            Mocker.GetMock<IConfigFileProvider>()
                .SetupGet(c => c.AuthenticationMethod)
                .Returns(AuthenticationType.Forms);

            Mocker.GetMock<IOptions<AuthOptions>>()
                .Setup(c => c.Value)
                .Returns(new AuthOptions());

            Mocker.GetMock<IUserService>()
                .Setup(c => c.FindUser())
                .Returns((User)null);
        }

        [Test]
        public void should_seed_default_user_with_generated_password()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IUserService>()
                .Verify(c => c.Add("sonarr", It.Is<string>(p => p.Length == 20)), Times.Once());
        }

        [Test]
        public void should_seed_from_auth_options()
        {
            Mocker.GetMock<IOptions<AuthOptions>>()
                .Setup(c => c.Value)
                .Returns(new AuthOptions { InitialUsername = "someone", InitialPassword = "somepassword" });

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IUserService>()
                .Verify(c => c.Add("someone", "somepassword"), Times.Once());
        }

        [Test]
        public void should_not_seed_when_a_user_already_exists()
        {
            Mocker.GetMock<IUserService>()
                .Setup(c => c.FindUser())
                .Returns(new User { Username = "existing" });

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IUserService>()
                .Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_seed_when_authentication_method_is_external()
        {
            Mocker.GetMock<IConfigFileProvider>()
                .SetupGet(c => c.AuthenticationMethod)
                .Returns(AuthenticationType.External);

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IUserService>()
                .Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_migrate_credentials_from_config_file()
        {
            File.WriteAllText(_configPath, "<Config><Username>olduser</Username><Password>oldpass</Password></Config>");

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(_configPath))
                .Returns(true);

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IUserService>()
                .Verify(c => c.Add("olduser", "oldpass"), Times.Once());
        }

        [Test]
        public void should_seed_a_new_user_when_config_file_credentials_are_empty()
        {
            File.WriteAllText(_configPath, "<Config><Username></Username><Password></Password></Config>");

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(_configPath))
                .Returns(true);

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IUserService>()
                .Verify(c => c.Add(string.Empty, string.Empty), Times.Never());

            Mocker.GetMock<IUserService>()
                .Verify(c => c.Add("sonarr", It.Is<string>(p => p.Length == 20)), Times.Once());
        }

        [Test]
        public void should_log_and_continue_when_seeding_fails()
        {
            Mocker.GetMock<IUserService>()
                .Setup(c => c.Add(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("database is locked"));

            Assert.DoesNotThrow(() => Subject.Handle(new ApplicationStartedEvent()));

            ExceptionVerification.IgnoreErrors();
        }
    }
}
