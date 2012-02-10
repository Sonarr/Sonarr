using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class JobModel
    {
        public Int32 Id { get; set; }
        public Boolean Enable { get; set; }
        public String TypeName { get; set; }
        public String Name { get; set; }
        public Int32 Interval { get; set; }
        public String LastExecution { get; set; }
        public Boolean Success { get; set; }
        public string Actions { get; set; }
    }
}