using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class SeriesDeleteMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public bool DeleteFiles { get; set; }
        public string DeletedFiles { get; set; }

        public override string ToString()
        {
            return Message;
        }
        public SeriesDeleteMessage (Series series, bool deleteFiles)
        {
            Series = series;
            DeleteFiles = deleteFiles;
            if (DeleteFiles == true)
            {
                DeletedFiles = "Series removed, files were not deleted";
            }
            else
            {
                DeletedFiles = "Series removed and all files were deleted";
            }
            Message = series.Title + " - " + DeletedFiles;
        }
    }
}
