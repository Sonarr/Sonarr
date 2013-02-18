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

        protected static void ThrowException()
        {
            throw new ApplicationException("This is a message for test exception");
        }


        protected FileStream OpenRead(params string[] path)
        {
            return File.OpenRead(Path.Combine(path));
        }
    }

    public abstract class CoreTest<TSubject> : CoreTest
    {
        [SetUp]
        public void CoreTestSetup()
        {
            Subject = Mocker.Resolve<TSubject>();
        }

        protected TSubject Subject { get; set; }
    }
}
