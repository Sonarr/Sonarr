using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class MediaFileProvider : IMediaFileProvider
    {


        private readonly IDiskProvider _diskProvider;
        private readonly IEpisodeProvider _episodeProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtentions = new[] { "*.mkv", "*.avi", "*.wmv" };

        public MediaFileProvider(IDiskProvider diskProvider, IEpisodeProvider episodeProvider)
        {
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
        }

        /// <summary>
        /// Scans the specified series folder for media files
        /// </summary>
        /// <param name="series">The series to be scanned</param>
        public void Scan(Series series)
        {
            var mediaFileList = new List<string>();
            Logger.Info("Scanning '{0}'", series.Path);
            foreach (var ext in MediaExtentions)
            {
                mediaFileList.AddRange(_diskProvider.GetFiles(series.Path, ext, SearchOption.AllDirectories));
            }

            Logger.Info("{0} media files were found", mediaFileList.Count);

            foreach (var file in mediaFileList)
            {
                var episode = Parser.ParseEpisodeInfo(file);
            }
        }
    }
}
