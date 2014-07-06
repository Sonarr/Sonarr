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

        public int WeightedQuality { get; set; }

        //OZnzb specific properties.
        public int OZnzbSpamReports { get; set; }
        public bool OZnzbIsSpamConfirmed { get; set; }
        public int OZnzbPasswordedReports { get; set; }
        public bool OZnzbIsPasswordedConfirmed { get; set; }
        public int OZnzbUpVotes { get; set; }
        public int OZnzbDownVotes { get; set; }
        public double OZnzbVideoRating { get; set; }
        public double OZnzbAudioRating { get; set; }
    }
}