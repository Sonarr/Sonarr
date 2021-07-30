using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Test.Common
{
    public abstract class TestBase<TSubject> : TestBase
        where TSubject : class
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
        private static readonly Random _random = new Random();
        private static int _nextUid;

        private AutoMoqer _mocker;
        protected AutoMoqer Mocker
        {
            get
            {
                if (_mocker == null)
                {
                    _mocker = new AutoMoqer();
                    _mocker.SetConstant<ICacheManager>(new CacheManager());
                    _mocker.SetConstant<IStartupContext>(new StartupContext(Array.Empty<string>()));
                    _mocker.SetConstant(TestLogger);
                }

                return _mocker;
            }
        }


        protected int RandomNumber
        {
            get
            {
                Thread.Sleep(1);
                return _random.Next(0, int.MaxValue);
            }
        }

        private string VirtualPath
        {
            get
            {
                var virtualPath = Path.Combine(TempFolder, "VirtualNzbDrone");
                if (!Directory.Exists(virtualPath))
                {
                    Directory.CreateDirectory(virtualPath);
                }

                return virtualPath;
            }
        }

        private string _tempFolder;
        protected string TempFolder
        {
            get
            {
                if (_tempFolder == null)
                {
                    _tempFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "_temp_" + GetUID());

                    Directory.CreateDirectory(_tempFolder);
                }

                return _tempFolder;
            }
        }

        [SetUp]
        public void TestBaseSetup()
        {
            GetType().IsPublic.Should().BeTrue("All Test fixtures should be public to work in mono.");

            LogManager.ReconfigExistingLoggers();

            _tempFolder = null;
        }

        [TearDown]
        public void TestBaseTearDown()
        {
            _mocker = null;

            DeleteTempFolder(_tempFolder);
        }


        public static string GetUID()
        {
            return ProcessProvider.GetCurrentProcessId() + "_" + DateTime.Now.Ticks + "_" + Interlocked.Increment(ref _nextUid);
        }

        public static void DeleteTempFolder(string folder)
        {
            if (folder == null)
            {
                return;
            }

            try
            {
                var tempFolder = new DirectoryInfo(folder);
                if (tempFolder.Exists)
                {
                    foreach (var file in tempFolder.GetFiles("*", SearchOption.AllDirectories))
                    {
                        file.IsReadOnly = false;
                    }

                    tempFolder.Delete(true);
                }
            }
            catch (Exception)
            {
            }
        }

        protected IAppFolderInfo TestFolderInfo { get; private set; }

        protected void WindowsOnly()
        {
            if (OsInfo.IsNotWindows)
            {
                throw new IgnoreException("windows specific test");
            }
        }

        protected void PosixOnly()
        {
            if (OsInfo.IsWindows)
            {
                throw new IgnoreException("non windows specific test");
            }
        }

        protected void NotBsd()
        {
            if (OsInfo.Os == Os.Bsd)
            {
                throw new IgnoreException("Ignored on BSD");
            }
        }

        protected void WithTempAsAppPath()
        {
            Mocker.GetMock<IAppFolderInfo>()
                .SetupGet(c => c.AppDataFolder)
                .Returns(VirtualPath);

            TestFolderInfo = Mocker.GetMock<IAppFolderInfo>().Object;
        }

        protected string GetTestPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine(path.Split('/')));
        }

        protected string ReadAllText(string path)
        {
            return File.ReadAllText(GetTestPath(path));
        }

        protected string GetTempFilePath()
        {
            return Path.Combine(TempFolder, Path.GetRandomFileName());
        }

        protected void VerifyEventPublished<TEvent>()
            where TEvent : class, IEvent
        {
            VerifyEventPublished<TEvent>(Times.Once());
        }

        protected void VerifyEventPublished<TEvent>(Times times)
            where TEvent : class, IEvent
        {
            Mocker.GetMock<IEventAggregator>().Verify(c => c.PublishEvent(It.IsAny<TEvent>()), times);
        }

        protected void VerifyEventNotPublished<TEvent>()
            where TEvent : class, IEvent
        {
            Mocker.GetMock<IEventAggregator>().Verify(c => c.PublishEvent(It.IsAny<TEvent>()), Times.Never());
        }
    }
}
