using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Controllers
{
    public interface IDownloadClientController
    {
        bool AddByUrl(ItemInfo nzb); //Should accept something other than string (NzbInfo?) returns success or failure
        bool IsInQueue(Episode episode);//Should accept something other than string (Episode?) returns bool
    }
}
