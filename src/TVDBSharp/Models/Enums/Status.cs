namespace TVDBSharp.Models.Enums
{
    /// <summary>
    ///     Describes the current status of a show.
    /// </summary>
    public enum Status
    {
        /// <summary>
        ///     No more episodes are being released.
        /// </summary>
        Ended,

        /// <summary>
        ///     The show is ongoing.
        /// </summary>
        Continuing,

        /// <summary>
        ///     Default value if no status is specified.
        /// </summary>
        Unknown
    }
}