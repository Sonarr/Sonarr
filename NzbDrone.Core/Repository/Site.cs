using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Repository
{
    public class Site
    {
        private static readonly IList<Site> Sites = new List<Site>
        {
            new Site {Name = "nzbmatrix", Url = "nzbmatrix.com", Pattern = @"\d{6,10}"},
            new Site {Name = "nzbsDotOrg", Url = "nzbs.org", Pattern = @"\d{5,10}"},
            new Site {Name = "nzbsrus", Url = "nzbsrus.com", Pattern = @"\d{6,10}"},
            new Site {Name = "lilx", Url = "lilx.net", Pattern = @"\d{6,10}"},
        };

        public string Name { get; set; }
        public string Pattern { get; set; }
        public string Url { get; set; }

        // TODO: use HttpUtility.ParseQueryString();
        // https://nzbmatrix.com/api-nzb-download.php?id=626526
        public string ParseId(string url)
        {
            return Regex.Match(url, Pattern).Value;
        }

        public static Site Parse(string url)
        {
            return Sites.Where(site => url.Contains(site.Url)).SingleOrDefault() ??
                new Site { Name = "unknown", Pattern = @"\d{6,10}" };
        }
    }
}
