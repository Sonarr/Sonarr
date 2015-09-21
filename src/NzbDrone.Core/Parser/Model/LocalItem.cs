using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Qualities;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Parser.Model
{
    public abstract class LocalItem
    {
        public String Path { get; set; }
        public Int64 Size { get; set; }
        public abstract Media Media { get; }
        public abstract IEnumerable<MediaModelBase> MediaFiles { get; }
        

        public ParsedInfo ParsedInfo { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public Boolean ExistingFile { get; set; }

        public abstract bool IsEmpty();
        public override string ToString()
        {
            return Path;
        }
    }
}