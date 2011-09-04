using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.Model.Xbmc
{
    public class TvShow
    {
        public int TvShowId { get; set; }
        public string Label { get; set; }
        public int ImdbNumber { get; set; }
        public string File { get; set; }
    }
}
