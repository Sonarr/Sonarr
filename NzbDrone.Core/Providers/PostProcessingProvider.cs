using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class PostProcessingProvider : IPostProcessingProvider
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly IMediaFileProvider _mediaFileProvider;
        private readonly IRenameProvider _renameProvider;

        public PostProcessingProvider(ISeriesProvider seriesProvider,
            IMediaFileProvider mediaFileProvider, IRenameProvider renameProvider)
        {
            _seriesProvider = seriesProvider;
           _mediaFileProvider = mediaFileProvider;
            _renameProvider = renameProvider;
        }

        #region IPostProcessingProvider Members

        public void ProcessEpisode(string dir, string nzbName)
        {
            var parsedSeries = Parser.ParseSeriesName(nzbName);
            var series = _seriesProvider.FindSeries(parsedSeries);

            if (series == null)
                return;

            //Import the files, and then rename the newly added ones.
            var fileList = _mediaFileProvider.Scan(series, dir);

            foreach (var file in fileList)
            {
                //Notifications will be sent from the Renamer, depending on the bool NewDownload (which will be set to true from here), a normal rename will be treated as such.
                _renameProvider.RenameEpisodeFile(file.EpisodeFileId, true);
            }
        }

        #endregion
    }
}
