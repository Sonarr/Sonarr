using System;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Parser.Model
{
    public class ReleaseUserRatings
    {
        public Int32? SpamReports { get; set; }
        public Boolean IsSpamConfirmed { get; set; }
        public Int32? PasswordedReports { get; set; }
        public Boolean IsPasswordedConfirmed { get; set; }
        public Int32? UpVotes { get; set; }
        public Int32? DownVotes { get; set; }
        public Double? VideoRating { get; set; }
        public Double? AudioRating { get; set; }
    }
}