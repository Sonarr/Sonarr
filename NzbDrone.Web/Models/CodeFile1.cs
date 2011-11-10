using System;

namespace NzbDrone.Web.Models
{
    public class FooterModel
    {
        public Version Version { get; set; }
        public DateTime BuildTime { get; set; }
    }
}