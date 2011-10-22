namespace NzbDrone.Core.Model
{
    public enum PostDownloadStatusType
    {

        Unknown = 0,
        Unpacking = 1,
        Failed = 2,
        Processed = 3,
        InvalidSeries = 4,
        ParseError = 5,
        InvalidEpisode = 6,
        NoError = 7,
    }
}