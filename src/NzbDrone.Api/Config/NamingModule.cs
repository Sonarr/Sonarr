using System.Collections.Generic;
using FluentValidation;
using Nancy.Responses;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using Nancy.ModelBinding;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.Config
{
    public class NamingModule : NzbDroneRestModule<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IBuildFileNames _buildFileNames;

        public NamingModule(INamingConfigService namingConfigService, IBuildFileNames buildFileNames)
            : base("config/naming")
        {
            _namingConfigService = namingConfigService;
            _buildFileNames = buildFileNames;
            GetResourceSingle = GetNamingConfig;
            GetResourceById = GetNamingConfig;
            UpdateResource = UpdateNamingConfig;

            Get["/samples"] = x => GetExamples(this.Bind<NamingConfigResource>());

            SharedValidator.RuleFor(c => c.MultiEpisodeStyle).InclusiveBetween(0, 3);
        }

        private void UpdateNamingConfig(NamingConfigResource resource)
        {
            _namingConfigService.Save(resource.InjectTo<NamingConfig>());
        }

        private NamingConfigResource GetNamingConfig()
        {
            return _namingConfigService.GetConfig().InjectTo<NamingConfigResource>();
        }

        private NamingConfigResource GetNamingConfig(int id)
        {
            return GetNamingConfig();
        }

        private JsonResponse<NamingSampleResource> GetExamples(NamingConfigResource config)
        {
            var nameSpec = config.InjectTo<NamingConfig>();

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
                Path = @"C:\Test\Series.Title.S01E01.720p.HDTV.x264-EVOLVE.mkv"
            };

            var sampleResource = new NamingSampleResource();

            sampleResource.SingleEpisodeExample = _buildFileNames.BuildFilename(new List<Episode> { episode1 },
                                                                          series,
                                                                          episodeFile,
                                                                          nameSpec);

            episodeFile.Path = @"C:\Test\Series.Title.S01E01-E02.720p.HDTV.x264-EVOLVE.mkv";

            sampleResource.MultiEpisodeExample = _buildFileNames.BuildFilename(new List<Episode> { episode1, episode2 },
                                                                         series,
                                                                         episodeFile,
                                                                         nameSpec);

            return sampleResource.AsResponse();
        }
    }
}