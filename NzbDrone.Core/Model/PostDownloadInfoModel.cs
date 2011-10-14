using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class PostDownloadInfoModel
    {
        public string Name { get; set; }
        public DateTime Added { get; set; }
        public PostDownloadStatusType Status { get; set; }
    }
}
