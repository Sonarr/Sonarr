using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace NzbDrone.Web.Models
{

    public class SettingsModel
    {
            
        [Required]
        [DataType(DataType.Text)]
        [DisplayName("TV Folder")]
        public String TvFolder
        {
            get;
            set;
        }
    }

}
