using System;

namespace NzbDrone.Core.Parser.Model
{
    [Flags]
    public enum IndexerFlags
    {
        /// <summary>
        /// Torrent download amount does not count
        /// </summary>
        Freeleech = 1,

        /// <summary>
        /// Torrent download amount only counts 50%
        /// </summary>
        Halfleech = 2,

        /// <summary>
        /// Torrent upload amount is doubled
        /// </summary>
        DoubleUpload = 4,

        /// <summary>
        /// Uploader is an internal release group
        /// </summary>
        Internal = 8,

        /// <summary>
        /// The release comes from a scene group
        /// </summary>
        Scene = 16,

        /// <summary>
        /// Torrent download amount only counts 75%
        /// </summary>
        Freeleech75 = 32,

        /// <summary>
        /// Torrent download amount only counts 25%
        /// </summary>
        Freeleech25 = 64,

        /// <summary>
        /// The release is nuked
        /// </summary>
        Nuked = 128
    }
}
