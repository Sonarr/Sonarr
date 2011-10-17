namespace NzbDrone.Core.Model
{
    public enum PostDownloadStatusType
    {
        /// <summary>
        ///   Unknown (Default)
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///   Unpacking
        /// </summary>
        Unpacking = 1,

        /// <summary>
        /// Failed
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Processed
        /// </summary>
        Processed = 3,

        /// <summary>
        /// InvalidSeries
        /// </summary>
        InvalidSeries = 4,

        /// <summary>
        /// ParseError
        /// </summary>
        ParseError = 5,

        /// <summary>
        /// InvalidEpisode
        /// </summary>
        InvalidEpisode = 6,
    }
}