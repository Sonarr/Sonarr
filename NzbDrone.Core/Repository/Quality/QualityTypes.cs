namespace NzbDrone.Core.Repository.Quality
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    ///   Represents Video Quality
    /// </summary>
    public enum QualityTypes
    {
        /// <summary>
        ///   Quality is unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///   SD File (Source could be HD)
        /// </summary>
        SDTV = 1,

        /// <summary>
        ///   SD File (DVD Source)
        /// </summary>
        DVD = 2,

        /// <summary>
        ///   HD File (HDTV Source)
        /// </summary>
        HDTV = 4,

        /// <summary>
        ///   HD File (Online Source)
        /// </summary>
        WEBDL = 5,

        /// <summary>
        ///   HD File (720p Blu-ray Source)
        /// </summary>
        Bluray720p = 6,

        /// <summary>
        ///   HD File (1080p Blu-ray Source)
        /// </summary>
        Bluray1080p = 7,
    }
}