using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download.Clients.Sabnzbd.JsonConverters;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdConfig
    {
        public SabnzbdConfigMisc Misc { get; set; }

        public List<SabnzbdCategory> Categories { get; set; }

        public List<object> Servers { get; set; }
    }

    public class SabnzbdConfigMisc
    {
        public string complete_dir { get; set; }
        public string[] tv_categories { get; set; }
        public bool enable_tv_sorting { get; set; }
        public string[] movie_categories { get; set; }
        public bool enable_movie_sorting { get; set; }
        [JsonConverter(typeof(SabnzbdStringArrayConverter))]
        public string[] date_categories { get; set; }
        public bool enable_date_sorting { get; set; }
        public bool pre_check { get; set; }
    }

    public class SabnzbdCategory
    {
        public int Priority { get; set; }
        public string PP { get; set; }
        public string Name { get; set; }
        public string Script { get; set; }
        public string Dir { get; set; }

        public OsPath FullPath { get; set; }
    }
}
