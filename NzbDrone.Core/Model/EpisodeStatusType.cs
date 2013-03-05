namespace NzbDrone.Core.Model
{
    public enum EpisodeStatusType
    {
        /// <summary>
        ///   Episode has not aired yet
        /// </summary>
        NotAired,

        /// <summary>
        /// Episode is ignored
        /// </summary>
        Ignored,

        /// <summary>
        /// Episode has aired, but no episode
        /// files are available
        /// Todo: We shouldn't set missing until the episode has past the actual airtime + runtime, including UtcOffset
        /// </summary>
        Missing,

        /// <summary>
        /// Episode airs today, but no episode
        /// files are available
        /// </summary>
        AirsToday,
        
        /// <summary>
        /// Episode is being downloaded
        /// </summary>
        Downloading,

        /// <summary>
        /// Episode has been downloaded and is unpacking (_UNPACK_)
        /// </summary>
        Unpacking,

        /// <summary>
        /// Episode has failed to download properly (_FAILED_)
        /// </summary>
        Failed,

        /// <summary>
        /// Episode is present in disk
        /// </summary>
        Ready
    }
}