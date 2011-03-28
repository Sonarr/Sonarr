using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NzbDrone.Web.Models
{
    public class AddExistingManualModel
    {
        public string Path { get; set; }
        public string FolderName { get; set; }

        [DisplayName("Quality Profile")]
        public int QualityProfileId { get; set; }

        public SelectList QualitySelectList { get; set; }
    }
}