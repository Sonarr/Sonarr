using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using Nancy.ModelBinding;
using Sonarr.Http.Extensions;
using Sonarr.Http;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api.Config
{
    public class NamingConfigModule : SonarrRestModule<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;
        private readonly IBuildFileNames _filenameBuilder;

        public NamingConfigModule(INamingConfigService namingConfigService,
                            IFilenameSampleService filenameSampleService,
                            IFilenameValidationService filenameValidationService,
                            IBuildFileNames filenameBuilder)
            : base("config/naming")
        {
            _namingConfigService = namingConfigService;
            _filenameSampleService = filenameSampleService;
            _filenameValidationService = filenameValidationService;
            _filenameBuilder = filenameBuilder;
            GetResourceSingle = GetNamingConfig;
            GetResourceById = GetNamingConfig;
            UpdateResource = UpdateNamingConfig;

            Get["/samples"] = x => GetExamples(this.Bind<NamingConfigResource>());

            SharedValidator.RuleFor(c => c.MultiEpisodeStyle).InclusiveBetween(0, 5);
            SharedValidator.RuleFor(c => c.StandardEpisodeFormat).ValidEpisodeFormat();
            SharedValidator.RuleFor(c => c.DailyEpisodeFormat).ValidDailyEpisodeFormat();
            SharedValidator.RuleFor(c => c.AnimeEpisodeFormat).ValidAnimeEpisodeFormat();
            SharedValidator.RuleFor(c => c.SeriesFolderFormat).ValidSeriesFolderFormat();
            SharedValidator.RuleFor(c => c.SeasonFolderFormat).ValidSeasonFolderFormat();
        }

        private void UpdateNamingConfig(NamingConfigResource resource)
        {
            var nameSpec = resource.ToModel();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);
        }

        private NamingConfigResource GetNamingConfig()
        {
            var nameSpec = _namingConfigService.GetConfig();
            var resource = nameSpec.ToResource();

            if (resource.StandardEpisodeFormat.IsNotNullOrWhiteSpace())
            {
                var basicConfig = _filenameBuilder.GetBasicNamingConfig(nameSpec);
                basicConfig.AddToResource(resource);
            }

            return resource;
        }

        private NamingConfigResource GetNamingConfig(int id)
        {
            return GetNamingConfig();
        }

        private JsonResponse<NamingSampleResource> GetExamples(NamingConfigResource config)
        {
            var nameSpec = config.ToModel();
            var sampleResource = new NamingSampleResource();
            
            var singleEpisodeSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
            var multiEpisodeSampleResult = _filenameSampleService.GetMultiEpisodeSample(nameSpec);
            var dailyEpisodeSampleResult = _filenameSampleService.GetDailySample(nameSpec);
            var animeEpisodeSampleResult = _filenameSampleService.GetAnimeSample(nameSpec);
            var animeMultiEpisodeSampleResult = _filenameSampleService.GetAnimeMultiEpisodeSample(nameSpec);

            sampleResource.SingleEpisodeExample = _filenameValidationService.ValidateStandardFilename(singleEpisodeSampleResult) != null
                    ? "Invalid format"
                    : singleEpisodeSampleResult.FileName;

            sampleResource.MultiEpisodeExample = _filenameValidationService.ValidateStandardFilename(multiEpisodeSampleResult) != null
                    ? "Invalid format"
                    : multiEpisodeSampleResult.FileName;

            sampleResource.DailyEpisodeExample = _filenameValidationService.ValidateDailyFilename(dailyEpisodeSampleResult) != null
                    ? "Invalid format"
                    : dailyEpisodeSampleResult.FileName;

            sampleResource.AnimeEpisodeExample = _filenameValidationService.ValidateAnimeFilename(animeEpisodeSampleResult) != null
                    ? "Invalid format"
                    : animeEpisodeSampleResult.FileName;

            sampleResource.AnimeMultiEpisodeExample = _filenameValidationService.ValidateAnimeFilename(animeMultiEpisodeSampleResult) != null
                    ? "Invalid format"
                    : animeMultiEpisodeSampleResult.FileName;

            sampleResource.SeriesFolderExample = nameSpec.SeriesFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetSeriesFolderSample(nameSpec);

            sampleResource.SeasonFolderExample = nameSpec.SeasonFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetSeasonFolderSample(nameSpec);

            return sampleResource.AsResponse();
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            var singleEpisodeSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
            var multiEpisodeSampleResult = _filenameSampleService.GetMultiEpisodeSample(nameSpec);
            var dailyEpisodeSampleResult = _filenameSampleService.GetDailySample(nameSpec);
            var animeEpisodeSampleResult = _filenameSampleService.GetAnimeSample(nameSpec);
            var animeMultiEpisodeSampleResult = _filenameSampleService.GetAnimeMultiEpisodeSample(nameSpec);

            var singleEpisodeValidationResult = _filenameValidationService.ValidateStandardFilename(singleEpisodeSampleResult);
            var multiEpisodeValidationResult = _filenameValidationService.ValidateStandardFilename(multiEpisodeSampleResult);
            var dailyEpisodeValidationResult = _filenameValidationService.ValidateDailyFilename(dailyEpisodeSampleResult);
            var animeEpisodeValidationResult = _filenameValidationService.ValidateAnimeFilename(animeEpisodeSampleResult);
            var animeMultiEpisodeValidationResult = _filenameValidationService.ValidateAnimeFilename(animeMultiEpisodeSampleResult);

            var validationFailures = new List<ValidationFailure>();

            validationFailures.AddIfNotNull(singleEpisodeValidationResult);
            validationFailures.AddIfNotNull(multiEpisodeValidationResult);
            validationFailures.AddIfNotNull(dailyEpisodeValidationResult);
            validationFailures.AddIfNotNull(animeEpisodeValidationResult);
            validationFailures.AddIfNotNull(animeMultiEpisodeValidationResult);

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
            }
        }
    }
}
