using System;
using System.Collections.Generic;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class PendingProcessingModel
    {
        public string Name { get; set; }
        public string Files { get; set; }
        public string Created { get; set; }
        public string Path { get; set; }
        public string Actions { get; set; }
        public string Details { get; set; }
    }
}