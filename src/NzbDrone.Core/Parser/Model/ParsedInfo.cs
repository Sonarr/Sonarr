using NzbDrone.Core.Qualities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedInfo
    {
        public String Title { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public String ReleaseGroup { get; set; }
        public String ReleaseHash { get; set; }

        public TitleInfo TitleInfo { get; set; }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Title, Quality);
        }
    }
}
