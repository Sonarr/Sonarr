using System;
using System.Linq;
using NzbDrone.Common;
using Sqo;

namespace NzbDrone.Core.Datastore
{
    public interface IObjectDbFactory
    {
        IObjectDatabase CreateMemoryDb();
        IObjectDatabase Create(string dbPath);
    }

    public class SiaqoDbFactory : IObjectDbFactory
    {
        private readonly DiskProvider _diskProvider;

        public SiaqoDbFactory(DiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public IObjectDatabase CreateMemoryDb()
        {
            throw new NotImplementedException();
        }

        public IObjectDatabase Create(string dbPath)
        {
            if(!_diskProvider.FolderExists(dbPath))
            {
                _diskProvider.CreateDirectory(dbPath);
            }

            SiaqodbConfigurator.SetTrialLicense("uvhpW4hT5Rtq+Uoyq8MOm1Smon15foxV5iS5bAegIXU=");

            var db = new Siaqodb(dbPath);

            return new SiaqodbProxy(db);
        }
    }
}
