using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{

    public class SettingsModel
    {

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Please enter a valid TV path")]
        [DisplayName("TV Folder")]
        public String TvFolder
        {
            get;
            set;
        }

        [DataType(DataType.Text)]
        [DisplayName("Initial Quality")]
        public int Quality
        {
            get;
            set;
        }
    }
}
