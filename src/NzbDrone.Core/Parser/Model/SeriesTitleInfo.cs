using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Parser.Model
{
    public class SeriesTitleInfo
    {
        public string Title { get; set; }
        public string TitleWithoutYear { get; set; }
        public int Year { get; set; }
    }
}
