using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.Model.Xbmc
{
    public class TvShowResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public Dictionary<string, List<TvShow>> Result { get; set; }
    }
}
