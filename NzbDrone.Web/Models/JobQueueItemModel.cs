using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class JobQueueItemModel
    {
        public string Name { get; set; }
        public int TargetId { get; set; }
        public int SecondaryTargetId { get; set; }
    }
}