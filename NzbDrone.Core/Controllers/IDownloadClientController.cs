using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Controllers
{
    public interface IDownloadClientController
    {
        string AddByUrl(string url); //Should accept something other than string (NzbInfo?) returns result if applicable
        bool IsInQueue(string goodName);//Should accept something other than string (Episode?) returns bool
    }
}
