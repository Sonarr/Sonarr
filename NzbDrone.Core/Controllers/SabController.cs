using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Controllers
{
    public class SabController : IDownloadClientController
    {
        private readonly IConfigController _config;

        public SabController(IConfigController config)
        {
            _config = config;
        }

        public string AddByUrl(string url)
        {
             

            return "";
        }

        public string AddByPath(string path)
        {

            return "";
        }


        public bool IsInQueue(string goodName)
        {

            return false;
        }
    }
}
