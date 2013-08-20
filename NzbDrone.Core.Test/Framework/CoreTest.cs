using System.IO;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Framework
{
    public abstract class CoreTest : TestBase
    {
        protected FileStream OpenRead(params string[] path)
        {
            return File.OpenRead(Path.Combine(path));
        }

        protected string ReadAllText(params string[] path)
        {
            return File.ReadAllText(Path.Combine(path));
        }

        protected void UseRealHttp()
        {
            Mocker.SetConstant<IHttpProvider>(new HttpProvider(TestLogger));
        }

        protected void UseRealDisk()
        {
            Mocker.SetConstant<IDiskProvider>(new DiskProvider());
            WithTempAsAppPath();
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
