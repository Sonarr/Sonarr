using System;
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
