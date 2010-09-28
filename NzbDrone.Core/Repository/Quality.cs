namespace NzbDrone.Core.Repository
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    ///   Represents Video Quality
    /// </summary>
    public enum Quality
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
        HDTV = 3,
        /// <summary>
        ///   HD File (Online Source)
        /// </summary>
        WEBDL = 4,
        /// <summary>
        ///   HD File (Blu-ray Source)
        /// </summary>
        Bluray = 5
    }
}