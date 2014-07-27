using System;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Parser.Model
{
    public class ReleaseInfo
    {
        public string Title { get; set; }
        public long Size { get; set; }
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public string CommentUrl { get; set; }
        public String Indexer { get; set; }
        public DownloadProtocol DownloadProtocol { get; set; }

        public DateTime PublishDate { get; set; }

        public Int32 Age
        {
            get
            {
                return DateTime.UtcNow.Subtract(PublishDate).Days;
            }

            //This prevents manually downloading a release from blowing up in mono
            //TODO: Is there a better way?
            private set
            {
                
            }
        }

        public Double AgeHours
        {
            get
            {
                return DateTime.UtcNow.Subtract(PublishDate).TotalHours;
            }

            //This prevents manually downloading a release from blowing up in mono
            //TODO: Is there a better way?
            private set
            {

            }
        }

        public int TvRageId { get; set; }

        public override string ToString()
        {
            return String.Format("[{0}] {1} [{2}]", PublishDate, Title, Size);
        }

        public ReleaseUserRatings UserRatings { get; set; }

        public int WeightedQuality { get { return CalculateWeightedQuality(); } }

        private int CalculateWeightedQuality()
        {
            if (UserRatings != null)
            {
                //Generate a weighted priority which any indexer should be able to hook into while trying to keep the numbers fair across all indexers. May require some tweaking
                int weightedPriority = 0;
                weightedPriority = (UserRatings.PasswordedReports + UserRatings.SpamReports) * -20; //drastically reduce weight for negative reports such as virus spam or passwords.
                weightedPriority += UserRatings.UpVotes - UserRatings.DownVotes; //add or substact a point for every up or down vote
                if (UserRatings.DownVotes > UserRatings.UpVotes && UserRatings.DownVotes >= 2)
                {
                    weightedPriority += -20; // drastically reduce weight if there are more down votes than up votes and at least two down votes.
                }

                if (UserRatings.RatingCeiling != 0)
                {
                    //convert any rating system to an out of 10 system, e.g. rating 3 out of 5 or 12 out of 20 all become 6 out of 10.
                    double ratingMultiplier = 10 / UserRatings.RatingCeiling;
                    int videoRatingOf10 = (int)Math.Round(UserRatings.VideoRating * ratingMultiplier);
                    int audioRatingOf10 = (int)Math.Round(UserRatings.AudioRating * ratingMultiplier);

                    weightedPriority += videoRatingOf10 != 0 && videoRatingOf10 >= 7 ? 5 : 0; //add 5 if video rating is 7 or more.
                    weightedPriority += audioRatingOf10 != 0 && audioRatingOf10 >= 7 ? 5 : 0; //add 5 if audio rating is 7 or more.
                    weightedPriority += videoRatingOf10 != 0 && videoRatingOf10 <= 3 ? 5 : 0; //minus 5 if video rating is 4 or less.
                    weightedPriority += audioRatingOf10 != 0 && audioRatingOf10 <= 3 ? 5 : 0; //minus 5 if audio rating is 4 or less.
                }

                return weightedPriority;
            }
            else
            {
                return 0;
            }
        }


    }
}