using System;
using System.Collections.Generic;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class JobQueueItemModel
    {
        public string Name { get; set; }
        public dynamic Options { get; set; }
    }
}