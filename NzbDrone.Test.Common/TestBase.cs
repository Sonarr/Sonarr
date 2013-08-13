using System;
using System.IO;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Test.Common
{
    public abstract class TestBase<TSubject> : TestBase where TSubject : class
    {

        private TSubject _subject;

        [SetUp]
        public void CoreTestSetup()
        {
            _subject = null;

        }

        protected TSubject Subject
        {
            get
            {
                if (_subject == null)
                {
                    _subject = Mocker.Resolve<TSubject>();
                }

                return _subject;
            }

        }

    }

    public abstract class TestBase : LoggingTest
    {
        private AutoMoqer _mocker;
        protected AutoMoqer Mocker
        {
            get
            {
                if (_mocker == null)
                {
                    _mocker = new AutoMoqer();
                }

                return _mocker;
            }
        }

        protected Mock<RestProvider> MockedRestProvider { get; private set; }

        private string VirtualPath
        {
            get
            {
                var virtualPath = Path.Combine(TempFolder, "VirtualNzbDrone");
                if (!Directory.Exists(virtualPath)) Directory.CreateDirectory(virtualPath);

                return virtualPath;
            }
        }

        protected string TempFolder { get; private set; }

        [SetUp]
        public void TestBaseSetup()
        {

            GetType().IsPublic.Should().BeTrue("All Test fixtures should be public to work in mono.");

            Mocker.SetConstant<ICacheManger>(new CacheManger());

            Mocker.SetConstant(LogManager.GetLogger("TestLogger"));

            Mocker.SetConstant<IStartupArguments>(new StartupArguments(new string[0]));

            LogManager.ReconfigExistingLoggers();

            TempFolder = Path.Combine(Directory.GetCurrentDirectory(), "_temp_" + DateTime.Now.Ticks);

            MockedRestProvider = new Mock<RestProvider>();

            Directory.CreateDirectory(TempFolder);
        }

        [TearDown]
        public void TestBaseTearDown()
        {
            _mocker = null;

            try
            {
                if (Directory.Exists(TempFolder))
                {
                    Directory.Delete(TempFolder, true);
                }
            }
            catch (Exception)
            {
            }
        }


        protected IAppFolderInfo TestFolderInfo { get; private set; }

        protected void WindowsOnly()
        {
            if (OsInfo.IsLinux)
            {
                throw new IgnoreException("windows specific test");
            }
        }


        protected void LinuxOnly()
        {
            if (!OsInfo.IsLinux)
            {
                throw new IgnoreException("linux specific test");
            }
        }

        protected void WithTempAsAppPath()
        {
            Mocker.GetMock<IAppFolderInfo>()
                .SetupGet(c => c.AppDataFolder)
                .Returns(VirtualPath);

            TestFolderInfo = Mocker.GetMock<IAppFolderInfo>().Object;
        }

        protected string GetTestFilePath(string fileName)
        {
            return Path.Combine(SandboxFolder, fileName);
        }

        protected string SandboxFolder
        {
            get
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "Files");
                Directory.CreateDirectory(folder);
                return folder;
            }

        }
        protected void VerifyEventPublished<TEvent>() where TEvent : class, IEvent
        {
            VerifyEventPublished<TEvent>(Times.Once());
        }

        protected void VerifyEventPublished<TEvent>(Times times) where TEvent : class, IEvent
        {
            Mocker.GetMock<IMessageAggregator>().Verify(c => c.PublishEvent(It.IsAny<TEvent>()), times);
        }

        protected void VerifyEventNotPublished<TEvent>() where TEvent : class, IEvent
        {
            Mocker.GetMock<IMessageAggregator>().Verify(c => c.PublishEvent(It.IsAny<TEvent>()), Times.Never());
        }
    }
}
