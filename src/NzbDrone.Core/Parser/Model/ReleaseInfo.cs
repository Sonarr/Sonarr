using System;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Parser.Model
{
    using System.Text;

    public class ReleaseInfo
    {
        public String Guid { get; set; }
        public String Title { get; set; }
        public Int64 Size { get; set; }
        public String DownloadUrl { get; set; }
        public String InfoUrl { get; set; }
        public String CommentUrl { get; set; }
        public String Indexer { get; set; }
        public DownloadProtocol DownloadProtocol { get; set; }
        public Int32 TvRageId { get; set; }
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

        public Double AgeMinutes
        {
            get
            {
                return DateTime.UtcNow.Subtract(PublishDate).TotalMinutes;
            }

            //This prevents manually downloading a release from blowing up in mono
            //TODO: Is there a better way?
            private set { }
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1} [{2}]", PublishDate, Title, Size);
        }

        public virtual string ToString(string format)
        {
            switch (format.ToUpperInvariant())
            {
                case "L": // Long format
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Guid: " + Guid ?? "Empty");
                    stringBuilder.AppendLine("Title: " + Title ?? "Empty");
                    stringBuilder.AppendLine("Size: " + Size ?? "Empty");
                    stringBuilder.AppendLine("InfoUrl: " + InfoUrl ?? "Empty");
                    stringBuilder.AppendLine("DownloadUrl: " + DownloadUrl ?? "Empty");
                    stringBuilder.AppendLine("Indexer: " + Indexer ?? "Empty");
                    stringBuilder.AppendLine("CommentUrl: " + CommentUrl ?? "Empty");
                    stringBuilder.AppendLine("DownloadProtocol: " + DownloadProtocol ?? "Empty");
                    stringBuilder.AppendLine("TvRageId: " + TvRageId ?? "Empty");
                    stringBuilder.AppendLine("PublishDate: " + PublishDate ?? "Empty");
                    return stringBuilder.ToString();
                default:
                    return ToString();
            }
        }
    }
}