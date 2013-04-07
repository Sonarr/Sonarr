using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NzbDrone.Core.Model.Nzbx.JsonConverter;

namespace NzbDrone.Core.Model.Nzbx
{
    public class NzbxSearchItem
    {
        //"name": "30.Rock.S06E06E07.HDTV.XviD-LOL",
        //"fromname": "teevee@4u.tv (teevee)",
        //"size": 418067671,
        //"groupid": 4,
        //"categoryid": 5030,
        //"totalpart": 36,
        //"completion": 100,
        //"rageid": "-1",
        //"imdbid": "",
        //"comments": "0",
        //"guid": "97be14dbf1776eec4fb8f2bb835935c0",
        //"adddate": 1355343562,
        //"postdate": 1328839361,
        //"downloads": "0",
        //"votes": {

        //    "upvotes": 0,
        //    "downvotes": 0

        //},
        //"nzb": "https://nzbx.co/nzb?97be14dbf1776eec4fb8f2bb835935c0*|*30.Rock.S06E06E07.HDTV.XviD-LOL

        public string Name { get; set; }
        public int TotalPart { get; set; }
        public int GroupId { get; set; }
        public long Size { get; set; }

        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime AddDate { get; set; }

        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime PostDate { get; set; }

        public string Guid { get; set; }
        public string FromName { get; set; }
        public int Completion { get; set; }
        public int CategoryId { get; set; }
        public string ImdbId { get; set; }
        public int RageId { get; set; }
        public int Comments { get; set; }
        public int Downloads { get; set; }
        public NzbxVotesModel Votes { get; set; }
        public string Nzb { get; set; }
    }
}
