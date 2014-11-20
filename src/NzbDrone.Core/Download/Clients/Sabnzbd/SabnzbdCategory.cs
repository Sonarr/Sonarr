using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdConfig
    {
        public SabnzbdConfigMisc Misc { get; set; }

        public List<SabnzbdCategory> Categories { get; set; }

        public List<Object> Servers { get; set; }
    }

    public class SabnzbdConfigMisc
    {
        public String complete_dir { get; set; }
        public String[] tv_categories { get; set; }
        public Boolean enable_tv_sorting { get; set; }
        public Boolean pre_check { get; set; }
    }

    public class SabnzbdCategory
    {
        public Int32 Priority { get; set; }
        public String PP { get; set; }
        public String Name { get; set; }
        public String Script { get; set; }
        public String Dir { get; set; }

        public OsPath FullPath { get; set; }
    }
}
