using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.Model.Xbmc
{
    public class Command
    {
        public string jsonrpc
        {
            get { return "2.0"; }
        }

        public string method { get; set; }
        public Params @params { get; set; }
        public long id { get; set; }
    }
}
