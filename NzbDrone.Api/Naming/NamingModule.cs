using System;
using System.Collections.Generic;
using NzbDrone.Api.Commands;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Naming
{
    public class NamingModule : NzbDroneRestModule<NamingResource>
    {
        private readonly IBuildFileNames _buildFileNames;

        public NamingModule(IBuildFileNames buildFileNames)
            :base("naming")
        {
            _buildFileNames = buildFileNames;
            CreateResource = GetExamples;
        }

        private NamingResource GetExamples(NamingResource resource)
        {
            var nameSpec = new NamingConfig();
            nameSpec.InjectFrom(resource);

            var series = new Core.Tv.Series
            {
                SeriesType = SeriesTypes.Standard,
                Title = "Series Title"
            };

            var episode1 = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 1,
                Title = "Episode Title (1)"
            };

            var episode2 = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 2,
                Title = "Episode Title (2)"
            };

            var episodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p),
                Path = @"C:\Test\Series.Title.S01E01.hdtv.avi"
            };

            resource.SingleEpisodeExample = _buildFileNames.BuildFilename(new List<Episode> { episode1 },
                                                                          series,
                                                                          episodeFile,
                                                                          nameSpec);

            episodeFile.Path = @"C:\Test\Series.Title.S01E01-E02.hdtv.avi";

            resource.MultiEpisodeExample = _buildFileNames.BuildFilename(new List<Episode> { episode1, episode2 },
                                                                         series,
                                                                         episodeFile,
                                                                         nameSpec);

            return resource;
        }
    }
}