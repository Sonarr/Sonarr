namespace NzbDrone.Core.Model
{
    public enum EpisodeStatusType
    {
        /// <summary>
        ///   Episode has not aired yet
        /// </summary>
        NotAired ,

        /// <summary>
        /// Episode is ignored
        /// </summary>
        Ignored,

        /// <summary>
        /// Episode has aired but no episode
        /// files have avilable
        /// </summary>
        Missing ,
        
        /// <summary>
        /// Episode is being downloaded
        /// </summary>
        Downloading ,

        /// <summary>
        /// Episode is present in disk
        /// </summary>
        Ready
    }
}