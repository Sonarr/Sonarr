namespace NzbDrone.Core.Model
{
    public enum PostDownloadStatusType
    {
        Unknown = 0,
        Unpacking = 1,
        Failed = 2,
        UnknownSeries = 3,
        ParseError = 4,
        NoError = 5,
    }
}