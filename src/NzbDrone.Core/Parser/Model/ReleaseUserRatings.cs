using System;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Parser.Model
{
    public class ReleaseUserRatings
    {
        public int SpamReports { get; set; }
        public bool IsSpamConfirmed { get; set; }
        public int PasswordedReports { get; set; }
        public bool IsPasswordedConfirmed { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public double VideoRating { get; set; }
        public double AudioRating { get; set; }
        public double RatingCeiling { get; set; }
    }
}