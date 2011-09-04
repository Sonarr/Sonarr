using System;
using System.Collections.Generic;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class PendingProcessingModel
    {
        public string Name { get; set; }
        public string Files { get; set; }
        public DateTime Created { get; set; }
        public string Path { get; set; }
    }
}