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
        TV = 1,
        /// <summary>
        ///   SD File (DVD Source)
        /// </summary>
        DVD = 2,
        /// <summary>
        ///   HD File (HDTV Source)
        /// </summary>
        HDTV = 3,
        /// <summary>
        ///   HD File (Online Source)
        /// </summary>
        WEBDL = 4,
        /// <summary>
        ///   720P HD File (Blu-ray Source)
        /// </summary>
        Bluray720p = 5,
        /// <summary>
        ///   1080P HD File (Blu-ray Source)
        /// </summary>
        Bluray1080p = 6
    }
}