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

/*            if (TestContext.CurrentContext.Result.State == TestState.Failure || TestContext.CurrentContext.Result.State == TestState.Error)
            {
                var testName = TestContext.CurrentContext.Test.Name.ToLower();

                if (IAppDirectoryInfo.IsLinux && testName.Contains("windows"))
                {
                    throw new IgnoreException("windows specific test");
                }
                else if (testName.Contains("linux"))
                {
                    throw new IgnoreException("linux specific test");
                }
            }*/
        }

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
            Mocker.GetMock<IAppDirectoryInfo>()
                .SetupGet(c => c.WorkingDirectory)
                .Returns(VirtualPath);
        }

        protected string GetTestFilePath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
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
