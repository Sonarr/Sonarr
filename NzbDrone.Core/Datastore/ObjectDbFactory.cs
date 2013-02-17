using System.Linq;
using NzbDrone.Common;
using Sqo;

namespace NzbDrone.Core.Datastore
{
    public interface IObjectDbFactory
    {
        IObjectDatabase Create(string dbPath = null);
    }

    public class SiaqoDbFactory : IObjectDbFactory
    {
        private readonly DiskProvider _diskProvider;
        private readonly EnvironmentProvider _environmentProvider;

        public SiaqoDbFactory(DiskProvider diskProvider, EnvironmentProvider environmentProvider)
        {
            _diskProvider = diskProvider;
            _environmentProvider = environmentProvider;
        }

        public IObjectDatabase Create(string dbPath = null)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = _environmentProvider.GetObjectDbFolder();
            }

            if (!_diskProvider.FolderExists(dbPath))
            {
                _diskProvider.CreateDirectory(dbPath);
            }

            SiaqodbConfigurator.SetTrialLicense("uvhpW4hT5Rtq+Uoyq8MOm1Smon15foxV5iS5bAegIXU=");

            var db = new Siaqodb(dbPath);

            return new SiaqodbProxy(db);
        }
    }
}
