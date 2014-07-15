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
        public int TvRageId { get; set; }
        public DateTime PublishDate { get; set; }

        public Int32 Age
        {
            get
            {
                return DateTime.UtcNow.Subtract(PublishDate).Days;
            }

            //This prevents manually downloading a release from blowing up in mono
            //TODO: Is there a better way?
            private set { }
        }

        public Double AgeHours
        {
            get
            {
                return DateTime.UtcNow.Subtract(PublishDate).TotalHours;
            }

            //This prevents manually downloading a release from blowing up in mono
            //TODO: Is there a better way?
            private set { }
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1} [{2}]", PublishDate, Title, Size);
        }

        public int SpamReports { get; set; }
        public bool IsSpamConfirmed { get; set; }
        public int PasswordedReports { get; set; }
        public bool IsPasswordedConfirmed { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public double VideoRating { get; set; }
        public double AudioRating { get; set; }
        public double RatingCeiling { get; set; }
        public bool IsUsingWeightedQuality { get; set; }
        public int WeightedQuality { get { return CalculateWeightedQuality(); } }

        private int CalculateWeightedQuality()
        {
            if (IsUsingWeightedQuality)
            {
                //Generate a weighted priority which any indexer should be able to hook into while trying to keep the numbers fair across all indexers. May require some tweaking
                int weightedPriority = 0;
                int totalNegativeReports = PasswordedReports + SpamReports;
                weightedPriority = totalNegativeReports * -20; //drastically reduce weight for negative reports such as virus spam or passwords.
                weightedPriority += UpVotes - DownVotes; //add or substact a point for every up or down vote
                if (DownVotes > UpVotes && DownVotes >= 2)
                {
                    weightedPriority += -20; // drastically reduce weight if there are more down votes than up votes and at least two down votes.
                }

                if (RatingCeiling != 0)
                {
                    //convert any rating system to an out of 10 system, e.g. rating 3 out of 5 or 12 out of 20 all become 6 out of 10.
                    double ratingMultiplier = 10 / RatingCeiling;
                    int videoRatingOf10 = (int)Math.Round(VideoRating * ratingMultiplier);
                    int audioRatingOf10 = (int)Math.Round(AudioRating * ratingMultiplier);

                    //increase or decrease weight for quality rating, 4 and under are considered bad quality and are negative weighted.
                    if (VideoRating + AudioRating != 0)
                    {
                        weightedPriority += (videoRatingOf10 >= 4 ? videoRatingOf10 : videoRatingOf10 - 5);
                        weightedPriority += (audioRatingOf10 >= 4 ? audioRatingOf10 : videoRatingOf10 - 5);
                    }
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