using NzbDrone.Api.SignalR;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Api.RootFolders
{
    public class RootFolderConnection : BasicResourceConnection<RootFolder>
    {
        public override string Resource
        {
            get { return "RootFolder"; }
        }
    }
}
