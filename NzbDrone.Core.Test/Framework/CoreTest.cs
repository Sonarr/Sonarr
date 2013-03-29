using System;
using System.IO;
using NUnit.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Framework
{
    public abstract class CoreTest : TestBase
    {
        protected static ProgressNotification MockNotification
        {
            get
            {
                return new ProgressNotification("Mock notification");
            }
        }

        protected FileStream OpenRead(params string[] path)
        {
            return File.OpenRead(Path.Combine(path));
        }

        protected string ReadAllText(params string[] path)
        {
            return File.ReadAllText(Path.Combine(path));
        }
    }

    public abstract class CoreTest<TSubject> : CoreTest where TSubject : class
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
}
