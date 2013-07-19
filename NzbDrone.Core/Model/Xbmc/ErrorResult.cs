using System.Collections.Generic;

namespace NzbDrone.Core.Model.Xbmc
{
    public class ErrorResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public Dictionary<string, string> Error { get; set; }
    }
}
