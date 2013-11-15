using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy.Responses;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
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
            SharedValidator.RuleFor(c => c.StandardEpisodeFormat).ValidEpisodeFormat();
            SharedValidator.RuleFor(c => c.DailyEpisodeFormat).ValidDailyEpisodeFormat();
        }

        private void UpdateNamingConfig(NamingConfigResource resource)
        {
            var nameSpec = resource.InjectTo<NamingConfig>();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);
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
                Title = "Episode Title (1)",
                AirDate = "2013-10-30"
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

            sampleResource.SingleEpisodeExample = BuildSample(new List<Episode> { episode1 },
                                                              series,
                                                              episodeFile,
                                                              nameSpec);

            episodeFile.Path = @"C:\Test\Series.Title.S01E01-E02.720p.HDTV.x264-EVOLVE.mkv";

            sampleResource.MultiEpisodeExample = BuildSample(new List<Episode> { episode1, episode2 },
                                                             series,
                                                             episodeFile,
                                                             nameSpec);

            episodeFile.Path = @"C:\Test\Series.Title.2013.10.30.HDTV.x264-EVOLVE.mkv";
            series.SeriesType = SeriesTypes.Daily;

            sampleResource.DailyEpisodeExample = BuildSample(new List<Episode> { episode1 },
                                                             series,
                                                             episodeFile,
                                                             nameSpec);

            return sampleResource.AsResponse();
        }

        private string BuildSample(List<Episode> episodes, Core.Tv.Series series, EpisodeFile episodeFile, NamingConfig nameSpec)
        {
            try
            {
                //TODO: Validate the result is parsable
                return _buildFileNames.BuildFilename(episodes,
                                                     series,
                                                     episodeFile,
                                                     nameSpec);
            }
            catch (NamingFormatException ex)
            {
                //Catching to avoid blowing up all samples
                //TODO: Use validation to report error to client

                return String.Empty;
            }
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            if (!nameSpec.RenameEpisodes)
            {
                return;
            }

            var series = new Core.Tv.Series
            {
                SeriesType = SeriesTypes.Standard,
                Title = "Series Title"
            };

            var episode1 = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 1,
                Title = "Episode Title (1)",
                AirDate = "2013-10-30"
            };

            var episode2 = new Episode
            {
                SeasonNumber = 1,
                EpisodeNumber = 2,
                Title = "Episode Title (2)",
                AirDate = "2013-10-30"
            };

            var episodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p)
            };

            if (!ValidateStandardFormat(nameSpec, series, new List<Episode> { episode1 }, episodeFile))
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new ValidationFailure("StandardEpisodeFormat", "Results in unparsable filenames")
                }.ToArray());
            }

            if (!ValidateStandardFormat(nameSpec, series, new List<Episode> { episode1, episode2 }, episodeFile))
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new ValidationFailure("StandardEpisodeFormat", "Results in unparsable multi-episode filenames")
                }.ToArray());
            }

            if (!ValidateDailyFormat(nameSpec, series, episode1, episodeFile))
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new ValidationFailure("DailyEpisodeFormat", "Results in unparsable filenames")
                }.ToArray());
            }
        }

        private bool ValidateStandardFormat(NamingConfig nameSpec, Core.Tv.Series series, List<Episode> episodes, EpisodeFile episodeFile)
        {
            var filename = _buildFileNames.BuildFilename(episodes, series, episodeFile, nameSpec);
            var parsedEpisodeInfo = Parser.ParseTitle(filename);

            if (parsedEpisodeInfo == null)
            {
                return false;
            }

            return ValidateSeasonAndEpisodeNumbers(episodes, parsedEpisodeInfo);
        }

        private bool ValidateDailyFormat(NamingConfig nameSpec, Core.Tv.Series series, Episode episode, EpisodeFile episodeFile)
        {
            series.SeriesType = SeriesTypes.Daily;

            var filename = _buildFileNames.BuildFilename(new List<Episode> { episode }, series, episodeFile, nameSpec);
            var parsedEpisodeInfo = Parser.ParseTitle(filename);

            if (parsedEpisodeInfo == null)
            {
                return false;
            }

            if (parsedEpisodeInfo.IsDaily())
            {
                if (!parsedEpisodeInfo.AirDate.Equals(episode.AirDate))
                {
                    return false;
                }

                return true;
            }

            return ValidateSeasonAndEpisodeNumbers(new List<Episode> {episode}, parsedEpisodeInfo);
        }

        private bool ValidateSeasonAndEpisodeNumbers(List<Episode> episodes, ParsedEpisodeInfo parsedEpisodeInfo)
        {
            if (parsedEpisodeInfo.SeasonNumber != episodes.First().SeasonNumber ||
                !parsedEpisodeInfo.EpisodeNumbers.OrderBy(e => e).SequenceEqual(episodes.Select(e => e.EpisodeNumber).OrderBy(e => e)))
            {
                return false;
            }

            return true;
        }
    }
}