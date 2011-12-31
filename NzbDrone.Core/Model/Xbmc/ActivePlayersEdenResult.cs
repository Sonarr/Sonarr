using System;
using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Core.Model.Xbmc
{
    public class ActivePlayersEdenResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public List<ActivePlayer> Result { get; set; }
    }

    public class ActivePlayer
    {
        public int PlayerId { get; set; }
        public string Type { get; set; }
    }
}
