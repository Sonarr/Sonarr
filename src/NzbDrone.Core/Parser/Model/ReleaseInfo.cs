using System;

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

        public DateTime PublishDate { get; set; }

        public int Age
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

        public string ReleaseGroup { get; set; }
        public int TvRageId { get; set; }
    }
}