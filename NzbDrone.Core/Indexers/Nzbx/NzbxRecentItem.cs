using System;

namespace NzbDrone.Core.Indexers.Nzbx
{
    public class NzbxRecentItem
    {
        //"ID": "571777",
        //"name": "Cak4QCQG",
        //"totalpart": "10",
        //"groupID": "99",
        //"size": "890190951",
        //"postdate": "2012-12-20 18:14:13",
        //"guid": "48714abb00a095e00fbcbe161253abf6",
        //"fromname": "#cripples <masturb@ting.in.wheelchairs>",
        //"completion": "100",
        //"categoryID": "5050",
        //"imdbID": null,
        //"anidbID": null,
        //"rageID": "-1",
        //"comments": "0",
        //"downloads": "3",
        //"votes": {
        //    "upvotes": 0,
        //    "downvotes": 0
        //}

        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime PostDate { get; set; }
        public string Guid { get; set; }
    }
}
