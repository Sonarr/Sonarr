using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class SeriesDeleteMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public bool DeletedFiles { get; set; }
        public string DeletedFilesMessage { get; set; }

        public override string ToString()
        {
            return Message;
        }

        public SeriesDeleteMessage(Series series, bool deleteFiles)
        {
            Series = series;
            DeletedFiles = deleteFiles;
            DeletedFilesMessage = DeletedFiles ?
                "Series removed and all files were deleted" :
                "Series removed, files were not deleted";
            Message = series.Title + " - " + DeletedFilesMessage;
        }
    }
}
