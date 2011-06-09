using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Models
{
    public class RootDirModel
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string CleanPath { get; set; }
        public SelectList SelectList { get; set; }
    }
}