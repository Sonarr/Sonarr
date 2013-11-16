using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Nancy.Responses;
using NzbDrone.Api.REST;
using NzbDrone.Core.Organizer;
using Nancy.ModelBinding;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.Config
{
    public class NamingModule : NzbDroneRestModule<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;

        public NamingModule(INamingConfigService namingConfigService,
                            IFilenameSampleService filenameSampleService,
                            IFilenameValidationService filenameValidationService)
            : base("config/naming")
        {
            _namingConfigService = namingConfigService;
            _filenameSampleService = filenameSampleService;
            _filenameValidationService = filenameValidationService;
            GetResourceSingle = GetNamingConfig;
            GetResourceById = GetNamingConfig;
            UpdateResource = UpdateNamingConfig;

            Get["/samples"] = x => GetExamples(this.Bind<NamingConfigResource>());

            SharedValidator.RuleFor(c => c.MultiEpisodeStyle).InclusiveBetween(0, 3);

            SharedValidator.When(spec => spec.RenameEpisodes, () =>
            {
                SharedValidator.RuleFor(c => c.StandardEpisodeFormat).ValidEpisodeFormat();
                SharedValidator.RuleFor(c => c.DailyEpisodeFormat).ValidDailyEpisodeFormat();
            });
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
            //TODO: Validate that the format is valid
            var nameSpec = config.InjectTo<NamingConfig>();
            var sampleResource = new NamingSampleResource();
            
            var singleEpisodeSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
            var multiEpisodeSampleResult = _filenameSampleService.GetMultiEpisodeSample(nameSpec);
            var dailyEpisodeSampleResult = _filenameSampleService.GetDailySample(nameSpec);

            sampleResource.SingleEpisodeExample = _filenameValidationService.ValidateStandardFilename(singleEpisodeSampleResult) != null
                    ? "Invalid format"
                    : singleEpisodeSampleResult.Filename;

            sampleResource.MultiEpisodeExample = _filenameValidationService.ValidateStandardFilename(multiEpisodeSampleResult) != null
                    ? "Invalid format"
                    : multiEpisodeSampleResult.Filename;

            sampleResource.DailyEpisodeExample = _filenameValidationService.ValidateDailyFilename(dailyEpisodeSampleResult) != null
                    ? "Invalid format"
                    : dailyEpisodeSampleResult.Filename;

            return sampleResource.AsResponse();
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            var singleEpisodeSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
            var multiEpisodeSampleResult = _filenameSampleService.GetMultiEpisodeSample(nameSpec);
            var dailyEpisodeSampleResult = _filenameSampleService.GetDailySample(nameSpec);
            var singleEpisodeValidationResult = _filenameValidationService.ValidateStandardFilename(singleEpisodeSampleResult);
            var multiEpisodeValidationResult = _filenameValidationService.ValidateStandardFilename(multiEpisodeSampleResult);
            var dailyEpisodeValidationResult = _filenameValidationService.ValidateDailyFilename(dailyEpisodeSampleResult);

            var validationFailures = new List<ValidationFailure>();

            if (singleEpisodeValidationResult != null)
            {
                validationFailures.Add(singleEpisodeValidationResult);
            }

            if (multiEpisodeValidationResult != null)
            {
                validationFailures.Add(multiEpisodeValidationResult);
            }

            if (dailyEpisodeValidationResult != null)
            {
                validationFailures.Add(dailyEpisodeValidationResult);
            }

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.ToArray());
            }
        }
    }
}
